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
    public partial class NetspeedServiceAvailableChurnRequest : BaseRequest<string,SHA1>
    {
        [DataMember]
        public string XDSLNo { get; set; }
    }
}