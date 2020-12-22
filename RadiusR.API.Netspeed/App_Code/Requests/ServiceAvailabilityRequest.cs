using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ServiceAvailabilityRequest
/// </summary>
[DataContract]
public class ServiceAvailabilityRequest
{
    [DataMember]
    public string bbk { get; set; }
}