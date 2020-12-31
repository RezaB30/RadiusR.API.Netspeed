using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Summary description for SubscriberPayBillsResponse
/// </summary>
namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class PaymentVPOSResponse
    {
        [DataMember]
        public string HtmlForm { get; set; }
    }
    [DataContract]
    public partial class NetspeedServicePaymentVPOSResponse : BaseResponse<PaymentVPOSResponse, SHA1>
    {
        public NetspeedServicePaymentVPOSResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public PaymentVPOSResponse PaymentVPOSResponse
        {
            get { return Data; }
            set { Data = value; }
        }
    }
}
