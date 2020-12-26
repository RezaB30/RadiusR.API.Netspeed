using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for SubscriberPayBillsRequest
/// </summary>
[DataContract]
public class PaymentVPOSRequest
{
    [DataMember]
    public decimal PayableAmount { get; set; }
    [DataMember]
    public string SubscriberNo { get; set; }
    [DataMember]
    public string OkUrl { get; set; }
    [DataMember]
    public string FailUrl { get; set; }
}