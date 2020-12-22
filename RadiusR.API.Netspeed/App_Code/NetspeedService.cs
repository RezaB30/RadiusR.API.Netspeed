using RadiusR.DB;
using RadiusR.DB.Enums;
using RadiusR.DB.Utilities.Billing;
using RadiusR.VPOS;
using RezaB.API.WebService;
using RezaB.TurkTelekom.WebServices.Address;
using RezaB.TurkTelekom.WebServices.Availability;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class NetspeedService : INetspeedService
{
    string _password = "123456";
    TimeSpan _duration = new TimeSpan(0, 5, 0);
    readonly RadiusR.Address.AddressManager AddressClient = new RadiusR.Address.AddressManager();
    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetProvinces(BaseRequest<string, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetProvinces();
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<bool, SHA1> RegisterCustomerContact(BaseRequest<CustomerContactRequest, SHA1> baseRequest)
    {
        var password = string.Empty;
        try
        {
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                //get password from db
                password = _password;
                var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
                var IsValid = baseRequest.HasValidHash(passwordHash, _duration);
                if (IsValid)
                {
                    var description = $"{baseRequest.Data.FullName}{Environment.NewLine}{baseRequest.Data.PhoneNo}";
                    db.SupportRequests.Add(new RadiusR.DB.SupportRequest()
                    {
                        Date = DateTime.Now,
                        IsVisibleToCustomer = false,
                        StateID = (short)RadiusR.DB.Enums.SupportRequests.SupportRequestStateID.InProgress,
                        TypeID = baseRequest.Data.RequestTypeID,
                        SubTypeID = baseRequest.Data.RequestSubTypeID,
                        SupportPin = RadiusR.DB.RandomCode.CodeGenerator.GenerateSupportRequestPIN(),
                        SubscriptionID = null,
                        SupportRequestProgresses =
                        {
                            new RadiusR.DB.SupportRequestProgress()
                            {
                                Date = DateTime.Now,
                                IsVisibleToCustomer = false,
                                Message = description.Trim(new char[]{ ' ' , '\n' , '\r' }),
                                ActionType = (short)RadiusR.DB.Enums.SupportRequests.SupportRequestActionTypes.Create
                            }
                        }
                    });
                    db.SaveChanges();
                    return new BaseResponse<bool, SHA1>(passwordHash, baseRequest)
                    {
                        Culture = baseRequest.Culture,
                        ResponseMessage = CommonResponse<bool, SHA1>.SuccessResponse(baseRequest.Culture),
                        Data = true,
                        Username = baseRequest.Username
                    };
                }
                return CommonResponse<bool, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
        }
        catch (Exception ex)
        {
            return CommonResponse<bool, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<ServiceAvailabilityResponse, SHA1> ServiceAvailability(BaseRequest<ServiceAvailabilityRequest, SHA1> baseRequest)
    {
        var password = _password;
        try
        {
            var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<ServiceAvailabilityResponse, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                var domain = RadiusR.DB.DomainsCache.DomainsCache.GetDomainByID(1);
                var client = new AvailabilityServiceClient(domain.TelekomCredential.XDSLWebServiceUsernameInt, domain.TelekomCredential.XDSLWebServicePassword);
                var xdslTypeAdsl = AvailabilityServiceClient.XDSLType.ADSL;
                var xdslTypeVdsl = AvailabilityServiceClient.XDSLType.VDSL;
                var xdslTypeFiber = AvailabilityServiceClient.XDSLType.Fiber;
                var queryType = AvailabilityServiceClient.QueryType.BBK;
                List<Thread> threads = new List<Thread>();
                RezaB.TurkTelekom.WebServices.ServiceResponse<AvailabilityServiceClient.AvailabilityDescription> availabAdsl = null, availabVdsl = null, availabFiber = null;
                Thread threadAdsl = new Thread(() => { availabAdsl = client.Check(xdslTypeAdsl, queryType, baseRequest.Data.bbk); });
                Thread threadVdsl = new Thread(() => { availabVdsl = client.Check(xdslTypeVdsl, queryType, baseRequest.Data.bbk); });
                Thread threadFiber = new Thread(() => { availabFiber = client.Check(xdslTypeFiber, queryType, baseRequest.Data.bbk); });
                threadAdsl.Start();
                threadVdsl.Start();
                threadFiber.Start();
                threads.Add(threadAdsl);
                threads.Add(threadVdsl);
                threads.Add(threadFiber);
                foreach (var item in threads)
                {
                    item.Join();
                }
                bool HasInfrastructureAdsl = availabAdsl.InternalException != null ? false : availabAdsl.Data.Description.ErrorMessage == null ? availabAdsl.Data.Description.HasInfrastructure.Value : false;
                bool HasInfrastructureVdsl = availabVdsl.InternalException != null ? false : availabVdsl.Data.Description.ErrorMessage == null ? availabVdsl.Data.Description.HasInfrastructure.Value : false;
                bool HasInfrastructureFiber = availabFiber.InternalException != null ? false : availabFiber.Data.Description.ErrorMessage == null ? availabFiber.Data.Description.HasInfrastructure.Value : false;
                var speedAdsl = HasInfrastructureAdsl ? availabAdsl.Data.Description.DSLMaxSpeed.Value : 0;
                var speedVdsl = HasInfrastructureVdsl ? availabVdsl.Data.Description.DSLMaxSpeed.Value : 0;
                var speedFiber = HasInfrastructureFiber ? availabFiber.Data.Description.DSLMaxSpeed.Value : 0;
                AddressServiceClient addressServiceClient = new AddressServiceClient(domain.TelekomCredential.XDSLWebServiceUsernameInt, domain.TelekomCredential.XDSLWebServicePassword);
                var address = addressServiceClient.GetAddressFromCode(Convert.ToInt64(baseRequest.Data.bbk));
                return new BaseResponse<ServiceAvailabilityResponse, SHA1>(passwordHash, baseRequest)
                {
                    Culture = baseRequest.Culture,
                    Username = baseRequest.Username,
                    ResponseMessage = CommonResponse<ServiceAvailabilityResponse, SHA1>.SuccessResponse(baseRequest.Culture),
                    Data = new ServiceAvailabilityResponse()
                    {
                        address = address.InternalException == null ? address.Data.AddressText : "-",
                        HasInfrastructureAdsl = HasInfrastructureAdsl,
                        HasInfrastructureVdsl = HasInfrastructureVdsl,
                        HasInfrastructureFiber = HasInfrastructureFiber,
                        AdslDistance = availabAdsl.InternalException == null ? availabAdsl.Data.Description.Distance : null,
                        VdslDistance = availabVdsl.InternalException == null ? availabVdsl.Data.Description.Distance : null,
                        FiberDistance = availabFiber.InternalException == null ? availabFiber.Data.Description.Distance : null,
                        AdslPortState = availabAdsl.InternalException == null ? availabAdsl.Data.Description.PortState : AvailabilityServiceClient.PortState.NotAvailable,
                        VdslPortState = availabVdsl.InternalException == null ? availabVdsl.Data.Description.PortState : AvailabilityServiceClient.PortState.NotAvailable,
                        FiberPortState = availabFiber.InternalException == null ? availabFiber.Data.Description.PortState : AvailabilityServiceClient.PortState.NotAvailable,
                        AdslSpeed = availabAdsl.InternalException == null ? availabAdsl.Data.Description.DSLMaxSpeed : null,
                        VdslSpeed = availabVdsl.InternalException == null ? availabVdsl.Data.Description.DSLMaxSpeed : null,
                        FiberSpeed = availabFiber.InternalException == null ? availabFiber.Data.Description.DSLMaxSpeed : null,
                        AdslSVUID = availabAdsl.InternalException == null ? availabAdsl.Data.Description.SVUID : "-",
                        VdslSVUID = availabVdsl.InternalException == null ? availabVdsl.Data.Description.SVUID : "-",
                        FiberSVUID = availabFiber.InternalException == null ? availabFiber.Data.Description.SVUID : "-",
                        BBK = baseRequest.Data.bbk
                    }
                };
            }

        }
        catch (Exception ex)
        {
            return CommonResponse<ServiceAvailabilityResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetProvinceDistricts(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetProvinceDistricts(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetDistrictRuralRegions(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetDistrictRuralRegions(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetRuralRegionNeighbourhoods(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetRuralRegionNeighbourhoods(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetNeighbourhoodStreets(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetNeighbourhoodStreets(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetStreetBuildings(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetStreetBuildings(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetBuildingApartments(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetBuildingApartments(baseRequest.Data);
            return new BaseResponse<IEnumerable<ValueNamePair>, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse<IEnumerable<ValueNamePair>, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<ValueNamePair>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }
    public BaseResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1> GetApartmentAddress(BaseRequest<long, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            var result = AddressClient.GetApartmentAddress(baseRequest.Data);
            return new BaseResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1>(passwordHash, baseRequest)
            {
                Culture = baseRequest.Culture,
                Data = result.Data,
                ResponseMessage = CommonResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1>.SuccessResponse(baseRequest.Culture),
                Username = baseRequest.Username
            };
        }
        catch (Exception)
        {
            return CommonResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1> SubscriberGetBills(BaseRequest<SubscriberGetBillsRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                if (baseRequest.HasValidHash(passwordHash, _duration))
                {
                    if (baseRequest.Data.PhoneNo.StartsWith("0"))
                    {
                        baseRequest.Data.PhoneNo = baseRequest.Data.PhoneNo.TrimStart('0');
                    }
                    var dbClient = db.Subscriptions.Where(s => s.SubscriberNo == baseRequest.Data.SubscriberNo && s.Customer.ContactPhoneNo == baseRequest.Data.PhoneNo).FirstOrDefault();
                    if (dbClient == null)
                        return CommonResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>.SubscriberNotFoundErrorResponse(passwordHash, baseRequest);
                    var firstUnpaidBill = dbClient.Bills.Where(bill => bill.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid).OrderBy(bill => bill.IssueDate).FirstOrDefault();
                    var result = dbClient.Bills.OrderByDescending(bill => bill.IssueDate).Select(bill =>
                    new SubscriberGetBillsResponse()
                    {
                        BillDate = bill.IssueDate,
                        LastPaymentDate = bill.DueDate,
                        Status = bill.BillStatusID,
                        ID = bill.ID,
                        ServiceName = bill.BillFees.Any(bf => bf.FeeTypeID == (short)RadiusR.DB.Enums.FeeType.Tariff) ? bill.BillFees.FirstOrDefault(bf => bf.FeeTypeID == (short)RadiusR.DB.Enums.FeeType.Tariff).Description : "-",
                        CanBePaid = firstUnpaidBill != null && bill.ID == firstUnpaidBill.ID,
                        HasEArchiveBill = bill.EBill != null && bill.EBill.EBillType == (short)RadiusR.DB.Enums.EBillType.EArchive,
                        Total = bill.GetPayableCost().ToString("###,##0.00"),
                    }
                    );
                    return new BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>(passwordHash, baseRequest)
                    {
                        Culture = baseRequest.Culture,
                        Data = result.ToArray(),
                        ResponseMessage = CommonResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>.SuccessResponse(baseRequest.Culture)
                    };
                }
                return CommonResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
        }
        catch (Exception)
        {
            return CommonResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>.InternalException(passwordHash, baseRequest);
        }
    }

    public BaseResponse<SubscriberPayBillsResponse, SHA1> SubscriberPayBills(BaseRequest<SubscriberPayBillsRequest, SHA1> baseRequest)
    {
        //RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionForExistingCustomer()
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        //try
        //{
        //    if (!baseRequest.HasValidHash(passwordHash, _duration))
        //    {
        //        return CommonResponse<SubscriberPayBillsResponse, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
        //    }
        //    using (RadiusREntities db = new RadiusREntities())
        //    {
        //        var dbSubscription = db.Subscriptions.Where(s => s.SubscriberNo == baseRequest.Data.SubscriberNo).FirstOrDefault();
        //        if(dbSubscription == null)
        //            return CommonResponse<SubscriberPayBillsResponse, SHA1>.SubscriberNotFoundErrorResponse(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        //        var payableAmount = GetPayableAmount(dbSubscription, baseRequest.Data.id);
        //        if (payableAmount == 0)
        //            //payable bill not found
        //            return CommonResponse<SubscriberPayBillsResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);

        //        var tokenKey = VPOSTokenManager.RegisterPaymentToken(new BillPaymentToken()
        //        {
        //            SubscriberId = dbSubscription.ID,
        //            BillID = baseRequest.Data.id
        //        });

        //        var VPOSModel = VPOSManager.GetVPOSModel(
        //            new System.Web.Mvc.UrlHelper().Action("VPOSSuccess", null, new { id = tokenKey }, System.Web.HttpContext.Current.Request.Url.Scheme),
        //            new System.Web.Mvc.UrlHelper().Action("VPOSFail", null, new { id = tokenKey }, System.Web.HttpContext.Current.Request.Url.Scheme),
        //            payableAmount,
        //            dbSubscription.Customer.Culture.Split('-').FirstOrDefault(),
        //            dbSubscription.SubscriberNo + "-" + dbSubscription.ValidDisplayName);
        //        var helper = new System.Web.Mvc.HtmlHelper<RezaB.Web.VPOS.VPOS3DHostModel>(VPOSModel);
        //        var html = RezaB.Web.VPOS.VPOS3DHostHelper.VPOS3DHostFormFor(,VPOSModel);
        //    }
        //}
        //catch (Exception)
        //{
        //    return CommonResponse<SubscriberPayBillsResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        //}
        return CommonResponse<SubscriberPayBillsResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
    }
    private decimal GetPayableAmount(Subscription dbSubscription, long? billId)
    {
        // pre-paid sub
        if (!dbSubscription.HasBilling)
        {
            return dbSubscription.GetSubscriberPackageExtentionUnitPrice();
        }
        // billed sub
        var creditsAmount = dbSubscription.SubscriptionCredits.Sum(credit => credit.Amount);
        var bills = dbSubscription.Bills.Where(bill => bill.BillStatusID == (short)BillState.Unpaid).OrderBy(bill => bill.IssueDate).AsEnumerable();
        if (billId.HasValue)
            bills = bills.Where(bill => bill.ID == billId.Value);
        if (!bills.Any())
            return 0m;

        var billsAmount = bills.Sum(bill => bill.GetPayableCost());
        if (!dbSubscription.HasBilling)
        {
            billsAmount = dbSubscription.Service.Price;
        }

        return Math.Max(0m, billsAmount - creditsAmount);
    }

    public BaseResponse<ILookup<string, string>, SHA1> NewCustomerRegister(BaseRequest<NewCustomerRegisterRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<ILookup<string, string>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                var registeredCustomer = new Customer();
                var result = RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionWithNewCustomer(db, baseRequest.Data.Registration, out registeredCustomer);
                if (result == null)
                {
                    return new BaseResponse<ILookup<string, string>, SHA1>(passwordHash, baseRequest)
                    {
                        Data = result,
                        Culture = baseRequest.Culture,
                        ResponseMessage = CommonResponse<ILookup<string, string>, SHA1>.SuccessResponse(baseRequest.Culture),
                        Username = baseRequest.Username
                    };
                }
                return CommonResponse<ILookup<string, string>, SHA1>.Failed(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
            }

        }
        catch (Exception)
        {
            return CommonResponse<ILookup<string, string>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }
}
