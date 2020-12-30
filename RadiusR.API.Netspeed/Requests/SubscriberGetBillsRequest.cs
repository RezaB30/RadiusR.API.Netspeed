using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for SubscriberGetBillsRequest
/// </summary>
/// 
namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class SubscriberGetBillsRequest
    {
        [DataMember]
        public string TCKOrSubscriberNo { get; set; }
        [DataMember]
        public string PhoneNo { get; set; }
    }
}
