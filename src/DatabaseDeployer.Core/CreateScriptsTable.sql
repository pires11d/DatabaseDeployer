IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Scripts' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Scripts (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Filename VARCHAR(500) NOT NULL,
        UpgradeScript VARCHAR(MAX) NOT NULL,
        DowngradeScript VARCHAR(MAX) NULL,
        PreDeploy BIT NOT NULL DEFAULT 0,
        Executed BIT NOT NULL DEFAULT 0,
        ExecutedDate DATETIME NULL,
        CONSTRAINT UQ_Scripts_Filename UNIQUE (Filename)
    );
    
    CREATE INDEX IX_Scripts_Executed ON dbo.Scripts(Executed, PreDeploy);
END
