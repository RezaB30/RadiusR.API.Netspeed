//using RadiusR.DB.Enums;
//using RezaB.TurkTelekom.WebServices;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Web;

///// <summary>
///// Summary description for ExistingCustomerRegister
///// </summary>
//namespace RadiusR.API.Netspeed.Requests
//{
//    [DataContract]
//    public class ExistingCustomerRegisterRequest
//    {
//        [DataMember]
//        public long SubscriberID { get; set; }
//    }
//    [DataContract]
//    public class RegistrationInfo
//    {
//        [DataMember]
//        public int? DomainID { get; set; }
//        [DataMember]
//        public int? ServiceID { get; set; }
//        [DataMember]
//        public AddressInfo SetupAddress { get; set; }
//        [DataMember]
//        public string Username { get; set; }
//        [DataMember]
//        public string StaticIP { get; set; }
//        [DataMember]
//        public int? BillingPeriod { get; set; }
//        [DataMember]
//        public IEnumerable<int> GroupIds { get; set; }
//        [DataMember]
//        public CustomerCommitmentInfo CommitmentInfo { get; set; }
//        [DataMember]
//        public IEnumerable<SubscriptionAddedFeeItem> AddedFeesInfo { get; set; }
//        [DataMember]
//        public SubscriptionTelekomInfoDetails TelekomDetailedInfo { get; set; }
//        [DataMember]
//        public RegisteringPartnerInfo RegisteringPartner { get; set; }
//        [DataMember]
//        public ReferralDiscountInfo ReferralDiscount { get; set; }
//    }
//    [DataContract]
//    public class CustomerCommitmentInfo
//    {
//        [DataMember]
//        public CommitmentLength? CommitmentLength { get; set; }
//        [DataMember]
//        public DateTime? CommitmentExpirationDate { get; set; }
//    }
//    public class ReferralDiscountInfo
//    {
//        [DataMember]
//        public string ReferenceNo { get; set; }
//        [DataMember]
//        public int? SpecialOfferID { get; set; }
//    }
//    public class RegisteringPartnerInfo
//    {
//        [DataMember]
//        public int? PartnerID { get; set; }
//        [DataMember]
//        public decimal? Allowance { get; set; }
//        [DataMember]
//        public decimal? AllowanceThreshold { get; set; }
//    }
//    public class SubscriptionTelekomInfoDetails
//    {

//        [DataMember]
//        public string SubscriberNo { get; set; }
//        [DataMember]
//        public string CustomerCode { get; set; }
//        [DataMember]
//        public string PSTN { get; set; }
//        [DataMember]
//        public SubscriptionTelekomTariffInfo TelekomTariffInfo { get; set; }
//    }
//    public class SubscriptionTelekomTariffInfo
//    {
//        [DataMember]
//        public XDSLType? XDSLType { get; set; }
//        [DataMember]
//        public int? PacketCode { get; set; }
//        [DataMember]
//        public int? TariffCode { get; set; }
//        [DataMember]
//        public bool? IsPaperworkNeeded { get; set; }
//    }
//    public class SubscriptionAddedFeeItem
//    {
//        [DataMember]
//        public FeeType? FeeType { get; set; }
//        [DataMember]
//        public int? InstallmentCount { get; set; }
//        [DataMember]
//        public int? VariantType { get; set; }
//        [DataMember]
//        public IEnumerable<SubscriptionCustomAddedFeeItem> CustomFeesInfo { get; set; }
//    }
//    public class SubscriptionCustomAddedFeeItem
//    {
//        [DataMember]
//        public string Title { get; set; }
//        [DataMember]
//        public decimal? Price { get; set; }
//        [DataMember]
//        public int? InstallmentCount { get; set; }
//    }
//}
