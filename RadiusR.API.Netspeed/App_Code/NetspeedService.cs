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
using System.Globalization;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in code, svc and config file together.
public class NetspeedService : INetspeedService
{
    string _password = "123456";
    TimeSpan _duration = new TimeSpan(0, 5, 0);
    readonly RadiusR.Address.AddressManager AddressClient = new RadiusR.Address.AddressManager();
    Logger Errorslogger = LogManager.GetLogger("Errors");
    public NetspeedServiceArrayListResponse GetProvinces(NetspeedServiceRequests request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Username = request.Username
                };
            }
            var result = AddressClient.GetProvinces();
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Provinces");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceRegisterCustomerContactResponse RegisterCustomerContact(NetspeedServiceCustomerContactRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                //get password from db                
                var IsValid = request.HasValidHash(passwordHash, _duration);
                if (IsValid)
                {
                    var description = $"{request.Data.FullName}{Environment.NewLine}{request.Data.PhoneNo}";
                    db.SupportRequests.Add(new RadiusR.DB.SupportRequest()
                    {
                        Date = DateTime.Now,
                        IsVisibleToCustomer = false,
                        StateID = (short)RadiusR.DB.Enums.SupportRequests.SupportRequestStateID.InProgress,
                        TypeID = request.Data.RequestTypeID,
                        SubTypeID = request.Data.RequestSubTypeID,
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
                    return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                    {
                        Culture = request.Culture,
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                        Data = true,
                        Username = request.Username
                    };
                }
                return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = false,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Register Customer Contact");
            return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = false,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceServiceAvailabilityResponse ServiceAvailability(NetspeedServiceServiceAvailabilityRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
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
                Thread threadAdsl = new Thread(() => { availabAdsl = client.Check(xdslTypeAdsl, queryType, request.Data.bbk); });
                Thread threadVdsl = new Thread(() => { availabVdsl = client.Check(xdslTypeVdsl, queryType, request.Data.bbk); });
                Thread threadFiber = new Thread(() => { availabFiber = client.Check(xdslTypeFiber, queryType, request.Data.bbk); });
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
                var address = addressServiceClient.GetAddressFromCode(Convert.ToInt64(request.Data.bbk));
                return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    Data = new ServiceAvailabilityResponse()
                    {
                        address = address.InternalException == null ? address.Data.AddressText : "-",
                        HasInfrastructureAdsl = HasInfrastructureAdsl,
                        HasInfrastructureVdsl = HasInfrastructureVdsl,
                        HasInfrastructureFiber = HasInfrastructureFiber,
                        AdslDistance = availabAdsl.InternalException == null ? availabAdsl.Data.Description.Distance : null,
                        VdslDistance = availabVdsl.InternalException == null ? availabVdsl.Data.Description.Distance : null,
                        FiberDistance = availabFiber.InternalException == null ? availabFiber.Data.Description.Distance : null,
                        AdslPortState = availabAdsl.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabAdsl.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                        VdslPortState = availabVdsl.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabVdsl.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                        FiberPortState = availabFiber.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabFiber.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                        AdslSpeed = availabAdsl.InternalException == null ? availabAdsl.Data.Description.DSLMaxSpeed : null,
                        VdslSpeed = availabVdsl.InternalException == null ? availabVdsl.Data.Description.DSLMaxSpeed : null,
                        FiberSpeed = availabFiber.InternalException == null ? availabFiber.Data.Description.DSLMaxSpeed : null,
                        AdslSVUID = availabAdsl.InternalException == null ? availabAdsl.Data.Description.SVUID : "-",
                        VdslSVUID = availabVdsl.InternalException == null ? availabVdsl.Data.Description.SVUID : "-",
                        FiberSVUID = availabFiber.InternalException == null ? availabFiber.Data.Description.SVUID : "-",
                        BBK = request.Data.bbk
                    }
                };
            }

        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Service Availability");
            return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
            {
                Culture = request.Culture,
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Data = null,
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetProvinceDistricts(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetProvinceDistricts(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Province Dsitricts");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetDistrictRuralRegions(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetDistrictRuralRegions(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get District Rural Regions");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetRuralRegionNeighbourhoods(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetRuralRegionNeighbourhoods(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Rural Region Neighbourhoods");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetNeighbourhoodStreets(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetNeighbourhoodStreets(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Neighbourhood Streets");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetStreetBuildings(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetStreetBuildings(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Street Buildings");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceArrayListResponse GetBuildingApartments(NetspeedServiceArrayListRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    Data = null,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                };
            }
            var result = AddressClient.GetBuildingApartments(request.Data);
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                {
                    Code = p.Code,
                    Name = p.Name
                }) : Enumerable.Empty<ValueNamePair>(),
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Building Apartments");
            return new NetspeedServiceArrayListResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture),
                Username = request.Username
            };
        }
    }
    public NetspeedServiceAddressDetailsResponse GetApartmentAddress(NetspeedServiceAddressDetailsRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    Username = request.Username,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Data = null
                };
            }
            var result = AddressClient.GetApartmentAddress(request.Data);
            return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = new AddressDetailsResponse()
                {
                    AddressNo = result.Data.AddressNo,
                    AddressText = result.Data.AddressText,
                    ApartmentID = result.Data.ApartmentID,
                    ApartmentNo = result.Data.ApartmentNo,
                    DistrictID = result.Data.DistrictID,
                    DistrictName = result.Data.DistrictName,
                    DoorID = result.Data.DoorID,
                    DoorNo = result.Data.DoorNo,
                    NeighbourhoodID = result.Data.NeighbourhoodID,
                    NeighbourhoodName = result.Data.NeighbourhoodName,
                    ProvinceID = result.Data.ProvinceID,
                    ProvinceName = result.Data.ProvinceName,
                    RuralCode = result.Data.RuralCode,
                    StreetID = result.Data.StreetID,
                    StreetName = result.Data.StreetName
                },
                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                Username = request.Username
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Apartment Address");
            return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceSubscriberGetBillsResponse GetBills(NetspeedServiceSubscriberGetBillsRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                if (request.HasValidHash(passwordHash, _duration))
                {
                    if (request.Data.PhoneNo.StartsWith("0"))
                    {
                        request.Data.PhoneNo = request.Data.PhoneNo.TrimStart('0');
                    }
                    var dbClient = db.Subscriptions.Where(s => s.SubscriberNo == request.Data.SubscriberNo && s.Customer.ContactPhoneNo == request.Data.PhoneNo).FirstOrDefault();
                    if (dbClient == null)
                        return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                        {
                            Culture = request.Culture,
                            Data = null,
                            Username = request.Username,
                            ResponseMessage = CommonResponse.SubscriberNotFoundErrorResponse(request.Culture),
                        };
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
                    return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                    {
                        Culture = request.Culture,
                        Data = result.ToArray(),
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture)
                    };
                }
                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Username = request.Username,
                    Data = null
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Subscriber unpaid bill list");
            return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                Username = request.Username
            };
        }
    }

    public NetspeedServicePaymentVPOSResponse SubscriberPaymentVPOS(NetspeedServicePaymentVPOSRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Username = request.Username,
                    Data = null
                };
            }
            using (RadiusREntities db = new RadiusREntities())
            {
                var dbSubscription = db.Subscriptions.Where(s => s.SubscriberNo == request.Data.SubscriberNo).FirstOrDefault();
                if (dbSubscription == null)
                    return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                    {
                        Culture = request.Culture,
                        ResponseMessage = CommonResponse.SubscriberNotFoundErrorResponse(request.Culture),
                        Username = request.Username,
                        Data = null,
                    };
                var VPOSModel = VPOSManager.GetVPOSModel(
                    request.Data.OkUrl,
                    request.Data.FailUrl,
                    request.Data.PayableAmount,
                    dbSubscription.Customer.Culture.Split('-').FirstOrDefault(),
                    dbSubscription.SubscriberNo + "-" + dbSubscription.ValidDisplayName);
                var htmlForm = VPOSModel.GetHtmlForm().ToHtmlString();
                return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                {
                    Data = new PaymentVPOSResponse()
                    {
                        HtmlForm = htmlForm,
                    },
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    Username = request.Username
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get Subscriber payment VPOS");
            return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                Username = request.Username
            };
        }
    }

    public NetspeedServiceNewCustomerRegisterResponse NewCustomerRegister(NetspeedServiceNewCustomerRegisterRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Username = request.Username,
                    Data = null
                };
            }
            using (var db = new RadiusR.DB.RadiusREntities())
            {
                var registeredCustomer = new Customer();
                var register = request.Data;
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
                        ExecutiveNationality = (CountryCodes?)register.CorporateCustomerInfo.ExecutiveNationality,
                        ExecutiveProfession = (Profession?)register.CorporateCustomerInfo.ExecutiveProfession,
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
                        ExecutiveSex = (Sexes?)register.CorporateCustomerInfo.ExecutiveSex,
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
                        CustomerType = (CustomerType?)register.CustomerGeneralInfo.CustomerType,
                        Email = register.CustomerGeneralInfo.Email,
                        OtherPhoneNos = register.CustomerGeneralInfo.OtherPhoneNos == null ? null : register.CustomerGeneralInfo.OtherPhoneNos.Select(p => new CustomerRegistrationInfo.PhoneNoListItem()
                        {
                            Number = p.Number
                        })
                    },
                    IDCard = register.IDCardInfo == null ? null : new CustomerRegistrationInfo.IDCardInfo()
                    {
                        BirthDate = register.IDCardInfo.BirthDate,
                        CardType = (IDCardTypes?)register.IDCardInfo.CardType,
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
                        Nationality = (CountryCodes?)register.IndividualCustomerInfo.Nationality,
                        Profession = (Profession?)register.IndividualCustomerInfo.Profession,
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
                        Sex = (Sexes?)register.IndividualCustomerInfo.Sex
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
                    return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                    {
                        Data = valuePairs,
                        Culture = request.Culture,
                        ResponseMessage = CommonResponse.FailedResponse(request.Culture),
                        Username = request.Username
                    };
                }
                db.Customers.Add(registeredCustomer);
                db.SaveChanges();
                return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                {
                    Data = valuePairs,
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    Username = request.Username
                };
            }
        }
        catch (NullReferenceException ex)
        {
            Errorslogger.Error(ex, "Error Null Reference Exception");
            return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
            {
                Data = null,
                Username = request.Username,
                Culture = request.Culture,
                ResponseMessage = CommonResponse.NullObjectException(request.Culture)
            };
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error Get new customer register");
            return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                Username = request.Username,
                ResponseMessage = CommonResponse.InternalException(request.Culture)
            };
        }
    }

    //public BaseResponse<Dictionary<string, string>, SHA1> ExistingCustomerRegister(request<ExistingCustomerRegisterRequest, SHA1> request)
    //{
    //    var password = _password;
    //    var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
    //    try
    //    {
    //        if (!request.HasValidHash(passwordHash, _duration))
    //        {
    //            return CommonResponse<Dictionary<string, string>, SHA1>.UnauthorizedResponse(passwordHash, request);
    //        }
    //        using (var db = new RadiusR.DB.RadiusREntities())
    //        {
    //            var referenceCustomer = db.Subscriptions.Find(request.Data.SubscriberID);
    //            if (referenceCustomer == null)
    //                return CommonResponse<Dictionary<string, string>, SHA1>.SubscriberNotFoundErrorResponse(passwordHash, request);
    //            var result = RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionForExistingCustomer(db, new CustomerRegistrationInfo.SubscriptionRegistrationInfo(), referenceCustomer.Customer);
    //            Dictionary<string, string> valuePairs = null;
    //            if (result != null)
    //            {
    //                foreach (var item in result)
    //                {
    //                    valuePairs.Add(item.Key, item.FirstOrDefault());
    //                }
    //                return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, request)
    //                {
    //                    Data = valuePairs,
    //                    Culture = request.Culture,
    //                    ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.FailedResponse(request.Culture),
    //                    Username = request.Username
    //                };
    //            }
    //            return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, request)
    //            {
    //                Data = valuePairs,
    //                Culture = request.Culture,
    //                ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.SuccessResponse(request.Culture),
    //                Username = request.Username
    //            };
    //        }
    //    }
    //    catch (NullReferenceException ex)
    //    {
    //        Errorslogger.Error(ex, "Error Null Reference Exception");
    //        return CommonResponse<Dictionary<string, string>, SHA1>.NullObjectException(HashUtilities.CalculateHash<SHA1>(password), request);
    //    }
    //    catch (Exception ex)
    //    {
    //        Errorslogger.Error(ex, "Error Get new customer register");
    //        return CommonResponse<Dictionary<string, string>, SHA1>.InternalException(HashUtilities.CalculateHash<SHA1>(password), request);
    //    }
    //}

    public NetspeedServicePayBillsResponse PayBills(NetspeedServicePayBillsRequest request)
    {
        var password = _password;
        var passwordHash = HashUtilities.CalculateHash<SHA1>(password);
        try
        {
            if (!request.HasValidHash(passwordHash, _duration))
            {
                return new NetspeedServicePayBillsResponse(passwordHash, request)
                {
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    Username = request.Username,
                    Data = null,
                };
            }
            using (RadiusREntities db = new RadiusREntities())
            {
                if (request.Data.BillIds == null)
                    return new NetspeedServicePayBillsResponse(passwordHash, request)
                    {
                        Culture = request.Culture,
                        Data = null,
                        Username = request.Username,
                        ResponseMessage = CommonResponse.BillsNotFoundException(request.Culture)
                    };
                if (request.Data.BillIds.Count() == 0)
                    return new NetspeedServicePayBillsResponse(passwordHash, request)
                    {
                        Culture = request.Culture,
                        Data = null,
                        Username = request.Username,
                        ResponseMessage = CommonResponse.BillsNotFoundException(request.Culture)
                    };
                var Bills = db.Bills.Where(bill => request.Data.BillIds.Contains(bill.ID)).ToArray();
                var payResponse = RadiusR.DB.Utilities.Billing.BillPayment.PayBills(db, Bills, PaymentType.VirtualPos, BillPayment.AccountantType.Seller);
                db.SaveChanges();
                return new NetspeedServicePayBillsResponse(passwordHash, request)
                {
                    Data = new PayBillsResponse()
                    {
                        PaymentResponse = payResponse.ToString()
                    },
                    Culture = request.Culture,
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    Username = request.Username
                };
            }
        }
        catch (Exception ex)
        {
            Errorslogger.Error(ex, "Error pay bills");
            return new NetspeedServicePayBillsResponse(passwordHash, request)
            {
                Culture = request.Culture,
                Data = null,
                ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                Username = request.Username
            };
        }
    }
}
