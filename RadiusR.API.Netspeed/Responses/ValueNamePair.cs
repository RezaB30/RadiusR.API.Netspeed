using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for ValueNamePair
/// </summary>

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class ValueNamePair
    {
        [DataMember]
        public long Code { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceArrayListResponse : BaseResponse<IEnumerable<ValueNamePair>, SHA1>
    {
        public NetspeedServiceArrayListResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public IEnumerable<ValueNamePair> ValueNamePairList
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
