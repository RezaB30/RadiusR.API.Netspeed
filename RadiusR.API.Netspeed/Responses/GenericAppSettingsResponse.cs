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
    public class GenericAppSettingsResponse
    {
        [DataMember]
        public string RecaptchaClientKey { get; set; }
        [DataMember]
        public string RecaptchaServerKey { get; set; }
        [DataMember]
        public bool UseGoogleRecaptcha { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceGenericAppSettingsResponse : BaseResponse<GenericAppSettingsResponse, SHA1>
    {
        public NetspeedServiceGenericAppSettingsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public GenericAppSettingsResponse GenericAppSettings { get { return Data; } set { Data = value; } }
    }
}