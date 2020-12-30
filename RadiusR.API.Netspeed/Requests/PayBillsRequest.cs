using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for PayBills
/// </summary>
/// 

namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public class PayBillsRequest
    {
        [DataMember]
        public long BillId { get; set; }

    }
}
