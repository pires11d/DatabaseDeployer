-- Example Post-Deploy Script: Migrate data after schema changes
-- This script runs AFTER the DACPAC deployment

PRINT 'Starting customer data migration...';

-- Example: Migrate data from legacy CustomerType column to new normalized CustomerCategory table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') AND name = 'CustomerType')
    AND EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerCategory]') AND type in (N'U'))
BEGIN
    -- Insert missing categories
    INSERT INTO [dbo].[CustomerCategory] ([CategoryName], [Description])
    SELECT DISTINCT 
        c.CustomerType,
        'Migrated from legacy CustomerType column'
    FROM [dbo].[Customer] c
    WHERE NOT EXISTS (
        SELECT 1 
        FROM [dbo].[CustomerCategory] cc 
        WHERE cc.CategoryName = c.CustomerType
    )
    AND c.CustomerType IS NOT NULL;

    -- Update foreign key references
    UPDATE c
    SET c.CustomerCategoryId = cc.CategoryId
    FROM [dbo].[Customer] c
    INNER JOIN [dbo].[CustomerCategory] cc ON c.CustomerType = cc.CategoryName
    WHERE c.CustomerCategoryId IS NULL;

    PRINT 'Customer data migration completed';
END
ELSE
BEGIN
    PRINT 'Migration not needed or already completed';
END

GO

-- Example: Update reference data
PRINT 'Updating reference data...';

MERGE INTO [dbo].[ProductCategory] AS target
USING (VALUES 
    ('Electronics', 'Electronic devices and accessories', 1),
    ('Clothing', 'Apparel and fashion items', 1),
    ('Books', 'Physical and digital books', 1),
    ('Food', 'Perishable and non-perishable food items', 1)
) AS source (CategoryName, Description, IsActive)
ON target.CategoryName = source.CategoryName
WHEN MATCHED THEN
    UPDATE SET 
        Description = source.Description,
        IsActive = source.IsActive
WHEN NOT MATCHED THEN
    INSERT (CategoryName, Description, IsActive)
    VALUES (source.CategoryName, source.Description, source.IsActive);

PRINT 'Reference data updated';

GO
