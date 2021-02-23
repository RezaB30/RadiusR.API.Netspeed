using NLog;
using RadiusR.API.Netspeed.Requests;
using RadiusR.API.Netspeed.Responses;
using RadiusR.DB;
using RadiusR.DB.Enums;
using RadiusR.DB.Utilities.Billing;
using RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration;
using RadiusR.SMS;
using RadiusR.VPOS;
using RezaB.API.WebService;
using RezaB.Data.Localization;
using RezaB.TurkTelekom.WebServices.Address;
using RezaB.TurkTelekom.WebServices.Availability;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using RezaB.API.WebService.NLogExtentions;

namespace RadiusR.API.Netspeed
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class MainSiteService : IMainSiteService
    {
        readonly RadiusR.Address.AddressManager AddressClient = new RadiusR.Address.AddressManager();
        //Logger Errorslogger = LogManager.GetLogger("Errors");
        //Logger PaidLogger = LogManager.GetLogger("Paid");
        WebServiceLogger SMSLogger = new WebServiceLogger("SMSInternal");
        WebServiceLogger Errorslogger = new WebServiceLogger("Errors");
        WebServiceLogger PaidLogger = new WebServiceLogger("Paid");
        WebServiceLogger UnpaidLogger = new WebServiceLogger("Unpaid");
        WebServiceLogger InComingLogger = new WebServiceLogger("InComingInfo");
        public NetspeedServiceArrayListResponse GetProvinces(NetspeedServiceRequests request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                    };
                }
                var result = AddressClient.GetProvinces();
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                //Errorslogger.Error(ex, "Error Get Provinces");
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceRegisterCustomerContactResponse RegisterCustomerContact(NetspeedServiceCustomerContactRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                using (var db = new RadiusR.DB.RadiusREntities())
                {
                    //get password from db                
                    var IsValid = request.HasValidHash(passwordHash, new ServiceSettings().Duration());
                    if (IsValid)
                    {
                        var hasRegister = string.IsNullOrEmpty(CacheManager.Get(request.CustomerContactParameters.PhoneNo)) == false;
                        if (hasRegister)
                        {
                            return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                            {
                                RegisterCustomerContactResponse = false,
                                ResponseMessage = CommonResponse.FailedResponse(request.Culture, Localization.ErrorMessages.ResourceManager.GetString("AlreadyHaveRequest", CultureInfo.CreateSpecificCulture(request.Culture)))
                            };
                        }
                        var description = $"{request.CustomerContactParameters.FullName}{Environment.NewLine}{request.CustomerContactParameters.PhoneNo}";
                        db.SupportRequests.Add(new RadiusR.DB.SupportRequest()
                        {
                            Date = DateTime.Now,
                            IsVisibleToCustomer = false,
                            StateID = (short)RadiusR.DB.Enums.SupportRequests.SupportRequestStateID.InProgress,
                            TypeID = Properties.Settings.Default.SupportRequestTypeId,
                            SubTypeID = Properties.Settings.Default.SupportRequestSubTypeId,
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
                        CacheManager.Set(description, request.CustomerContactParameters.PhoneNo, CustomerWebsiteSettings.SupportRequestPassedTime);
                        return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                        {

                            ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                            RegisterCustomerContactResponse = true,

                        };
                    }
                    return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                    {
                        RegisterCustomerContactResponse = false,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceRegisterCustomerContactResponse(passwordHash, request)
                {

                    RegisterCustomerContactResponse = false,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceServiceAvailabilityResponse ServiceAvailability(NetspeedServiceServiceAvailabilityRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
                    {
                        ServiceAvailabilityResponse = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                using (var db = new RadiusR.DB.RadiusREntities())
                {
                    var domain = RadiusR.DB.DomainsCache.DomainsCache.GetDomainByID(RadiusR.DB.CustomerWebsiteSettings.WebsiteServicesInfrastructureDomainID);
                    var client = new AvailabilityServiceClient(domain.TelekomCredential.XDSLWebServiceUsernameInt, domain.TelekomCredential.XDSLWebServicePassword);
                    var xdslTypeAdsl = AvailabilityServiceClient.XDSLType.ADSL;
                    var xdslTypeVdsl = AvailabilityServiceClient.XDSLType.VDSL;
                    var xdslTypeFiber = AvailabilityServiceClient.XDSLType.Fiber;
                    var queryType = AvailabilityServiceClient.QueryType.BBK;
                    List<Thread> threads = new List<Thread>();
                    RezaB.TurkTelekom.WebServices.ServiceResponse<AvailabilityServiceClient.AvailabilityDescription> availabAdsl = null, availabVdsl = null, availabFiber = null;
                    Thread threadAdsl = new Thread(() => { availabAdsl = client.Check(xdslTypeAdsl, queryType, request.ServiceAvailabilityParameters.bbk); });
                    Thread threadVdsl = new Thread(() => { availabVdsl = client.Check(xdslTypeVdsl, queryType, request.ServiceAvailabilityParameters.bbk); });
                    Thread threadFiber = new Thread(() => { availabFiber = client.Check(xdslTypeFiber, queryType, request.ServiceAvailabilityParameters.bbk); });
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
                    var address = addressServiceClient.GetAddressFromCode(Convert.ToInt64(request.ServiceAvailabilityParameters.bbk));
                    return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
                    {
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                        ServiceAvailabilityResponse = new ServiceAvailabilityResponse()
                        {
                            address = address.InternalException == null ? address.Data.AddressText : "-",
                            ADSL = new ServiceAvailabilityResponse.ADSLInfo()
                            {
                                HasInfrastructureAdsl = HasInfrastructureAdsl,
                                AdslDistance = availabAdsl.InternalException == null ? availabAdsl.Data.Description.Distance : null,
                                AdslPortState = availabAdsl.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabAdsl.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                                AdslSpeed = availabAdsl.InternalException == null ? availabAdsl.Data.Description.DSLMaxSpeed : null,
                                AdslSVUID = availabAdsl.InternalException == null ? availabAdsl.Data.Description.SVUID : "-",
                            },
                            VDSL = new ServiceAvailabilityResponse.VDSLInfo()
                            {
                                HasInfrastructureVdsl = HasInfrastructureVdsl,
                                VdslDistance = availabVdsl.InternalException == null ? availabVdsl.Data.Description.Distance : null,
                                VdslPortState = availabVdsl.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabVdsl.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                                VdslSpeed = availabVdsl.InternalException == null ? availabVdsl.Data.Description.DSLMaxSpeed : null,
                                VdslSVUID = availabVdsl.InternalException == null ? availabVdsl.Data.Description.SVUID : "-",
                            },
                            FIBER = new ServiceAvailabilityResponse.FIBERInfo()
                            {
                                HasInfrastructureFiber = HasInfrastructureFiber,
                                FiberDistance = availabFiber.InternalException == null ? availabFiber.Data.Description.Distance : null,
                                FiberPortState = availabFiber.InternalException == null ? RadiusR.Localization.Lists.PortState.ResourceManager.GetString(availabFiber.Data.Description.PortState.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)) : RadiusR.Localization.Lists.PortState.ResourceManager.GetString(AvailabilityServiceClient.PortState.NotAvailable.ToString(), CultureInfo.CreateSpecificCulture(request.Culture)),
                                FiberSpeed = availabFiber.InternalException == null ? availabFiber.Data.Description.DSLMaxSpeed : null,
                                FiberSVUID = availabFiber.InternalException == null ? availabFiber.Data.Description.SVUID : "-",
                            },
                            BBK = request.ServiceAvailabilityParameters.bbk
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceServiceAvailabilityResponse(passwordHash, request)
                {
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    ServiceAvailabilityResponse = null,
                };
            }
        }
        public NetspeedServiceArrayListResponse GetProvinceDistricts(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {
                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetProvinceDistricts(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetDistrictRuralRegions(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {
                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetDistrictRuralRegions(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetRuralRegionNeighbourhoods(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {


                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetRuralRegionNeighbourhoods(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetNeighbourhoodStreets(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {


                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetNeighbourhoodStreets(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetStreetBuildings(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {


                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetStreetBuildings(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }
        public NetspeedServiceArrayListResponse GetBuildingApartments(NetspeedServiceArrayListRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {


                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture)
                    };
                }
                if (request.ItemCode == null)
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetBuildingApartments(request.ItemCode.Value);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = result.ErrorOccured == false ? result.Data.Select(p => new ValueNamePair()
                    {
                        Code = p.Code,
                        Name = p.Name
                    }) : Enumerable.Empty<ValueNamePair>(),
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture),

                };
            }
        }
        public NetspeedServiceAddressDetailsResponse GetApartmentAddress(NetspeedServiceAddressDetailsRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
                    {


                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                        AddressDetailsResponse = null
                    };
                }
                if (request.BBK == null)
                {
                    return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
                    {

                        AddressDetailsResponse = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                var result = AddressClient.GetApartmentAddress(request.BBK.Value);
                return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
                {

                    AddressDetailsResponse = new AddressDetailsResponse()
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

                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceAddressDetailsResponse(passwordHash, request)
                {

                    AddressDetailsResponse = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServiceSubscriberGetBillsResponse GetBills(NetspeedServiceSubscriberGetBillsRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                using (var db = new RadiusR.DB.RadiusREntities())
                {
                    if (request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                    {
                        if (request.GetBillParameters.PhoneNo.StartsWith("0"))
                        {
                            request.GetBillParameters.PhoneNo = request.GetBillParameters.PhoneNo.TrimStart('0');
                        }
                        if (request.GetBillParameters.TCKOrSubscriberNo.Length == 11)
                        {
                            var dbClient = db.Subscriptions.Where(s => s.Customer.CustomerIDCard.TCKNo == request.GetBillParameters.TCKOrSubscriberNo && s.Customer.ContactPhoneNo == request.GetBillParameters.PhoneNo).ToList();
                            if (dbClient == null || dbClient.Count() == 0)
                            {
                                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                                {

                                    SubscriberGetBillsResponse = null,

                                    ResponseMessage = CommonResponse.SubscriberNotFoundErrorResponse(request.Culture),
                                };
                            }
                            var subscriptionIdList = dbClient.Select(dc => dc.ID);
                            var dbSubscriptionBills = db.Bills.Where(bill => subscriptionIdList.Contains(bill.SubscriptionID) && bill.BillStatusID == (short)BillState.Unpaid).ToArray();
                            if (dbSubscriptionBills == null || dbSubscriptionBills.Count() == 0)
                            {
                                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                                {

                                    ResponseMessage = CommonResponse.BillsNotFoundException(request.Culture),

                                    SubscriberGetBillsResponse = null,
                                };
                            }
                            var HasMoreSubscription = dbSubscriptionBills.GroupBy(m => m.SubscriptionID).Count() > 1;
                            if (HasMoreSubscription)
                            {
                                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                                {


                                    ResponseMessage = CommonResponse.HasMoreSubscription(request.Culture),
                                    SubscriberGetBillsResponse = null,
                                };
                            }
                            var firstUnpaidBill = dbSubscriptionBills.Where(bill => bill.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid).OrderBy(bill => bill.IssueDate).FirstOrDefault();
                            var result = dbSubscriptionBills.Where(bill => bill.BillStatusID == (short)RadiusR.DB.Enums.BillState.Unpaid).OrderBy(bill => bill.IssueDate).Select(bill =>
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

                                SubscriberGetBillsResponse = result.ToArray(),
                                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                            };
                        }
                        else
                        {
                            var dbClient = db.Subscriptions.Where(s => s.SubscriberNo == request.GetBillParameters.TCKOrSubscriberNo && s.Customer.ContactPhoneNo == request.GetBillParameters.PhoneNo).FirstOrDefault();
                            if (dbClient == null)
                                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                                {

                                    SubscriberGetBillsResponse = null,

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

                                SubscriberGetBillsResponse = result.ToArray(),
                                ResponseMessage = CommonResponse.SuccessResponse(request.Culture)
                            };
                        }
                    }
                    return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                    {

                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                        SubscriberGetBillsResponse = null
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceSubscriberGetBillsResponse(passwordHash, request)
                {

                    SubscriberGetBillsResponse = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServicePaymentVPOSResponse SubscriberPaymentVPOS(NetspeedServicePaymentVPOSRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                    {

                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                        PaymentVPOSResponse = null
                    };
                }
                if (request.PaymentVPOSParameters.BillIds == null || request.PaymentVPOSParameters.BillIds.Count() == 0)
                {
                    return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                    {

                        PaymentVPOSResponse = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),

                    };
                }
                using (RadiusREntities db = new RadiusREntities())
                {
                    var dbSubscriptionBills = db.Bills.Where(bill => request.PaymentVPOSParameters.BillIds.Contains(bill.ID) && bill.BillStatusID == (short)BillState.Unpaid).ToArray();
                    if (dbSubscriptionBills == null || dbSubscriptionBills.Count() != request.PaymentVPOSParameters.BillIds.Count())
                    {
                        return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.WrongOrInvalidBills(request.Culture),
                            PaymentVPOSResponse = null,
                        };
                    }
                    var HasMoreSubscription = dbSubscriptionBills.GroupBy(m => m.SubscriptionID).Count() > 1;
                    if (HasMoreSubscription)
                    {
                        return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.HasMoreSubscription(request.Culture),
                            PaymentVPOSResponse = null,
                        };
                    }
                    var dbSubscription = db.Subscriptions.Find(dbSubscriptionBills.FirstOrDefault().SubscriptionID);
                    var PayableAmount = dbSubscriptionBills.Select(bill => GetPayableAmount(dbSubscription, bill.ID)).Sum();
                    var VPOSModel = VPOSManager.GetVPOSModel(
                        request.PaymentVPOSParameters.OkUrl,
                        request.PaymentVPOSParameters.FailUrl,
                        PayableAmount,
                        dbSubscription.Customer.Culture.Split('-').FirstOrDefault(),
                        dbSubscription.SubscriberNo + "-" + dbSubscription.ValidDisplayName);
                    var htmlForm = VPOSModel.GetHtmlForm().ToHtmlString();
                    return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                    {
                        PaymentVPOSResponse = new PaymentVPOSResponse()
                        {
                            HtmlForm = htmlForm,
                        },
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServicePaymentVPOSResponse(passwordHash, request)
                {

                    PaymentVPOSResponse = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServiceNewCustomerRegisterResponse NewCustomerRegister(NetspeedServiceNewCustomerRegisterRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                    {

                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                        NewCustomerRegisterResponse = null
                    };
                }
                using (var db = new RadiusR.DB.RadiusREntities())
                {
                    var specialOfferId = db.SpecialOffers.Where(s => s.IsReferral == true && DateTime.Now > s.StartDate && DateTime.Now < s.EndDate).ToList();
                    if (specialOfferId.Count != 1)
                    {
                        return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                        {
                            NewCustomerRegisterResponse = null,
                            ResponseMessage = CommonResponse.SpecialOfferError(request.Culture)
                        };
                    }
                    var externalTariff = db.ExternalTariffs.GetActiveExternalTariffs().FirstOrDefault(ext => ext.TariffID == request.CustomerRegisterParameters.SubscriptionInfo.ServiceID);
                    if (externalTariff == null)
                    {
                        return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.TariffNotFound(request.Culture),
                            NewCustomerRegisterResponse = null
                        };
                    }
                    var billingPeriod = externalTariff.Service.GetBestBillingPeriod(DateTime.Now.Day);
                    var currentSpecialOfferId = specialOfferId.FirstOrDefault().ID;
                    var registeredCustomer = new Customer();
                    var register = request.CustomerRegisterParameters;
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
                            CustomerType = CustomerType.Individual,
                            Email = register.CustomerGeneralInfo.Email,
                            OtherPhoneNos = register.CustomerGeneralInfo.OtherPhoneNos == null ? null : register.CustomerGeneralInfo.OtherPhoneNos.Select(p => new CustomerRegistrationInfo.PhoneNoListItem()
                            {
                                Number = p.Number
                            })
                        },
                        IDCard = register.IDCardInfo == null ? null : new CustomerRegistrationInfo.IDCardInfo()
                        {
                            BirthDate = RezaB.API.WebService.DataTypes.ServiceTypeConverter.ParseDate(register.IDCardInfo.BirthDate),
                            CardType = (IDCardTypes?)register.IDCardInfo.CardType,
                            DateOfIssue = RezaB.API.WebService.DataTypes.ServiceTypeConverter.ParseDate(register.IDCardInfo.DateOfIssue),
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
                            GroupIds = CustomerWebsiteSettings.CustomerWebsiteRegistrationGroupID == null ? null : new int[] { CustomerWebsiteSettings.CustomerWebsiteRegistrationGroupID.Value },
                            DomainID = externalTariff.DomainID,
                            ServiceID = externalTariff.TariffID,
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
                            BillingPeriod = billingPeriod,
                            ReferralDiscount = string.IsNullOrEmpty(request.CustomerRegisterParameters.SubscriptionInfo.ReferralDiscountInfo.ReferenceNo) ? null : new CustomerRegistrationInfo.ReferralDiscountInfo()
                            {
                                ReferenceNo = request.CustomerRegisterParameters.SubscriptionInfo.ReferralDiscountInfo.ReferenceNo,
                                SpecialOfferID = currentSpecialOfferId
                            }
                        },
                    };

                    var result = RadiusR.DB.Utilities.ComplexOperations.Subscriptions.Registration.Registration.RegisterSubscriptionWithNewCustomer(db, registrationInfo, out registeredCustomer);
                    Dictionary<string, string> valuePairs = new Dictionary<string, string>();

                    if (result != null)
                    {
                        var dic = result.ToDictionary(x => x.Key, x => x.ToArray());
                        foreach (var item in dic)
                        {
                            valuePairs.Add(item.Key, string.Join("-", item.Value));
                        }
                        return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                        {
                            NewCustomerRegisterResponse = valuePairs,

                            ResponseMessage = CommonResponse.FailedResponse(request.Culture),

                        };
                    }
                    if (registeredCustomer == null)
                    {
                        var currentCustomer = db.Customers.Where(c => c.CustomerIDCard.TCKNo == register.IDCardInfo.TCKNo
                        && (c.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.Active || s.State == (short)RadiusR.DB.Enums.CustomerState.Disabled
                        || s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd || s.State == (short)RadiusR.DB.Enums.CustomerState.Registered
                        || s.State == (short)RadiusR.DB.Enums.CustomerState.Reserved).FirstOrDefault() == null)).FirstOrDefault(); // if not null registered else not registered
                        if (currentCustomer != null)
                        {
                            db.SaveChanges();
                            var curSubscriptionId = currentCustomer.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd).FirstOrDefault() == null ?
                                currentCustomer.Subscriptions.FirstOrDefault().ID :
                                currentCustomer.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd).FirstOrDefault().ID;
                            var curSubscriberNo = currentCustomer.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd).FirstOrDefault() == null ?
                                currentCustomer.Subscriptions.FirstOrDefault().SubscriberNo :
                                currentCustomer.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd).FirstOrDefault().SubscriberNo;
                            db.SystemLogs.Add(RadiusR.SystemLogs.SystemLogProcessor.AddSubscription(null, curSubscriptionId, currentCustomer.ID, SystemLogInterface.MainSiteService, $"{request.Username} ({curSubscriberNo})", curSubscriberNo));
                            db.SaveChanges();
                            return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                            {
                                NewCustomerRegisterResponse = valuePairs,
                                ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                            };
                        }
                        return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.HaveAlreadyCustomer(request.Culture),
                            NewCustomerRegisterResponse = null
                        };
                    }
                    db.Customers.Add(registeredCustomer);
                    db.SaveChanges();
                    db.SystemLogs.Add(RadiusR.SystemLogs.SystemLogProcessor.AddSubscription(null, registeredCustomer.Subscriptions.FirstOrDefault().ID, registeredCustomer.ID, SystemLogInterface.MainSiteService, request.Username, registeredCustomer.Subscriptions.FirstOrDefault().SubscriberNo));
                    db.SaveChanges();
                    return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                    {
                        NewCustomerRegisterResponse = valuePairs,
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    };
                }
            }
            catch (NullReferenceException ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                {
                    NewCustomerRegisterResponse = null,
                    ResponseMessage = CommonResponse.NullObjectException(request.Culture)
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceNewCustomerRegisterResponse(passwordHash, request)
                {
                    NewCustomerRegisterResponse = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture)
                };
            }
        }

        //public BaseResponse<Dictionary<string, string>, SHA1> ExistingCustomerRegister(request<ExistingCustomerRegisterRequest, SHA1> request)
        //{
        //    var password = _password;
        //    var passwordHash = HashUtilities.GetHexString<SHA1>(password);
        //    try
        //    {
        //        if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
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
        //                    
        //                    ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.FailedResponse(request.Culture),
        //                    
        //                };
        //            }
        //            return new BaseResponse<Dictionary<string, string>, SHA1>(passwordHash, request)
        //            {
        //                Data = valuePairs,
        //                
        //                ResponseMessage = CommonResponse<Dictionary<string, string>, SHA1>.SuccessResponse(request.Culture),
        //                
        //            };
        //        }
        //    }
        //    catch (NullReferenceException ex)
        //    {
        //        Errorslogger.Error(ex, "Error Null Reference Exception");
        //        return CommonResponse<Dictionary<string, string>, SHA1>.NullObjectException(HashUtilities.GetHexString<SHA1>(password), request);
        //    }
        //    catch (Exception ex)
        //    {
        //        Errorslogger.Error(ex, "Error Get new customer register");
        //        return CommonResponse<Dictionary<string, string>, SHA1>.InternalException(HashUtilities.GetHexString<SHA1>(password), request);
        //    }
        //}

        public NetspeedServicePayBillsResponse PayBills(NetspeedServicePayBillsRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServicePayBillsResponse(passwordHash, request)
                    {
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                        PayBillsResponse = null,
                    };
                }
                using (RadiusREntities db = new RadiusREntities())
                {
                    if (request.PayBillsParameters == null)
                        return new NetspeedServicePayBillsResponse(passwordHash, request)
                        {

                            PayBillsResponse = null,

                            ResponseMessage = CommonResponse.BillsNotFoundException(request.Culture)
                        };
                    if (request.PayBillsParameters.Count() == 0)
                        return new NetspeedServicePayBillsResponse(passwordHash, request)
                        {

                            PayBillsResponse = null,

                            ResponseMessage = CommonResponse.BillsNotFoundException(request.Culture)
                        };
                    //
                    var Bills = db.Bills.Where(bill => request.PayBillsParameters.Contains(bill.ID) && bill.BillStatusID == (short)BillState.Unpaid).ToArray();
                    var HasMoreSubscription = Bills.GroupBy(m => m.SubscriptionID).Count() > 1;
                    if (HasMoreSubscription)
                    {
                        return new NetspeedServicePayBillsResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.HasMoreSubscription(request.Culture),
                            PayBillsResponse = null,
                        };
                    }
                    //
                    if (Bills.Count() != request.PayBillsParameters.Count())
                    {
                        return new NetspeedServicePayBillsResponse(passwordHash, request)
                        {

                            ResponseMessage = CommonResponse.WrongOrInvalidBills(request.Culture),

                            PayBillsResponse = null,
                        };
                    }
                    var payResponse = RadiusR.DB.Utilities.Billing.BillPayment.PayBills(db, Bills, PaymentType.VirtualPos, BillPayment.AccountantType.Admin);
                    db.SystemLogs.Add(RadiusR.SystemLogs.SystemLogProcessor.BillPayment(Bills.Select(b => b.ID).ToArray(), null, Bills.FirstOrDefault().SubscriptionID, SystemLogInterface.MainSiteService, request.Username, PaymentType.VirtualPos));
                    db.SaveChanges();
                    if (payResponse == BillPayment.ResponseType.Success)
                    {
                        PaidLogger.LogInfo(request.Username, $"Paid Successful. Bills : {string.Join("-", Bills.Select(b => b.ID.ToString()))}");
                        return new NetspeedServicePayBillsResponse(passwordHash, request)
                        {
                            PayBillsResponse = new PayBillsResponse()
                            {
                                PaymentResponse = CommonResponse.SuccessResponse(request.Culture).ErrorMessage
                            },

                            ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

                        };
                    }
                    UnpaidLogger.LogInfo(request.Username, $"Paid failed. Bills : {string.Join("-", Bills.Select(b => b.ID.ToString()))}");
                    return new NetspeedServicePayBillsResponse(passwordHash, request)
                    {
                        PayBillsResponse = new PayBillsResponse()
                        {
                            PaymentResponse = CommonResponse.PaymentResponse(request.Culture, payResponse).ErrorMessage
                        },
                        ResponseMessage = CommonResponse.FailedResponse(request.Culture),
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServicePayBillsResponse(passwordHash, request)
                {
                    PayBillsResponse = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                };
            }
        }

        readonly Random random = new Random();
        public NetspeedServiceSendGenericSMSResponse SendGenericSMS(NetspeedServiceSendGenericSMSRequest request)
        {
            InComingLogger.LogIncomingMessage(request);
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceSendGenericSMSResponse(passwordHash, request)
                    {
                        SMSCode = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    };
                }
                if (string.IsNullOrEmpty(request.SendGenericSMSParameters.CustomerPhoneNo))
                {
                    return new NetspeedServiceSendGenericSMSResponse(passwordHash, request)
                    {
                        SMSCode = null,
                        ResponseMessage = CommonResponse.NullObjectException(request.Culture),
                    };
                }
                var randomPassword = random.Next(100000, 999999);
                SMSService SMS = new SMSService();
                SMS.SendGenericSMS(request.SendGenericSMSParameters.CustomerPhoneNo, request.Culture, rawText: string.Format(Localization.Common.RegisterSMS, randomPassword, Properties.Settings.Default.PasswordDuration));
                SMSLogger.LogInfo(request.Username, $"Sent sms to {request.SendGenericSMSParameters.CustomerPhoneNo} . password is {randomPassword}");
                return new NetspeedServiceSendGenericSMSResponse(passwordHash, request)
                {
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    SMSCode = randomPassword.ToString(),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceSendGenericSMSResponse(passwordHash, request)
                {
                    SMSCode = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),
                };
            }
        }
        public NetspeedServiceIDCardValidationResponse IDCardValidation(NetspeedServiceIDCardValidationRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceIDCardValidationResponse(passwordHash, request)
                    {

                        IDCardValidationResponse = false,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    };
                }
                var validation = new RezaB.API.TCKValidation.TCKValidationClient();
                var BirthDate = RezaB.API.WebService.DataTypes.ServiceTypeConverter.ParseDate(request.IDCardValidationRequest.BirthDate);
                if (!BirthDate.HasValue)
                {
                    BirthDate = DateTime.Now;
                }
                if (request.IDCardValidationRequest.IDCardType == (int)RadiusR.DB.Enums.IDCardTypes.TCIDCardWithChip)
                {
                    var result = validation.ValidateNewTCK(
                        request.IDCardValidationRequest.TCKNo,
                        request.IDCardValidationRequest.FirstName,
                        request.IDCardValidationRequest.LastName,
                        BirthDate.Value,
                        request.IDCardValidationRequest.RegistirationNo);
                    return new NetspeedServiceIDCardValidationResponse(passwordHash, request)
                    {
                        IDCardValidationResponse = result,
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture)
                    };
                }
                else
                {
                    if (request.IDCardValidationRequest.RegistirationNo.Length != 9)
                    {
                        return new NetspeedServiceIDCardValidationResponse(passwordHash, request)
                        {
                            IDCardValidationResponse = false,
                            ResponseMessage = CommonResponse.FailedResponse(request.Culture, Localization.ErrorMessages.ResourceManager.GetString("InvalidSerialNo", CultureInfo.CreateSpecificCulture(request.Culture)))
                        };
                    }
                    var serial = request.IDCardValidationRequest.RegistirationNo.Substring(0, 3);
                    var serialNumber = request.IDCardValidationRequest.RegistirationNo.Substring(3, request.IDCardValidationRequest.RegistirationNo.Length - 3);
                    var result = validation.ValidateOldTCK(
                        request.IDCardValidationRequest.TCKNo,
                        request.IDCardValidationRequest.FirstName,
                        request.IDCardValidationRequest.LastName,
                        BirthDate.Value, serialNumber, serial);
                    return new NetspeedServiceIDCardValidationResponse(passwordHash, request)
                    {
                        IDCardValidationResponse = result,
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture)
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceIDCardValidationResponse(passwordHash, request)
                {
                    IDCardValidationResponse = false,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }
        //public NetspeedServiceRegisterSMSValidationResponse RegisterSMSValidation(NetspeedServiceRegisterSMSValidationRequest request)
        //{
        //    var password = new ServiceSettings().Password(request.Username);
        //    var passwordHash = HashUtilities.GetHexString<SHA1>(password);
        //    try
        //    {
        //        if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
        //        {
        //            return new NetspeedServiceRegisterSMSValidationResponse(passwordHash, request)
        //            {

        //                IsSuccess = false,
        //                ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

        //            };
        //        }
        //        if (string.IsNullOrEmpty(request.RegisterSMSValidationParameters.CustomerPhoneNo) || string.IsNullOrEmpty(request.RegisterSMSValidationParameters.Password))
        //        {
        //            //Errorslogger.Error($"Null object error. Phone No : {request.Data.CustomerPhoneNo} - Password : {request.Data.Password}");
        //            return new NetspeedServiceRegisterSMSValidationResponse(passwordHash, request)
        //            {

        //                IsSuccess = false,
        //                ResponseMessage = CommonResponse.NullObjectException(request.Culture),

        //            };
        //        }
        //        var getCacheValue = CacheManager.Get(request.RegisterSMSValidationParameters.CustomerPhoneNo);
        //        if (string.IsNullOrEmpty(getCacheValue))
        //        {
        //            SMSLogger.LogInfo(request.Username, $"SMS validation is failed. key : {request.RegisterSMSValidationParameters.CustomerPhoneNo} - value : {request.RegisterSMSValidationParameters.Password} ");
        //            return new NetspeedServiceRegisterSMSValidationResponse(passwordHash, request)
        //            {
        //                IsSuccess = false,
        //                ResponseMessage = CommonResponse.FailedResponse(request.Culture, Localization.ErrorMessages.ResourceManager.GetString("WrongSMSPassword", CultureInfo.CreateSpecificCulture(request.Culture))),

        //            };
        //        }
        //        return new NetspeedServiceRegisterSMSValidationResponse(passwordHash, request)
        //        {

        //            IsSuccess = true,
        //            ResponseMessage = CommonResponse.SuccessResponse(request.Culture),

        //        };

        //    }
        //    catch (Exception ex)
        //    {
        //        Errorslogger.LogException(request.Username, ex);
        //        return new NetspeedServiceRegisterSMSValidationResponse(passwordHash, request)
        //        {

        //            IsSuccess = false,
        //            ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

        //        };
        //    }
        //}
        #region
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
        #endregion
        public NetspeedServiceArrayListResponse GetNationalities(NetspeedServiceRequests request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                    };
                }
                var list = new LocalizedList<CountryCodes, RadiusR.Localization.Lists.CountryCodes>().GetList(CultureInfo.CreateSpecificCulture(request.Culture));
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {


                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    ValueNamePairList = list.Select(l => new ValueNamePair()
                    {
                        Code = l.Key,
                        Name = l.Value
                    }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetSexes(NetspeedServiceRequests request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                    };
                }
                var list = new LocalizedList<Sexes, RadiusR.Localization.Lists.Sexes>().GetList(CultureInfo.CreateSpecificCulture(request.Culture));
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {


                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    ValueNamePairList = list.Select(l => new ValueNamePair()
                    {
                        Code = l.Key,
                        Name = l.Value
                    }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetProfessions(NetspeedServiceRequests request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                    };
                }
                var list = new LocalizedList<Profession, RadiusR.Localization.Lists.Profession>().GetList(CultureInfo.CreateSpecificCulture(request.Culture));
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {


                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    ValueNamePairList = list.Select(l => new ValueNamePair()
                    {
                        Code = l.Key,
                        Name = l.Value
                    }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }

        public NetspeedServiceArrayListResponse GetIDCardTypes(NetspeedServiceRequests request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceArrayListResponse(passwordHash, request)
                    {

                        ValueNamePairList = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),

                    };
                }
                var list = new LocalizedList<IDCardTypes, RadiusR.Localization.Lists.IDCardTypes>().GetList(CultureInfo.CreateSpecificCulture(request.Culture));
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {


                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                    ValueNamePairList = list.Take(2).Select(l => new ValueNamePair()
                    {
                        Code = l.Key,
                        Name = l.Value
                    }).ToArray(),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceArrayListResponse(passwordHash, request)
                {

                    ValueNamePairList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }
        public string GetKeyFragment(string username)
        {
            return KeyManager.GenerateKeyFragment(username, new ServiceSettings().Duration());
        }
        public NetspeedServiceExternalTariffResponse ExternalTariffList(NetspeedServiceExternalTariffRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    return new NetspeedServiceExternalTariffResponse(passwordHash, request)
                    {
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                        ExternalTariffList = null
                    };
                }
                using (var db = new RadiusREntities())
                {
                    var tariffs = db.ExternalTariffs.Select(t => new ExternalTariffResponse()
                    {
                        DisplayName = t.DisplayName,
                        HasFiber = t.HasFiber,
                        HasXDSL = t.HasXDSL,
                        TariffID = t.TariffID,
                        Price = t.Service.Price,
                        Speed = t.Service.RateLimit
                    }).ToArray();
                    foreach (var item in tariffs)
                    {
                        var speedRate = RezaB.Data.Formating.RateLimitParser.ParseString(item.Speed);
                        item.Speed = $"{speedRate.DownloadRate} {speedRate.DownloadRateSuffix}";
                    }
                    return new NetspeedServiceExternalTariffResponse(passwordHash, request)
                    {
                        ExternalTariffList = tariffs,
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture)
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceExternalTariffResponse(passwordHash, request)
                {

                    ExternalTariffList = null,
                    ResponseMessage = CommonResponse.InternalException(request.Culture, ex),

                };
            }
        }
        public NetspeedServiceGenericAppSettingsResponse GenericAppSettings(NetspeedServiceGenericAppSettingsRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    //Errorslogger.Error($"unauthorize error. User : {request.Username}");
                    Errorslogger.LogException(request.Username, new Exception("unauthorize error"));
                    return new NetspeedServiceGenericAppSettingsResponse(passwordHash, request)
                    {
                        GenericAppSettings = null,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    };
                }
                var recaptchaSiteKey = CustomerWebsiteSettings.CustomerWebsiteRecaptchaClientKey;
                var recaptchaSecretKey = CustomerWebsiteSettings.CustomerWebsiteRecaptchaServerKey;
                var useGoogleRecaptcha = CustomerWebsiteSettings.CustomerWebsiteUseGoogleRecaptcha;
                return new NetspeedServiceGenericAppSettingsResponse(passwordHash, request)
                {
                    GenericAppSettings = new GenericAppSettingsResponse()
                    {
                        RecaptchaClientKey = recaptchaSiteKey,
                        RecaptchaServerKey = recaptchaSecretKey,
                        UseGoogleRecaptcha = useGoogleRecaptcha
                    },
                    ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                };
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceGenericAppSettingsResponse(passwordHash, request)
                {
                    ResponseMessage = CommonResponse.InternalException(request.Culture),
                    GenericAppSettings = null,
                };
            }
        }
        public NetspeedServiceGenericValidateResponse CheckRegisteredCustomer(NetspeedServiceCheckRegisteredCustomerRequest request)
        {
            var password = new ServiceSettings().Password(request.Username);
            var passwordHash = HashUtilities.GetHexString<SHA1>(password);
            try
            {
                InComingLogger.LogIncomingMessage(request);
                if (!request.HasValidHash(passwordHash, new ServiceSettings().Duration()))
                {
                    //Errorslogger.Error($"unauthorize error. User : {request.Username}");
                    Errorslogger.LogException(request.Username, new Exception("unauthorize error"));
                    return new NetspeedServiceGenericValidateResponse(passwordHash, request)
                    {
                        ValidateResult = false,
                        ResponseMessage = CommonResponse.UnauthorizedResponse(request.Culture),
                    };
                }
                using (var db = new RadiusREntities())
                {
                    var registeredCustomer = db.Customers.Where(c => c.ContactPhoneNo.Contains(request.PhoneNo)).FirstOrDefault();
                    if (registeredCustomer != null && registeredCustomer.Subscriptions.Where(s => s.State == (short)RadiusR.DB.Enums.CustomerState.Active
                    || s.State == (short)RadiusR.DB.Enums.CustomerState.PreRegisterd
                    || s.State == (short)RadiusR.DB.Enums.CustomerState.Registered
                    || s.State == (short)RadiusR.DB.Enums.CustomerState.Reserved).FirstOrDefault() != null)
                    {
                        return new NetspeedServiceGenericValidateResponse(passwordHash, request)
                        {
                            ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                            ValidateResult = true
                        };
                    }
                    return new NetspeedServiceGenericValidateResponse(passwordHash, request)
                    {
                        ResponseMessage = CommonResponse.SuccessResponse(request.Culture),
                        ValidateResult = false
                    };
                }
            }
            catch (Exception ex)
            {
                Errorslogger.LogException(request.Username, ex);
                return new NetspeedServiceGenericValidateResponse(passwordHash, request)
                {
                    ResponseMessage = CommonResponse.InternalException(request.Culture),
                    ValidateResult = false,
                };
            }
        }
    }
}
