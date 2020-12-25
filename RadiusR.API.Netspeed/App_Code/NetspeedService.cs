using Newtonsoft.Json;
using RadiusR.DB;
using RadiusR.DB.Enums;
using RadiusR.DB.Utilities.Billing;
using RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration;
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
using System.Web.WebPages.Html;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using RezaB.Web.VPOS;
using NLog;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class NetspeedService : INetspeedService
{
    string _password = "123456";
    TimeSpan _duration = new TimeSpan(0, 5, 0);
    readonly RadiusR.Address.AddressManager AddressClient = new RadiusR.Address.AddressManager();
    Logger Errorslogger = LogManager.GetLogger("Errors");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Provinces");
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
            Errorslogger.Error(ex, "Error Register Customer Contact");
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
            Errorslogger.Error(ex, "Error Service Availability");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Province Dsitricts");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get District Rural Regions");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Rural Region Neighbourhoods");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Neighbourhood Streets");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Street Buildings");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Building Apartments");
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Apartment Address");
            return CommonResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1> GetBills(BaseRequest<SubscriberGetBillsRequest, SHA1> baseRequest)
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
                    var result = dbClient.Bills.Where(bill => bill.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid).OrderBy(bill => bill.IssueDate).Select(bill =>
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
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Subscriber unpaid bill list");
            return CommonResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>.InternalException(passwordHash, baseRequest);
        }
    }

    public BaseResponse<SubscriberPayBillsResponse, SHA1> SubscriberPaymentVPOS(BaseRequest<SubscriberPayBillsRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<SubscriberPayBillsResponse, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (RadiusREntities db = new RadiusREntities())
            {
                var dbSubscription = db.Subscriptions.Where(s => s.SubscriberNo == baseRequest.Data.SubscriberNo).FirstOrDefault();
                if (dbSubscription == null)
                    return CommonResponse<SubscriberPayBillsResponse, SHA1>.SubscriberNotFoundErrorResponse(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
                var VPOSModel = VPOSManager.GetVPOSModel(
                    baseRequest.Data.OkUrl,
                    baseRequest.Data.FailUrl,
                    baseRequest.Data.PayableAmount,
                    dbSubscription.Customer.Culture.Split('-').FirstOrDefault(),
                    dbSubscription.SubscriberNo + "-" + dbSubscription.ValidDisplayName);
                var htmlForm = VPOSModel.GetHtmlForm().ToHtmlString();
                return new BaseResponse<SubscriberPayBillsResponse, SHA1>(passwordHash, baseRequest)
                {
                    Data = new SubscriberPayBillsResponse()
                    {
                        HtmlForm = htmlForm,
                    },
                    Culture = baseRequest.Culture,
                    ResponseMessage = CommonResponse<SubscriberPayBillsResponse, SHA1>.SuccessResponse(baseRequest.Culture),
                    Username = baseRequest.Username
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Subscriber payment VPOS");
            return CommonResponse<SubscriberPayBillsResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest, ex);
        }
    }

    public BaseResponse<Dictionary<string, string>, SHA1> NewCustomerRegister(BaseRequest<NewCustomerRegisterRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<Dictionary<string, string>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                var registeredCustomer = new Customer();
                var register = baseRequest.Data;
                CustomerRegistrationInfo registrationInfo = new CustomerRegistrationInfo()
                {
                    CorporateInfo = register.CorporateCustomerInfo == null ? null : new CustomerRegistrationInfo.CorporateCustomerInfo()
                    {
                        CentralSystemNo = register.CorporateCustomerInfo.CentralSystemNo,
                        CompanyAddress = register.CorporateCustomerInfo.CompanyAddress == null ? null : new CustomerRegistrationInfo.AddressInfo()
                        {
                            AddressNo = register.CorporateCustomerInfo.CompanyAddress.AddressNo,
                            AddressText = register.CorporateCustomerInfo.CompanyAddress.AddressText,
                            ApartmentID = register.CorporateCustomerInfo.CompanyAddress.ApartmentID,
                            ApartmentNo = register.CorporateCustomerInfo.CompanyAddress.ApartmentNo,
                            DistrictID = register.CorporateCustomerInfo.CompanyAddress.DistrictID,
                            DistrictName = register.CorporateCustomerInfo.CompanyAddress.DistrictName,
                            DoorID = register.CorporateCustomerInfo.CompanyAddress.DoorID,
                            DoorNo = register.CorporateCustomerInfo.CompanyAddress.DoorNo,
                            Floor = register.CorporateCustomerInfo.CompanyAddress.Floor,
                            NeighbourhoodID = register.CorporateCustomerInfo.CompanyAddress.NeighbourhoodID,
                            NeighbourhoodName = register.CorporateCustomerInfo.CompanyAddress.NeighbourhoodName,
                            PostalCode = register.CorporateCustomerInfo.CompanyAddress.PostalCode,
                            ProvinceID = register.CorporateCustomerInfo.CompanyAddress.ProvinceID,
                            ProvinceName = register.CorporateCustomerInfo.CompanyAddress.ProvinceName,
                            RuralCode = register.CorporateCustomerInfo.CompanyAddress.RuralCode,
                            StreetID = register.CorporateCustomerInfo.CompanyAddress.StreetID,
                            StreetName = register.CorporateCustomerInfo.CompanyAddress.StreetName
                        },
                        ExecutiveBirthPlace = register.CorporateCustomerInfo.ExecutiveBirthPlace,
                        ExecutiveFathersName = register.CorporateCustomerInfo.ExecutiveFathersName,
                        ExecutiveMothersMaidenName = register.CorporateCustomerInfo.ExecutiveMothersMaidenName,
                        ExecutiveMothersName = register.CorporateCustomerInfo.ExecutiveMothersName,
                        ExecutiveNationality = register.CorporateCustomerInfo.ExecutiveNationality,
                        ExecutiveProfession = register.CorporateCustomerInfo.ExecutiveProfession,
                        ExecutiveResidencyAddress = register.CorporateCustomerInfo.ExecutiveResidencyAddress == null ? null : new CustomerRegistrationInfo.AddressInfo()
                        {
                            AddressNo = register.CorporateCustomerInfo.ExecutiveResidencyAddress.AddressNo,
                            AddressText = register.CorporateCustomerInfo.ExecutiveResidencyAddress.AddressText,
                            ApartmentID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.ApartmentID,
                            ApartmentNo = register.CorporateCustomerInfo.ExecutiveResidencyAddress.ApartmentNo,
                            DistrictID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.DistrictID,
                            DistrictName = register.CorporateCustomerInfo.ExecutiveResidencyAddress.DistrictName,
                            DoorID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.DoorID,
                            DoorNo = register.CorporateCustomerInfo.ExecutiveResidencyAddress.DoorNo,
                            Floor = register.CorporateCustomerInfo.ExecutiveResidencyAddress.Floor,
                            NeighbourhoodID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.NeighbourhoodID,
                            NeighbourhoodName = register.CorporateCustomerInfo.ExecutiveResidencyAddress.NeighbourhoodName,
                            PostalCode = register.CorporateCustomerInfo.ExecutiveResidencyAddress.PostalCode,
                            ProvinceID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.ProvinceID,
                            ProvinceName = register.CorporateCustomerInfo.ExecutiveResidencyAddress.ProvinceName,
                            StreetID = register.CorporateCustomerInfo.ExecutiveResidencyAddress.StreetID,
                            StreetName = register.CorporateCustomerInfo.ExecutiveResidencyAddress.StreetName,
                            RuralCode = register.CorporateCustomerInfo.ExecutiveResidencyAddress.RuralCode,
                        },
                        ExecutiveSex = register.CorporateCustomerInfo.ExecutiveSex,
                        TaxNo = register.CorporateCustomerInfo.TaxNo,
                        TaxOffice = register.CorporateCustomerInfo.TaxOffice,
                        Title = register.CorporateCustomerInfo.Title,
                        TradeRegistrationNo = register.CorporateCustomerInfo.TradeRegistrationNo
                    },
                    GeneralInfo = register.CustomerGeneralInfo == null ? null : new CustomerRegistrationInfo.CustomerGeneralInfo()
                    {
                        BillingAddress = register.CustomerGeneralInfo.BillingAddress == null ? null : new CustomerRegistrationInfo.AddressInfo()
                        {
                            AddressNo = register.CustomerGeneralInfo.BillingAddress.AddressNo,
                            AddressText = register.CustomerGeneralInfo.BillingAddress.AddressText,
                            ApartmentID = register.CustomerGeneralInfo.BillingAddress.ApartmentID,
                            ApartmentNo = register.CustomerGeneralInfo.BillingAddress.ApartmentNo,
                            DistrictID = register.CustomerGeneralInfo.BillingAddress.DistrictID,
                            DistrictName = register.CustomerGeneralInfo.BillingAddress.DistrictName,
                            DoorID = register.CustomerGeneralInfo.BillingAddress.DoorID,
                            DoorNo = register.CustomerGeneralInfo.BillingAddress.DoorNo,
                            Floor = register.CustomerGeneralInfo.BillingAddress.Floor,
                            NeighbourhoodID = register.CustomerGeneralInfo.BillingAddress.NeighbourhoodID,
                            NeighbourhoodName = register.CustomerGeneralInfo.BillingAddress.NeighbourhoodName,
                            PostalCode = register.CustomerGeneralInfo.BillingAddress.PostalCode,
                            ProvinceID = register.CustomerGeneralInfo.BillingAddress.ProvinceID,
                            ProvinceName = register.CustomerGeneralInfo.BillingAddress.ProvinceName,
                            RuralCode = register.CustomerGeneralInfo.BillingAddress.RuralCode,
                            StreetID = register.CustomerGeneralInfo.BillingAddress.StreetID,
                            StreetName = register.CustomerGeneralInfo.BillingAddress.StreetName
                        },
                        ContactPhoneNo = register.CustomerGeneralInfo.ContactPhoneNo,
                        Culture = register.CustomerGeneralInfo.Culture,
                        CustomerType = register.CustomerGeneralInfo.CustomerType,
                        Email = register.CustomerGeneralInfo.Email,
                        OtherPhoneNos = register.CustomerGeneralInfo.OtherPhoneNos == null ? null : register.CustomerGeneralInfo.OtherPhoneNos.Select(p => new CustomerRegistrationInfo.PhoneNoListItem()
                        {
                            Number = p.Number
                        })
                    },
                    IDCard = register.IDCardInfo == null ? null : new CustomerRegistrationInfo.IDCardInfo()
                    {
                        BirthDate = register.IDCardInfo.BirthDate,
                        CardType = register.IDCardInfo.CardType,
                        DateOfIssue = register.IDCardInfo.DateOfIssue,
                        District = register.IDCardInfo.District,
                        FirstName = register.IDCardInfo.FirstName,
                        LastName = register.IDCardInfo.LastName,
                        Neighbourhood = register.IDCardInfo.Neighbourhood,
                        PageNo = register.IDCardInfo.PageNo,
                        PassportNo = register.IDCardInfo.PassportNo,
                        PlaceOfIssue = register.IDCardInfo.PlaceOfIssue,
                        Province = register.IDCardInfo.Province,
                        RowNo = register.IDCardInfo.RowNo,
                        SerialNo = register.IDCardInfo.SerialNo,
                        TCKNo = register.IDCardInfo.TCKNo,
                        VolumeNo = register.IDCardInfo.VolumeNo
                    },
                    IndividualInfo = register.IndividualCustomerInfo == null ? null : new CustomerRegistrationInfo.IndividualCustomerInfo()
                    {
                        BirthPlace = register.IndividualCustomerInfo.BirthPlace,
                        FathersName = register.IndividualCustomerInfo.FathersName,
                        MothersMaidenName = register.IndividualCustomerInfo.MothersMaidenName,
                        MothersName = register.IndividualCustomerInfo.MothersName,
                        Nationality = register.IndividualCustomerInfo.Nationality,
                        Profession = register.IndividualCustomerInfo.Profession,
                        ResidencyAddress = register.IndividualCustomerInfo.ResidencyAddress == null ? null : new CustomerRegistrationInfo.AddressInfo()
                        {
                            AddressNo = register.IndividualCustomerInfo.ResidencyAddress.AddressNo,
                            AddressText = register.IndividualCustomerInfo.ResidencyAddress.AddressText,
                            ApartmentID = register.IndividualCustomerInfo.ResidencyAddress.ApartmentID,
                            ApartmentNo = register.IndividualCustomerInfo.ResidencyAddress.ApartmentNo,
                            DistrictID = register.IndividualCustomerInfo.ResidencyAddress.DistrictID,
                            DistrictName = register.IndividualCustomerInfo.ResidencyAddress.DistrictName,
                            DoorID = register.IndividualCustomerInfo.ResidencyAddress.DoorID,
                            DoorNo = register.IndividualCustomerInfo.ResidencyAddress.DoorNo,
                            Floor = register.IndividualCustomerInfo.ResidencyAddress.Floor,
                            NeighbourhoodID = register.IndividualCustomerInfo.ResidencyAddress.NeighbourhoodID,
                            NeighbourhoodName = register.IndividualCustomerInfo.ResidencyAddress.NeighbourhoodName,
                            PostalCode = register.IndividualCustomerInfo.ResidencyAddress.PostalCode,
                            ProvinceID = register.IndividualCustomerInfo.ResidencyAddress.ProvinceID,
                            ProvinceName = register.IndividualCustomerInfo.ResidencyAddress.ProvinceName,
                            RuralCode = register.IndividualCustomerInfo.ResidencyAddress.RuralCode,
                            StreetID = register.IndividualCustomerInfo.ResidencyAddress.StreetID,
                            StreetName = register.IndividualCustomerInfo.ResidencyAddress.StreetName
                        },
                        Sex = register.IndividualCustomerInfo.Sex
                    },
                    SubscriptionInfo = register.SubscriptionInfo == null ? null : new CustomerRegistrationInfo.SubscriptionRegistrationInfo()
                    {
                        DomainID = register.SubscriptionInfo.DomainID,
                        ServiceID = register.SubscriptionInfo.ServiceID,
                        SetupAddress = new CustomerRegistrationInfo.AddressInfo()
                        {
                            AddressNo = register.SubscriptionInfo.SetupAddress.AddressNo,
                            AddressText = register.SubscriptionInfo.SetupAddress.AddressText,
                            ApartmentID = register.SubscriptionInfo.SetupAddress.ApartmentID,
                            ApartmentNo = register.SubscriptionInfo.SetupAddress.ApartmentNo,
                            DistrictID = register.SubscriptionInfo.SetupAddress.DistrictID,
                            DistrictName = register.SubscriptionInfo.SetupAddress.DistrictName,
                            DoorID = register.SubscriptionInfo.SetupAddress.DoorID,
                            DoorNo = register.SubscriptionInfo.SetupAddress.DoorNo,
                            Floor = register.SubscriptionInfo.SetupAddress.Floor,
                            NeighbourhoodID = register.SubscriptionInfo.SetupAddress.NeighbourhoodID,
                            NeighbourhoodName = register.SubscriptionInfo.SetupAddress.NeighbourhoodName,
                            PostalCode = register.SubscriptionInfo.SetupAddress.PostalCode,
                            ProvinceID = register.SubscriptionInfo.SetupAddress.ProvinceID,
                            ProvinceName = register.SubscriptionInfo.SetupAddress.ProvinceName,
                            RuralCode = register.SubscriptionInfo.SetupAddress.RuralCode,
                            StreetID = register.SubscriptionInfo.SetupAddress.StreetID,
                            StreetName = register.SubscriptionInfo.SetupAddress.StreetName
                        },
                        BillingPeriod = register.SubscriptionInfo.BillingPeriod
                    },
                };
                var result = RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionWithNewCustomer(db, registrationInfo, out registeredCustomer);
                Dictionary<string, string> valuePairs = new Dictionary<string, string>();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        valuePairs.Add(item.Key, item.FirstOrDefault());
                    }
                    return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, baseRequest)
                    {
                        Data = valuePairs,
                        Culture = baseRequest.Culture,
                        ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.FailedResponse(baseRequest.Culture),
                        Username = baseRequest.Username
                    };
                }
                db.Customers.Add(registeredCustomer);
                db.SaveChanges();
                return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, baseRequest)
                {
                    Data = valuePairs,
                    Culture = baseRequest.Culture,
                    ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.SuccessResponse(baseRequest.Culture),
                    Username = baseRequest.Username
                };
            }
        }
        catch (NullReferenceException ex)
        {
            Errorslogger.Error(ex, "Error Null Reference Exception");
            return CommonResponse<Dictionary<string, string>, SHA1>.NullObjectException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get new customer register");
            return CommonResponse<Dictionary<string, string>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<Dictionary<string, string>, SHA1> ExistingCustomerRegister(BaseRequest<ExistingCustomerRegisterRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<Dictionary<string, string>, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                var referenceCustomer = db.Subscriptions.Find(baseRequest.Data.SubscriberID);
                if (referenceCustomer == null)
                    return CommonResponse<Dictionary<string, string>, SHA1>.SubscriberNotFoundErrorResponse(passwordHash, baseRequest);
                var result = RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionForExistingCustomer(db, new CustomerRegistrationInfo.SubscriptionRegistrationInfo(), referenceCustomer.Customer);
                Dictionary<string, string> valuePairs = null;
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        valuePairs.Add(item.Key, item.FirstOrDefault());
                    }
                    return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, baseRequest)
                    {
                        Data = valuePairs,
                        Culture = baseRequest.Culture,
                        ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.FailedResponse(baseRequest.Culture),
                        Username = baseRequest.Username
                    };
                }
                return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, baseRequest)
                {
                    Data = valuePairs,
                    Culture = baseRequest.Culture,
                    ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.SuccessResponse(baseRequest.Culture),
                    Username = baseRequest.Username
                };
            }
        }
        catch (NullReferenceException ex)
        {
            Errorslogger.Error(ex, "Error Null Reference Exception");
            return CommonResponse<Dictionary<string, string>, SHA1>.NullObjectException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get new customer register");
            return CommonResponse<Dictionary<string, string>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
        }
    }

    public BaseResponse<PayBillsResponse, SHA1> PayBills(BaseRequest<PayBillsRequest, SHA1> baseRequest)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!baseRequest.HasValidHash(passwordHash, _duration))
            {
                return CommonResponse<PayBillsResponse, SHA1>.UnauthorizedResponse(passwordHash, baseRequest);
            }
            using (RadiusREntities db = new RadiusREntities())
            {
                if (baseRequest.Data.BillIds == null)
                    return CommonResponse<PayBillsResponse, SHA1>.BillsNotFoundException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);
                if (baseRequest.Data.BillIds.Count() == 0)
                    return CommonResponse<PayBillsResponse, SHA1>.BillsNotFoundException(HashUtilities.CalculateHash<SHA1>(password), baseRequest);

                //var PayableBills = db.Bills.OrderBy(bill => bill.IssueDate).Take(baseRequest.Data.BillIds.Count()).Select(bill => bill.ID).ToArray();
                //var IsPayable = true;
                //foreach (var bill in PayableBills)
                //{
                //    if (!baseRequest.Data.BillIds.Contains(bill))
                //    {
                //        IsPayable = false;
                //    }
                //}
                //if (IsPayable )
                //{

                //}
                var Bills = db.Bills.Where(bill => baseRequest.Data.BillIds.Contains(bill.ID)).ToArray();
                var payResponse = RadiusR.DB.Utilities.Billing.BillPayment.PayBills(db, Bills, PaymentType.VirtualPos, BillPayment.AccountantType.Seller);
                db.SaveChanges();
                return new BaseResponse<PayBillsResponse, SHA1>(passwordHash, baseRequest)
                {
                    Data = new PayBillsResponse()
                    {
                        PaymentResponse = payResponse
                    },
                    Culture = baseRequest.Culture,
                    ResponseMessage = CommonResponse<PayBillsResponse, SHA1>.SuccessResponse(baseRequest.Culture),
                    Username = baseRequest.Username
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error pay bills");
            return CommonResponse<PayBillsResponse, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), baseRequest, ex);
        }
    }
}
