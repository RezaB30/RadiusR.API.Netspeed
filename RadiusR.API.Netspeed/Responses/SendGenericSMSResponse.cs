using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public partial class NetspeedServiceSendGenericSMSResponse : BaseResponse<string, SHA1>
    {
        public NetspeedServiceSendGenericSMSResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public string SMSCode { get { return Data; } set { Data = value; } }
    }
}