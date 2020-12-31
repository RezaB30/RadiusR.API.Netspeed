using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for SubscriberPayBillsRequest
/// </summary>
/// 
namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class PaymentVPOSRequest
    {
        [DataMember]
        public long[] BillIds { get; set; }
        [DataMember]
        public string OkUrl { get; set; }
        [DataMember]
        public string FailUrl { get; set; }
    }
    [DataContract]
    public partial class NetspeedServicePaymentVPOSRequest : BaseRequest<PaymentVPOSRequest, SHA1>
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
