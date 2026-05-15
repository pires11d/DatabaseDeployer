-- Downgrade: Remove seeded data
DELETE FROM dbo.Orders WHERE OrderId > 0;
GO

DELETE FROM dbo.Products WHERE ProductId IN (SELECT TOP 3 ProductId FROM dbo.Products ORDER BY ProductId);
GO
