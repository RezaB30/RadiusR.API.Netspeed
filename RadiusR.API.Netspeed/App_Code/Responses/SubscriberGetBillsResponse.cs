using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for SubscriberGetBillsResponse
/// </summary>
[DataContract]
public class SubscriberGetBillsResponse
{
    [DataMember]
    public long ID { get; set; }
    [DataMember]
    public string BillName { get; set; }
    [DataMember]
    public string ServiceName { get; set; }
    [DataMember]
    public DateTime BillDate { get; set; }
    [DataMember]
    public DateTime LastPaymentDate { get; set; }
    [DataMember]
    public string Total { get; set; }
    [DataMember]
    public short Status { get; set; } // billstate enum
    [DataMember]
    public bool CanBePaid { get; set; }
    [DataMember]
    public bool HasEArchiveBill { get; set; }
}