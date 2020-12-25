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
    BaseResponse<bool, SHA1> RegisterCustomerContact(BaseRequest<CustomerContactRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<ServiceAvailabilityResponse, SHA1> ServiceAvailability(BaseRequest<ServiceAvailabilityRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetProvinces(BaseRequest<string, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetProvinceDistricts(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetDistrictRuralRegions(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetRuralRegionNeighbourhoods(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetNeighbourhoodStreets(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetStreetBuildings(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<ValueNamePair>, SHA1> GetBuildingApartments(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<RadiusR.Address.QueryInterface.AddressDetails, SHA1> GetApartmentAddress(BaseRequest<long, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1> GetBills(BaseRequest<SubscriberGetBillsRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<SubscriberPayBillsResponse, SHA1> SubscriberPaymentVPOS(BaseRequest<SubscriberPayBillsRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<PayBillsResponse, SHA1> PayBills(BaseRequest<PayBillsRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<Dictionary<string, string>, SHA1> NewCustomerRegister(BaseRequest<NewCustomerRegisterRequest, SHA1> baseRequest);
    [OperationContract]
    BaseResponse<Dictionary<string, string>, SHA1> ExistingCustomerRegister(BaseRequest<ExistingCustomerRegisterRequest, SHA1> baseRequest);
}
