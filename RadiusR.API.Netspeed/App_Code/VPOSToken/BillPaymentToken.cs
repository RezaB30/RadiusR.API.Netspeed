using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for BillPaymentToken
/// </summary>
public class BillPaymentToken : PaymentTokenBase
{
    public long? BillID { get; set; }
}