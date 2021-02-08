using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for CustomerContactRequest
/// </summary>
namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class CustomerContactRequest
    {
        [DataMember]
        public string PhoneNo { get; set; }
        [DataMember]
        public string FullName { get; set; }
    }
    [DataContract]
    public partial class NetspeedServiceCustomerContactRequest : BaseRequest<CustomerContactRequest, SHA1>
    {
        [DataMember]
        public CustomerContactRequest CustomerContactParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
}
