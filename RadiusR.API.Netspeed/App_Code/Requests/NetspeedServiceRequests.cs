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
[DataContract]
public class NetspeedServiceCustomerContactRequest : BaseRequest<CustomerContactRequest, SHA1> { }
[DataContract]
public class NetspeedServiceNewCustomerRegisterRequest : BaseRequest<NewCustomerRegisterRequest, SHA1> { }
[DataContract]
public class NetspeedServicePayBillsRequest : BaseRequest<PayBillsRequest, SHA1> { }
[DataContract]
public class NetspeedServiceServiceAvailabilityRequest : BaseRequest<ServiceAvailabilityRequest, SHA1> { }
[DataContract]
public class NetspeedServiceSubscriberGetBillsRequest : BaseRequest<SubscriberGetBillsRequest, SHA1> { }
[DataContract]
public class NetspeedServiceSubscriberPayBillsRequest : BaseRequest<PaymentVPOSRequest, SHA1> { }
[DataContract]
public class NetspeedServiceRequests : BaseRequest<SHA1> { }
[DataContract]
public class NetspeedServiceArrayListRequest : BaseRequest<long, SHA1> { }
[DataContract]
public class NetspeedServiceAddressDetailsRequest : BaseRequest<long, SHA1> { }
[DataContract]
public class NetspeedServicePaymentVPOSRequest : BaseRequest<PaymentVPOSRequest, SHA1> { }