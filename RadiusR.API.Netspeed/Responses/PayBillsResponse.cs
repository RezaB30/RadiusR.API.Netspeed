using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for PayBillsResponse
/// </summary>

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class PayBillsResponse
    {
        /*public RadiusR.DB.Utilities.Billing.BillPayment.ResponseType*/
        [DataMember]
        public string PaymentResponse { get; set; }
    }
    [DataContract]
    public partial class NetspeedServicePayBillsResponse : BaseResponse<PayBillsResponse, SHA1>
    {
        public NetspeedServicePayBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public PayBillsResponse PayBillsResponse
        {
            get { return Data; }
            set { Data = value; }
        }
    }
}
