/* Query Configuration
{
  "Using": "using Foundation.Assertions;
using Foundation.Collections.ReadOnly;
using Foundation.Data;
using Foundation.Data.SqlClient;",
  "Namespace": "OrmSamples.GetInvoices",
  "Name": "GetInvoices",
  "Results": [
    "Invoice(s)"
  ]
}
*/
-- CommandText
select  [InvoiceID],
        [CustomerID],
        [BillToCustomerID],
        [OrderID],
        [DeliveryMethodID],
        [ContactPersonID],
        [AccountsPersonID],
        [SalespersonPersonID],
        [PackedByPersonID],
        [InvoiceDate],
        [CustomerPurchaseOrderNumber],
        [IsCreditNote],
        [CreditNoteReason],
        [Comments],
        [DeliveryInstructions],
        [InternalComments],
        [TotalDryItems],
        [TotalChillerItems],
        [DeliveryRun],
        [RunPosition],
        [ReturnedDeliveryData],
        [ConfirmedDeliveryTime],
        [ConfirmedReceivedBy],
        [LastEditedBy],
        [LastEditedWhen]
from    [WideWorldImporters].[Sales].[Invoices]