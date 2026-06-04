USE FinanceBillingDB;
GO

SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

SELECT
    i.InvoiceNumber,
    c.FullName AS Customer,
    i.TotalAmount,
    i.PaidAmount,
    (i.TotalAmount - i.PaidAmount) AS BalanceDue,
    i.Status
FROM Invoices i
JOIN Customers c ON i.CustomerId = c.CustomerId;

SELECT * FROM AuditLogs;