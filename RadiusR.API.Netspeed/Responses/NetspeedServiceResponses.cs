using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for NetspeedServiceResponses
/// </summary>
/// 

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class NetspeedServiceAddressDetailsResponse : BaseResponse<AddressDetailsResponse, SHA1>
    {
        public NetspeedServiceAddressDetailsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public AddressDetailsResponse AddressDetailsResponse
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public class NetspeedServicePayBillsResponse : BaseResponse<PayBillsResponse, SHA1>
    {
        public NetspeedServicePayBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public PayBillsResponse PayBillsResponse
        {
            get { return Data; }
            set { Data = value; }
        }
    }
    [DataContract]
    public class NetspeedServicePaymentVPOSResponse : BaseResponse<PaymentVPOSResponse, SHA1>
    {
        public NetspeedServicePaymentVPOSResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public PaymentVPOSResponse PaymentVPOSResponse
        {
            get { return Data; }
            set { Data = value; }
        }
    }
    [DataContract]
    public class NetspeedServiceServiceAvailabilityResponse : BaseResponse<ServiceAvailabilityResponse, SHA1>
    {
        public NetspeedServiceServiceAvailabilityResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public ServiceAvailabilityResponse ServiceAvailabilityResponse
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
    [DataContract]
    public class NetspeedServiceSubscriberGetBillsResponse : BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>
    {
        public NetspeedServiceSubscriberGetBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public IEnumerable<SubscriberGetBillsResponse> SubscriberGetBillsResponse
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
    [DataContract]
    public class NetspeedServiceArrayListResponse : BaseResponse<IEnumerable<ValueNamePair>, SHA1>
    {
        public NetspeedServiceArrayListResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public IEnumerable<ValueNamePair> ValueNamePairList
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
    [DataContract]
    public class NetspeedServiceRegisterCustomerContactResponse : BaseResponse<bool?, SHA1>
    {
        public NetspeedServiceRegisterCustomerContactResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public bool? RegisterCustomerContactResponse
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
    [DataContract]
    public class NetspeedServiceNewCustomerRegisterResponse : BaseResponse<Dictionary<string, string>, SHA1>
    {
        public NetspeedServiceNewCustomerRegisterResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public Dictionary<string, string> NewCustomerRegisterResponse
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
}
