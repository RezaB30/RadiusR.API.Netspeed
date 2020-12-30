using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
}
