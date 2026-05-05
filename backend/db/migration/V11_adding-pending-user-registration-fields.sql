ALTER TABLE pending_user
ADD
    created_at DATETIME2 NOT NULL CONSTRAINT df_pending_user_created_at DEFAULT SYSUTCDATETIME(),
    registration_key_hash VARCHAR(64) NOT NULL;
    
GO

CREATE UNIQUE INDEX ux_pending_user_registration_key_hash
ON pending_user (registration_key_hash)
WHERE registration_key_hash IS NOT NULL;
