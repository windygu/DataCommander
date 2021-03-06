/* Query Configuration
{
  "Using": "using Foundation.Assertions;
using Foundation.Collections.ReadOnly;
using Foundation.Data;
using Foundation.Data.SqlClient;",
  "Namespace": "OrmSamples.GetCustomerInvoices",
  "Name": "GetCustomerInvoices",
  "Results": [
    "Customer(s)",
    "Invoice(s)"
  ]
}
*/
declare @customerId int /*not null*/ = 1
declare @invoiceDate date /*not null*/ = '20160101'
-- CommandText
select
    c.CustomerID,
    c.CustomerName
from Sales.Customers c
where c.CustomerID = @customerId

select
    i.InvoiceID,
    i.CustomerID,
    i.InvoiceDate
from Sales.Invoices i
where
    i.CustomerID = @customerId
    and i.InvoiceDate >= @invoiceDate
order by i.CustomerID,i.InvoiceID