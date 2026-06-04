USE FinanceBillingDB;
GO

-- ============================================================
-- CUSTOMERS TABLE
-- ============================================================
CREATE TABLE Customers (
    CustomerId      INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(150)   NOT NULL,
    Email           NVARCHAR(200)   NOT NULL UNIQUE,
    Phone           NVARCHAR(20)    NULL,
    Address         NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    IsDeleted       BIT             NOT NULL DEFAULT 0
);
GO

-- ============================================================
-- INVOICES TABLE
-- ============================================================
CREATE TABLE Invoices (
    InvoiceId       INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber   NVARCHAR(50)    NOT NULL UNIQUE,
    CustomerId      INT             NOT NULL,
    IssueDate       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    DueDate         DATETIME2       NOT NULL,
    Status          TINYINT         NOT NULL DEFAULT 0,
                    -- 0=Draft, 1=Sent, 2=PartiallyPaid, 3=Paid, 4=Overdue, 5=Cancelled
    TotalAmount     DECIMAL(18,2)   NOT NULL DEFAULT 0,
    PaidAmount      DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Notes           NVARCHAR(1000)  NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    IsDeleted       BIT             NOT NULL DEFAULT 0,

    CONSTRAINT FK_Invoices_Customers
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),

    CONSTRAINT CK_Invoices_DueDate
        CHECK (DueDate > IssueDate),

    CONSTRAINT CK_Invoices_TotalAmount
        CHECK (TotalAmount >= 0),

    CONSTRAINT CK_Invoices_PaidAmount
        CHECK (PaidAmount >= 0)
);
GO

-- ============================================================
-- INVOICE ITEMS TABLE
-- ============================================================
CREATE TABLE InvoiceItems (
    ItemId          INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId       INT             NOT NULL,
    Description     NVARCHAR(500)   NOT NULL,
    Quantity        INT             NOT NULL,
    UnitPrice       DECIMAL(18,2)   NOT NULL,
    LineTotal       AS (Quantity * UnitPrice) PERSISTED,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NULL,
    IsDeleted       BIT             NOT NULL DEFAULT 0,

    CONSTRAINT FK_InvoiceItems_Invoices
        FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),

    CONSTRAINT CK_InvoiceItems_Quantity
        CHECK (Quantity > 0),

    CONSTRAINT CK_InvoiceItems_UnitPrice
        CHECK (UnitPrice >= 0)
);
GO

-- ============================================================
-- PAYMENTS TABLE
-- ============================================================
CREATE TABLE Payments (
    PaymentId           INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId           INT             NOT NULL,
    Amount              DECIMAL(18,2)   NOT NULL,
    PaymentDate         DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    PaymentMethod       NVARCHAR(50)    NOT NULL,
    TransactionReference NVARCHAR(100)  NULL,
    Status              TINYINT         NOT NULL DEFAULT 0,
                        -- 0=Pending, 1=Completed, 2=Failed, 3=Refunded
    CreatedAt           DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt           DATETIME2       NULL,
    IsDeleted           BIT             NOT NULL DEFAULT 0,

    CONSTRAINT FK_Payments_Invoices
        FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId),

    CONSTRAINT CK_Payments_Amount
        CHECK (Amount > 0)
);
GO

-- ============================================================
-- AUDIT LOG TABLE
-- ============================================================
CREATE TABLE AuditLogs (
    AuditId         INT IDENTITY(1,1) PRIMARY KEY,
    TableName       NVARCHAR(100)   NOT NULL,
    RecordId        INT             NOT NULL,
    Action          NVARCHAR(10)    NOT NULL, -- INSERT, UPDATE, DELETE
    OldValues       NVARCHAR(MAX)   NULL,
    NewValues       NVARCHAR(MAX)   NULL,
    ChangedBy       NVARCHAR(100)   NOT NULL DEFAULT SYSTEM_USER,
    ChangedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);
GO