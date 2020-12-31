using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for NetspeedServiceRequests
/// </summary>
/// 
namespace RadiusR.API.Netspeed.Requests
{
    

    [DataContract]
    public partial class NetspeedServicePayBillsRequest : BaseRequest<IEnumerable<long>, SHA1>
    {
        [DataMember]
        public IEnumerable<long> PayBillsParameters
        {
            get { return Data; }
            set { Data = value; }
        }

    }
    [DataContract]
    public partial class NetspeedServiceRequests : BaseRequest<SHA1> { }
    [DataContract]
    public partial class NetspeedServiceArrayListRequest : BaseRequest<long?, SHA1>
    {
        [DataMember]
        public long? ItemCode
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
