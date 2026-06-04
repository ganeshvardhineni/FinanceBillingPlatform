USE FinanceBillingDB;
GO

-- ============================================================
-- TRIGGER 1: Auto-update Invoice TotalAmount when items change
-- ============================================================
CREATE TRIGGER trg_InvoiceItems_UpdateTotal
ON InvoiceItems
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InvoiceId INT;

    -- Get InvoiceId from inserted or deleted
    SELECT @InvoiceId = COALESCE(
        (SELECT TOP 1 InvoiceId FROM inserted),
        (SELECT TOP 1 InvoiceId FROM deleted)
    );

    -- Recalculate and update TotalAmount on the invoice
    UPDATE Invoices
    SET
        TotalAmount = (
            SELECT ISNULL(SUM(LineTotal), 0)
            FROM InvoiceItems
            WHERE InvoiceId = @InvoiceId
              AND IsDeleted = 0
        ),
        UpdatedAt = GETUTCDATE()
    WHERE InvoiceId = @InvoiceId;
END;
GO

-- ============================================================
-- TRIGGER 2: Auto-update Invoice PaidAmount when payments change
-- ============================================================
CREATE TRIGGER trg_Payments_UpdatePaidAmount
ON Payments
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @InvoiceId INT;

    SELECT @InvoiceId = InvoiceId FROM inserted;

    UPDATE Invoices
    SET
        PaidAmount = (
            SELECT ISNULL(SUM(Amount), 0)
            FROM Payments
            WHERE InvoiceId = @InvoiceId
              AND Status = 1  -- Completed only
              AND IsDeleted = 0
        ),
        UpdatedAt = GETUTCDATE()
    WHERE InvoiceId = @InvoiceId;
END;
GO

-- ============================================================
-- TRIGGER 3: Audit log on Invoice changes
-- ============================================================
CREATE TRIGGER trg_Invoices_Audit
ON Invoices
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditLogs (TableName, RecordId, Action, OldValues, NewValues)
    SELECT
        'Invoices',
        i.InvoiceId,
        'UPDATE',
        'Status:' + CAST(d.Status AS NVARCHAR) + ',PaidAmount:' + CAST(d.PaidAmount AS NVARCHAR),
        'Status:' + CAST(i.Status AS NVARCHAR) + ',PaidAmount:' + CAST(i.PaidAmount AS NVARCHAR)
    FROM inserted i
    JOIN deleted d ON i.InvoiceId = d.InvoiceId;
END;
GO