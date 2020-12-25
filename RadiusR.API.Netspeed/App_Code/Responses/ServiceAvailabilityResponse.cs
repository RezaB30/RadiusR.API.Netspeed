using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ServiceAvailabilityResponse
/// </summary>
[DataContract]
public class ServiceAvailabilityResponse
{
    [DataMember]
    public bool HasInfrastructureAdsl { get; set; }
    [DataMember]
    public bool HasInfrastructureVdsl { get; set; }
    [DataMember]
    public bool HasInfrastructureFiber { get; set; }
    [DataMember]
    public int? AdslSpeed { get; set; }
    [DataMember]
    public int? VdslSpeed { get; set; }
    [DataMember]
    public int? FiberSpeed { get; set; }
    [DataMember]
    public int? AdslDistance { get; set; }
    [DataMember]
    public int? VdslDistance { get; set; }
    [DataMember]
    public int? FiberDistance { get; set; }
    [DataMember]
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState AdslPortState { get; set; }
    [DataMember]
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState VdslPortState { get; set; }
    [DataMember]
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState FiberPortState { get; set; }
    [DataMember]
    public string address { get; set; }
    [DataMember]
    public string AdslSVUID { get; set; }
    [DataMember]
    public string VdslSVUID { get; set; }
    [DataMember]
    public string FiberSVUID { get; set; }
    [DataMember]
    public string BBK { get; set; }
}