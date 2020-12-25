﻿using RadiusR.DB.Enums;
using RezaB.TurkTelekom.WebServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for SubscriptionRegisterRequest
/// </summary>
[DataContract]
public class NewCustomerRegisterRequest
{
    [DataMember]
    public IDCardInfo IDCardInfo { get; set; }
    [DataMember]
    public CustomerGeneralInfo CustomerGeneralInfo { get; set; }
    [DataMember]
    public IndividualCustomerInfo IndividualCustomerInfo { get; set; }
    [DataMember]
    public CorporateCustomerInfo CorporateCustomerInfo { get; set; }
    [DataMember]
    public SubscriptionRegistrationInfo SubscriptionInfo { get; set; }

}
[DataContract]
public class IDCardInfo
{
    [DataMember]
    public IDCardTypes? CardType { get; set; }
    [DataMember]
    public string PassportNo { get; set; }
    [DataMember]
    public string FirstName { get; set; }
    [DataMember]
    public string LastName { get; set; }
    [DataMember]
    public string TCKNo { get; set; }
    [DataMember]
    public string VolumeNo { get; set; }
    [DataMember]
    public string RowNo { get; set; }
    [DataMember]
    public string PageNo { get; set; }
    [DataMember]
    public string Province { get; set; }
    [DataMember]
    public string District { get; set; }
    [DataMember]
    public string Neighbourhood { get; set; }
    [DataMember]
    public string SerialNo { get; set; }
    [DataMember]
    public string PlaceOfIssue { get; set; }
    [DataMember]
    public DateTime? DateOfIssue { get; set; }
    [DataMember]
    public DateTime? BirthDate { get; set; }
}
[DataContract]
public class CustomerGeneralInfo
{
    [DataMember]
    public CustomerType? CustomerType { get; set; }
    [DataMember]
    public string Email { get; set; }
    [DataMember]
    public string Culture { get; set; }
    [DataMember]
    public string ContactPhoneNo { get; set; }
    [DataMember]
    public IEnumerable<PhoneNoListItem> OtherPhoneNos { get; set; }
    [DataMember]
    public AddressInfo BillingAddress { get; set; }
}
[DataContract]
public class PhoneNoListItem
{
    [DataMember]
    public string Number { get; set; }
}
[DataContract]
public class AddressInfo
{
    [DataMember]
    public string AddressText { get; set; }
    [DataMember]
    public string StreetName { get; set; }
    [DataMember]
    public string NeighbourhoodName { get; set; }
    [DataMember]
    public string DistrictName { get; set; }
    [DataMember]
    public string ProvinceName { get; set; }
    [DataMember]
    public long? AddressNo { get; set; }
    [DataMember]
    public string Floor { get; set; }
    [DataMember]
    public int? PostalCode { get; set; }
    [DataMember]
    public string ApartmentNo { get; set; }
    [DataMember]
    public long? ApartmentID { get; set; }
    [DataMember]
    public long? DoorID { get; set; }
    [DataMember]
    public long? StreetID { get; set; }
    [DataMember]
    public long? NeighbourhoodID { get; set; }
    [DataMember]
    public long? RuralCode { get; set; }
    [DataMember]
    public long? DistrictID { get; set; }
    [DataMember]
    public long? ProvinceID { get; set; }
    [DataMember]
    public string DoorNo { get; set; }
}
[DataContract]
public class IndividualCustomerInfo
{
    [DataMember]
    public Sexes? Sex { get; set; }
    [DataMember]
    public CountryCodes? Nationality { get; set; }
    [DataMember]
    public string FathersName { get; set; }
    [DataMember]
    public string MothersName { get; set; }
    [DataMember]
    public string MothersMaidenName { get; set; }
    [DataMember]
    public string BirthPlace { get; set; }
    [DataMember]
    public Profession? Profession { get; set; }
    [DataMember]
    public AddressInfo ResidencyAddress { get; set; }
}
[DataContract]
public class CorporateCustomerInfo
{

    [DataMember]
    public string Title { get; set; }
    [DataMember]
    public string TaxNo { get; set; }
    [DataMember]
    public string TaxOffice { get; set; }
    [DataMember]
    public string CentralSystemNo { get; set; }
    [DataMember]
    public string TradeRegistrationNo { get; set; }
    [DataMember]
    public string ExecutiveFathersName { get; set; }
    [DataMember]
    public string ExecutiveMothersName { get; set; }
    [DataMember]
    public string ExecutiveMothersMaidenName { get; set; }
    [DataMember]
    public Sexes? ExecutiveSex { get; set; }
    [DataMember]
    public CountryCodes? ExecutiveNationality { get; set; }
    [DataMember]
    public string ExecutiveBirthPlace { get; set; }
    [DataMember]
    public Profession? ExecutiveProfession { get; set; }
    [DataMember]
    public AddressInfo ExecutiveResidencyAddress { get; set; }
    [DataMember]
    public AddressInfo CompanyAddress { get; set; }
}
[DataContract]
public class SubscriptionRegistrationInfo
{
    [DataMember]
    public int? DomainID { get; set; }
    [DataMember]
    public int? ServiceID { get; set; }
    [DataMember]
    public AddressInfo SetupAddress { get; set; }
    //public string Username { get; set; }
    //public string StaticIP { get; set; }
    [DataMember]
    public int? BillingPeriod { get; set; }
    //public IEnumerable<int> GroupIds { get; set; }
    //public CustomerCommitmentInfo CommitmentInfo { get; set; }
    //public IEnumerable<SubscriptionAddedFeeItem> AddedFeesInfo { get; set; }
    //public SubscriptionTelekomInfoDetails TelekomDetailedInfo { get; set; }
    //public RegisteringPartnerInfo RegisteringPartner { get; set; }
    //public ReferralDiscountInfo ReferralDiscount { get; set; }
}
//public class CustomerCommitmentInfo
//{
//    public CommitmentLength? CommitmentLength { get; set; }
//    public DateTime? CommitmentExpirationDate { get; set; }
//}
//public class RegisteringPartnerInfo
//{
//    public int? PartnerID { get; set; }
//    public decimal? Allowance { get; set; }
//    public decimal? AllowanceThreshold { get; set; }
//}
//public class ReferralDiscountInfo
//{
//    public string ReferenceNo { get; set; }
//    public int? SpecialOfferID { get; set; }
//}
//public class SubscriptionTelekomInfoDetails
//{
//    public string SubscriberNo { get; set; }
//    public string CustomerCode { get; set; }
//    public string PSTN { get; set; }
//    public SubscriptionTelekomTariffInfo TelekomTariffInfo { get; set; }
//}
//public class SubscriptionTelekomTariffInfo
//{
//    public XDSLType? XDSLType { get; set; }
//    public int? PacketCode { get; set; }
//    public int? TariffCode { get; set; }
//    public bool? IsPaperworkNeeded { get; set; }
//}
//public class SubscriptionAddedFeeItem
//{
//    public FeeType? FeeType { get; set; }
//    public int? InstallmentCount { get; set; }
//    public int? VariantType { get; set; }
//    public IEnumerable<SubscriptionCustomAddedFeeItem> CustomFeesInfo { get; set; }

//}
//public class SubscriptionCustomAddedFeeItem
//{
//    public string Title { get; set; }
//    public decimal? Price { get; set; }
//    public int? InstallmentCount { get; set; }
//}