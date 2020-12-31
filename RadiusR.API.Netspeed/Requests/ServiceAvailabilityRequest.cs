using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for ServiceAvailabilityRequest
/// </summary>
/// 

namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class ServiceAvailabilityRequest
    {
        [DataMember]
        public string bbk { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceServiceAvailabilityRequest : BaseRequest<ServiceAvailabilityRequest, SHA1>
    {
        [DataMember]
        public ServiceAvailabilityRequest ServiceAvailabilityParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
}

