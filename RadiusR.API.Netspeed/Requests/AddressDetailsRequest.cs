﻿using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

namespace RadiusR.API.Netspeed.Requests
{
    [DataContract]
    public partial class NetspeedServiceAddressDetailsRequest : BaseRequest<long?, SHA1>
    {
        [DataMember]
        public long? BBK
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