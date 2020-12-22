using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for CustomerContactRequest
/// </summary>
[DataContract]
public class CustomerContactRequest
{
    [DataMember]
    public int RequestTypeID { get; set; }
    [DataMember]
    public int RequestSubTypeID { get; set; }
    [DataMember]
    public string PhoneNo { get; set; }
    [DataMember]
    public string FullName { get; set; }
}