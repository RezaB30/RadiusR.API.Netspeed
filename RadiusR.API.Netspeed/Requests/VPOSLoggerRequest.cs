using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class VPOSLoggerRequest
    {
        [DataMember]
        public IEnumerable<long> BillIds { get; set; }
        [DataMember]
        public Dictionary<string, string> FormKeys { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceVPOSLoggerRequest : BaseRequest<VPOSLoggerRequest, SHA1>
    {
        [DataMember]
        public VPOSLoggerRequest VPOSLoggerParameters { get { return Data; } set { Data = value; } }
    }
}