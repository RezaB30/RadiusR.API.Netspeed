using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for PayBills
/// </summary>
public class PayBillsRequest
{
    public IEnumerable<long> BillIds { get; set; }

}