-- Upgrade: Create new tables
CREATE TABLE dbo.Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE dbo.Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    OrderDate DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Orders_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);
GO
