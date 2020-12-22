using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Class1
/// </summary>
public enum ErrorCodes
{
    Success = 0,
    AuthenticationFailed = 1,
    SubscriberNotFound = 2,
    InternalServerError = 199,
    Failed = 200
}