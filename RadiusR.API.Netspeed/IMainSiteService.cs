﻿using RadiusR.API.Netspeed.Requests;
using RadiusR.API.Netspeed.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace RadiusR.API.Netspeed
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IMainSiteService
    {
        [OperationContract]
        NetspeedServiceRegisterCustomerContactResponse RegisterCustomerContact(NetspeedServiceCustomerContactRequest request);
        [OperationContract]
        NetspeedServiceServiceAvailabilityResponse ServiceAvailability(NetspeedServiceServiceAvailabilityRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetProvinces(NetspeedServiceRequests request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetProvinceDistricts(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetDistrictRuralRegions(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetRuralRegionNeighbourhoods(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetNeighbourhoodStreets(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetStreetBuildings(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetBuildingApartments(NetspeedServiceArrayListRequest request);
        [OperationContract]
        NetspeedServiceAddressDetailsResponse GetApartmentAddress(NetspeedServiceAddressDetailsRequest request);
        [OperationContract]
        NetspeedServiceSubscriberGetBillsResponse GetBills(NetspeedServiceSubscriberGetBillsRequest request);
        [OperationContract]
        NetspeedServicePaymentVPOSResponse SubscriberPaymentVPOS(NetspeedServicePaymentVPOSRequest request);
        [OperationContract]
        NetspeedServicePayBillsResponse PayBills(NetspeedServicePayBillsRequest request);
        [OperationContract]
        NetspeedServiceNewCustomerRegisterResponse NewCustomerRegister(NetspeedServiceNewCustomerRegisterRequest request);
        //[OperationContract]
        //NetspeedServiceVPOSLoggerResponse VPOSSuccessLogger(NetspeedServiceVPOSLoggerRequest request);
        //[OperationContract]
        //NetspeedServiceVPOSLoggerResponse VPOSFailLogger(NetspeedServiceVPOSLoggerRequest request);
        [OperationContract]
        NetspeedServiceSendGenericSMSResponse SendGenericSMS(NetspeedServiceSendGenericSMSRequest request);
        //[OperationContract]
        //NetspeedServiceRegisterSMSValidationResponse RegisterSMSValidation(NetspeedServiceRegisterSMSValidationRequest request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetNationalities(NetspeedServiceRequests request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetSexes(NetspeedServiceRequests request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetProfessions(NetspeedServiceRequests request);
        [OperationContract]
        NetspeedServiceArrayListResponse GetIDCardTypes(NetspeedServiceRequests request);
        [OperationContract]
        string GetKeyFragment(string username);
        [OperationContract]
        NetspeedServiceExternalTariffResponse ExternalTariffList(NetspeedServiceExternalTariffRequest request);
        [OperationContract]
        NetspeedServiceGenericAppSettingsResponse GenericAppSettings(NetspeedServiceGenericAppSettingsRequest request);
        [OperationContract]
        NetspeedServiceIDCardValidationResponse IDCardValidation(NetspeedServiceIDCardValidationRequest request);
    }
}
