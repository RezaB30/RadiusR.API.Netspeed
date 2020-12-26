using RezaB.API.WebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Web;

/// <summary>
/// Summary description for NetspeedServiceResponses
/// </summary>
/// 
[DataContract]
public class NetspeedServiceAddressDetailsResponse : BaseResponse<AddressDetailsResponse, SHA1>
{
    public NetspeedServiceAddressDetailsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServicePayBillsResponse : BaseResponse<PayBillsResponse, SHA1>
{
    public NetspeedServicePayBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServicePaymentVPOSResponse : BaseResponse<PaymentVPOSResponse, SHA1>
{
    public NetspeedServicePaymentVPOSResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServiceServiceAvailabilityResponse : BaseResponse<ServiceAvailabilityResponse, SHA1>
{
    public NetspeedServiceServiceAvailabilityResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServiceSubscriberGetBillsResponse : BaseResponse<IEnumerable<SubscriberGetBillsResponse>, SHA1>
{
    public NetspeedServiceSubscriberGetBillsResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServiceArrayListResponse : BaseResponse<IEnumerable<ValueNamePair>, SHA1>
{
    public NetspeedServiceArrayListResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServiceRegisterCustomerContactResponse : BaseResponse<bool, SHA1>
{
    public NetspeedServiceRegisterCustomerContactResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}
[DataContract]
public class NetspeedServiceNewCustomerRegisterResponse : BaseResponse<Dictionary<string, string>, SHA1>
{
    public NetspeedServiceNewCustomerRegisterResponse(string passwordHash, BaseRequest<SHA1> baseRequest) : base(passwordHash, baseRequest) { }
}