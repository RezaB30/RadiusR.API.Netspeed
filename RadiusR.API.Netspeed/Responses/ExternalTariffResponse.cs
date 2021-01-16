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
    public class ExternalTariffResponse
    {
        [DataMember]
        public int TariffID { get; set; }
        [DataMember]
        public int DomainID { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public bool HasXDSL { get; set; }
        [DataMember]
        public bool HasFiber { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceExternalTariffResponse : BaseResponse<IEnumerable<ExternalTariffResponse>, SHA1>
    {
        public NetspeedServiceExternalTariffResponse(string passwordHash, BaseRequest<SHA1> request) : base(passwordHash, request) { }
        [DataMember]
        public IEnumerable<ExternalTariffResponse> ExternalTariffList { get { return Data; } set { Data = value; } }
    }
}