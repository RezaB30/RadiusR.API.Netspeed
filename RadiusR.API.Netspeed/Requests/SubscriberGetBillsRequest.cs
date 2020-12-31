using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
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
    [DataContract]
    public partial class NetspeedServiceSubscriberGetBillsRequest : BaseRequest<SubscriberGetBillsRequest, SHA1>
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
}
