using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for NetspeedServiceRequests
/// </summary>
/// 
namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class NetspeedServiceCustomerContactRequest : BaseRequest<CustomerContactRequest, SHA1>
    {
        [DataMember]
        public CustomerContactRequest CustomerContactParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public class NetspeedServiceNewCustomerRegisterRequest : BaseRequest<NewCustomerRegisterRequest, SHA1>
    {
        [DataMember]
        public NewCustomerRegisterRequest CustomerRegisterParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public class NetspeedServicePayBillsRequest : BaseRequest<IEnumerable<long>, SHA1>
    {
        [DataMember]
        public IEnumerable<long> PayBillsParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public class NetspeedServiceServiceAvailabilityRequest : BaseRequest<ServiceAvailabilityRequest, SHA1>
    {
        [DataMember]
        public ServiceAvailabilityRequest ServiceAvailabilityParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public class NetspeedServiceSubscriberGetBillsRequest : BaseRequest<SubscriberGetBillsRequest, SHA1>
    {
        [DataMember]
        public SubscriberGetBillsRequest GetBillParameters
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
    public class NetspeedServiceRequests : BaseRequest<SHA1> { }
    [DataContract]
    public class NetspeedServiceArrayListRequest : BaseRequest<long?, SHA1>
    {
        [DataMember]
        public long? ItemCode
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
    public class NetspeedServiceAddressDetailsRequest : BaseRequest<long?, SHA1>
    {
        [DataMember]
        public long? BBK
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
    public class NetspeedServicePaymentVPOSRequest : BaseRequest<PaymentVPOSRequest, SHA1>
    {
        [DataMember]
        public PaymentVPOSRequest PaymentVPOSParameters
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
