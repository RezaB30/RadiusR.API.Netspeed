﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ValueNamePair
/// </summary>

namespace RadiusR.API.Netspeed.Responses
{
    [DataContract]
    public class ValueNamePair
    {
        [DataMember]
        public long Code { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
