using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RadiusR.API.Netspeed.Enums
{
    public enum ErrorCodes
    {
        Success = 0,
        AuthenticationFailed = 1,
        SubscriberNotFound = 2,
        NullObjectFound = 3,
        BillsNotFound = 4,
        HasMoreSubscription = 5,
        WrongOrInvalidBill = 6,
        AlreadyHaveCustomer = 7,
        InternalServerError = 199,
        Failed = 200
    }
}