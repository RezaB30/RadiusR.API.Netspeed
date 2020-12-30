using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
}
