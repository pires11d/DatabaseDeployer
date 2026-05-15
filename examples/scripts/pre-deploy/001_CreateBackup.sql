-- Example Pre-Deploy Script: Create a backup table before schema changes
-- This script runs BEFORE the DACPAC deployment

PRINT 'Creating backup of critical customer data...';

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerBackup]') AND type in (N'U'))
BEGIN
    SELECT * 
    INTO [dbo].[CustomerBackup]
    FROM [dbo].[Customer]
    WHERE ModifiedDate >= DATEADD(DAY, -30, GETUTCDATE());

    PRINT 'Backup created successfully';
END
ELSE
BEGIN
    PRINT 'Backup table already exists, skipping';
END

GO
