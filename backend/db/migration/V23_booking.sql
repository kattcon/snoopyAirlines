CREATE TABLE dbo.booking (
    guid                UNIQUEIDENTIFIER
                        NOT NULL
                        CONSTRAINT df_booking_guid DEFAULT NEWSEQUENTIALID(),
    purchase_order_id   INT
                        NOT NULL,
    flight_guid         UNIQUEIDENTIFIER
                        NOT NULL,
    confirmation_code   VARCHAR(12)
                        NOT NULL,
    email               VARCHAR(254)
                        NOT NULL,
    status              VARCHAR(20)
                        NOT NULL
                        CONSTRAINT df_booking_status DEFAULT 'confirmed',
    total_amount        DECIMAL(10, 2)
                        NOT NULL,
    card_brand          VARCHAR(30)
                        NULL,
    card_last_four      CHAR(4)
                        NULL,
    card_holder_name    VARCHAR(120)
                        NULL,
    created_at          DATETIME2(0)
                        NOT NULL
                        CONSTRAINT df_booking_created_at DEFAULT SYSUTCDATETIME(),
    confirmed_at        DATETIME2(0)
                        NOT NULL
                        CONSTRAINT df_booking_confirmed_at DEFAULT SYSUTCDATETIME(),

    CONSTRAINT pk_booking PRIMARY KEY (guid),
    CONSTRAINT fk_booking_purchase_order FOREIGN KEY (purchase_order_id) REFERENCES dbo.PurchaseOrder(Id),
    CONSTRAINT fk_booking_flight FOREIGN KEY (flight_guid) REFERENCES dbo.flight(guid),
    CONSTRAINT ck_booking_status CHECK (status IN ('confirmed', 'cancelled', 'refunded')),
    CONSTRAINT ck_booking_total_amount CHECK (total_amount >= 0),
    CONSTRAINT ck_booking_email CHECK (
        email NOT LIKE '% %'
        AND email LIKE '%_@_%._%'
    ),
    CONSTRAINT ck_booking_card_last_four CHECK (
        card_last_four IS NULL
        OR card_last_four LIKE '[0-9][0-9][0-9][0-9]'
    ),
    CONSTRAINT uq_booking_purchase_order UNIQUE (purchase_order_id),
    CONSTRAINT uq_booking_confirmation_code UNIQUE (confirmation_code)
);
GO

ALTER TABLE dbo.PurchaseOrder
ADD IntendedDate DATE NULL;
GO

DECLARE @dropPurchaseOrderStatusDefaultSql NVARCHAR(MAX);

SELECT @dropPurchaseOrderStatusDefaultSql =
    N'ALTER TABLE dbo.PurchaseOrder DROP CONSTRAINT ' + QUOTENAME(default_constraints.name)
FROM sys.default_constraints
INNER JOIN sys.columns
    ON columns.object_id = default_constraints.parent_object_id
    AND columns.column_id = default_constraints.parent_column_id
WHERE default_constraints.parent_object_id = OBJECT_ID(N'dbo.PurchaseOrder')
    AND columns.name = N'Status';

EXEC sp_executesql @dropPurchaseOrderStatusDefaultSql;
GO

ALTER TABLE dbo.PurchaseOrder
DROP COLUMN Status;
GO
