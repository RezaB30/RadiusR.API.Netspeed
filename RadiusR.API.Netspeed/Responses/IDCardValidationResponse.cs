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
    public class NetspeedServiceIDCardValidationResponse : BaseResponse<bool, SHA1>
    {
        public NetspeedServiceIDCardValidationResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public bool IDCardValidationResponse { get { return Data; } set { Data = value; } }
    }
}