using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using RezaB.API.WebService;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
[ServiceContract]
public interface INetspeedService
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
}
