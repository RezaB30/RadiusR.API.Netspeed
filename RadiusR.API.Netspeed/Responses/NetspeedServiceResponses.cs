using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for NetspeedServiceResponses
/// </summary>
/// 

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public partial class NetspeedServiceRegisterCustomerContactResponse : BaseResponse<bool?, SHA1>
    {
        public NetspeedServiceRegisterCustomerContactResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public bool? RegisterCustomerContactResponse
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
    [DataContract]
    public partial class NetspeedServiceNewCustomerRegisterResponse : BaseResponse<Dictionary<string, string>, SHA1>
    {
        public NetspeedServiceNewCustomerRegisterResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public Dictionary<string, string> NewCustomerRegisterResponse
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
    [DataContract]
    public partial class NetspeedServiceGenericValidateResponse : BaseResponse<bool?, SHA1>
    {
        public NetspeedServiceGenericValidateResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public bool? ValidateResult
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
    [DataContract]
    public partial class NetspeedServiceAvailableChurnResponse : BaseResponse<bool, SHA1>
    {
        public NetspeedServiceAvailableChurnResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public bool IsAvailable
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
