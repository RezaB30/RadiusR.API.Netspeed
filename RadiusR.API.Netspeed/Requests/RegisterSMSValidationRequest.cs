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
    public class RegisterSMSValidationRequest
    {
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string CustomerPhoneNo { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceRegisterSMSValidationRequest : BaseRequest<RegisterSMSValidationRequest, SHA1>
    {
        [DataMember]
        public RegisterSMSValidationRequest RegisterSMSValidationParameters { get { return Data; } set { Data = value; } }
    }
}