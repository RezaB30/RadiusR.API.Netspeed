using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        //[DataMember]
        //public string OkUrl { get; set; }
        //[DataMember]
        //public string FailUrl { get; set; }
        //[DataMember]
        //public decimal PurchaseAmount { get; set; }
        //[DataMember]
        //public string Storekey { get; set; }
        //[DataMember]
        //public string MerchantId { get; set; }
        //[DataMember]
        //public int CurrencyCode { get; set; }
        //[DataMember]
        //public string Language { get; set; }
        //[DataMember]
        //public int? InstallmentCount { get; set; }
        //[DataMember]
        //public string OrderId { get; set; }
        //[DataMember]
        //public string BillingCustomerName { get; set; }
        //[DataMember]
        //public string ActionLink { get; set; }
    }
}
