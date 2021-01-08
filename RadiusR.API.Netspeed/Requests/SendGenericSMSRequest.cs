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
    public class SendGenericSMSRequest
    {
        [DataMember]
        public string CustomerPhoneNo { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceSendGenericSMSRequest : BaseRequest<SendGenericSMSRequest, SHA1>
    {
        [DataMember]
        public SendGenericSMSRequest SendGenericSMSParameters { get { return Data; } set { Data = value; } }
    }
}