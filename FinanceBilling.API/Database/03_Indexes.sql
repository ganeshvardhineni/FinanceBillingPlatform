USE FinanceBillingDB;
GO

-- Fast lookup by customer
CREATE INDEX IX_Invoices_CustomerId
    ON Invoices(CustomerId);

-- Fast lookup of overdue invoices
CREATE INDEX IX_Invoices_Status_DueDate
    ON Invoices(Status, DueDate);

-- Fast lookup of payments per invoice
CREATE INDEX IX_Payments_InvoiceId
    ON Payments(InvoiceId);

-- Fast lookup of items per invoice
CREATE INDEX IX_InvoiceItems_InvoiceId
    ON InvoiceItems(InvoiceId);
GO