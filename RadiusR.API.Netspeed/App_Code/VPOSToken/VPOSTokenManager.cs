using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

/// <summary>
/// Summary description for VPOSTokenManager
/// </summary>
public static class VPOSTokenManager
{
    private static MemoryCache _internalCahce = new MemoryCache("PaymentTokens");
    public static string RegisterPaymentToken(PaymentTokenBase token)
    {
        var key = Guid.NewGuid().ToString();
        _internalCahce.Set(key, token, DateTimeOffset.Now.AddMinutes(10));
        return key;
    }

    public static PaymentTokenBase RetrievePaymentToken(string key)
    {
        return _internalCahce.Get(key) as PaymentTokenBase;
    }
}