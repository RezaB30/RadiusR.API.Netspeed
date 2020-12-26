using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for AddressDetailsResponse
/// </summary>
public class AddressDetailsResponse
{
    public long AddressNo { get; set; }
    public long ProvinceID { get; set; }
    public string ProvinceName { get; set; }
    public long DistrictID { get; set; }
    public string DistrictName { get; set; }
    public long RuralCode { get; set; }
    public long NeighbourhoodID { get; set; }
    public string NeighbourhoodName { get; set; }
    public long StreetID { get; set; }
    public string StreetName { get; set; }
    public long DoorID { get; set; }
    public string DoorNo { get; set; }
    public long ApartmentID { get; set; }
    public string ApartmentNo { get; set; }
    public string AddressText { get; set; }
}