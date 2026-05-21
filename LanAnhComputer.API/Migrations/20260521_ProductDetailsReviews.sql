IF COL_LENGTH('Products', 'ShortDescription') IS NULL
    ALTER TABLE Products ADD ShortDescription NVARCHAR(500) NULL;

IF COL_LENGTH('Products', 'Description') IS NULL
    ALTER TABLE Products ADD Description NVARCHAR(MAX) NULL;

IF COL_LENGTH('Products', 'ThumbnailUrl') IS NULL
    ALTER TABLE Products ADD ThumbnailUrl NVARCHAR(500) NULL;

IF COL_LENGTH('Products', 'ViewCount') IS NULL
    ALTER TABLE Products ADD ViewCount BIGINT NOT NULL CONSTRAINT DF_Products_ViewCount DEFAULT 0;

IF COL_LENGTH('Products', 'AverageRating') IS NULL
    ALTER TABLE Products ADD AverageRating DECIMAL(3,2) NOT NULL CONSTRAINT DF_Products_AverageRating DEFAULT 0;

IF COL_LENGTH('Products', 'TotalReviews') IS NULL
    ALTER TABLE Products ADD TotalReviews INT NOT NULL CONSTRAINT DF_Products_TotalReviews DEFAULT 0;

IF COL_LENGTH('Products', 'SoldQuantity') IS NULL
    ALTER TABLE Products ADD SoldQuantity INT NOT NULL CONSTRAINT DF_Products_SoldQuantity DEFAULT 0;

IF COL_LENGTH('Products', 'LowStockThreshold') IS NULL
    ALTER TABLE Products ADD LowStockThreshold INT NOT NULL CONSTRAINT DF_Products_LowStockThreshold DEFAULT 0;

IF OBJECT_ID('ProductImages', 'U') IS NULL
BEGIN
    CREATE TABLE ProductImages
    (
        ProductImageId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductImages PRIMARY KEY,
        ProductId BIGINT NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        AltText NVARCHAR(255) NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_ProductImages_IsPrimary DEFAULT 0,
        SortOrder INT NOT NULL CONSTRAINT DF_ProductImages_SortOrder DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductImages_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT FK_ProductImages_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductImages_ProductId ON ProductImages(ProductId);
END;

IF OBJECT_ID('ProductSpecifications', 'U') IS NULL
BEGIN
    CREATE TABLE ProductSpecifications
    (
        ProductSpecificationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductSpecifications PRIMARY KEY,
        ProductId BIGINT NOT NULL,
        GroupName NVARCHAR(100) NOT NULL,
        SpecKey NVARCHAR(100) NOT NULL,
        SpecValue NVARCHAR(500) NOT NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_ProductSpecifications_SortOrder DEFAULT 0,
        CONSTRAINT FK_ProductSpecifications_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProductSpecifications_ProductId_SpecKey ON ProductSpecifications(ProductId, SpecKey);
END;

IF OBJECT_ID('ProductReviews', 'U') IS NULL
BEGIN
    CREATE TABLE ProductReviews
    (
        ProductReviewId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductReviews PRIMARY KEY,
        ProductId BIGINT NOT NULL,
        UserId BIGINT NOT NULL,
        Rating INT NOT NULL,
        Comment NVARCHAR(1000) NULL,
        IsVisible BIT NOT NULL CONSTRAINT DF_ProductReviews_IsVisible DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductReviews_CreatedAt DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_ProductReviews_Products_ProductId
            FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
        CONSTRAINT FK_ProductReviews_Users_UserId
            FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT CK_ProductReviews_Rating CHECK (Rating BETWEEN 1 AND 5)
    );

    CREATE UNIQUE INDEX IX_ProductReviews_ProductId_UserId ON ProductReviews(ProductId, UserId);
    CREATE INDEX IX_ProductReviews_ProductId_IsVisible ON ProductReviews(ProductId, IsVisible);
END;
