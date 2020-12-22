using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ServiceAvailabilityResponse
/// </summary>
public class ServiceAvailabilityResponse
{
    public bool HasInfrastructureAdsl { get; set; }
    public bool HasInfrastructureVdsl { get; set; }
    public bool HasInfrastructureFiber { get; set; }
    public int? AdslSpeed { get; set; }
    public int? VdslSpeed { get; set; }
    public int? FiberSpeed { get; set; }
    public int? AdslDistance { get; set; }
    public int? VdslDistance { get; set; }
    public int? FiberDistance { get; set; }
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState AdslPortState { get; set; }
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState VdslPortState { get; set; }
    public RezaB.TurkTelekom.WebServices.Availability.AvailabilityServiceClient.PortState FiberPortState { get; set; }
    public string address { get; set; }
    public string AdslSVUID { get; set; }
    public string VdslSVUID { get; set; }
    public string FiberSVUID { get; set; }
    public string BBK { get; set; }
}