using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for SubscriberGetBillsResponse
/// </summary>
/// 
namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class SubscriberGetBillsResponse
    {
        [DataMember]
        public long ID { get; set; }
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
    [DataContract]
    public partial class NetspeedServiceSubscriberGetBillsResponse : BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>
    {
        public NetspeedServiceSubscriberGetBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
        [DataMember]
        public IEnumerable<SubscriberGetBillsResponse> SubscriberGetBillsResponse
        {
            get
            {
                return Data;
            }
            set
            {
                Data = value;
            }
        }
    }
}
