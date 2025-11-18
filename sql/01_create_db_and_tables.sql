IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'TaxiDb')
BEGIN
    CREATE DATABASE TaxiDb;
END

USE TaxiDb;
GO

IF OBJECT_ID(N'dbo.TaxiTrips', N'U') IS NOT NULL
    DROP TABLE dbo.TaxiTrips;
GO

CREATE TABLE dbo.TaxiTrips (
    Id BIGINT IDENTITY (1, 1) PRIMARY KEY,
    PickupDateTime DATETIME2 NOT NULL,
    DropoffDateTime DATETIME2 NOT NULL,
    PassengerCount TINYINT NOT NULL,
    TripDistance DECIMAL(9, 2) NOT NULL,
    StoreAndFwdFlag NVARCHAR(3) NOT NULL,
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    FareAmount DECIMAL(10, 2) NOT NULL,
    TipAmount DECIMAL(10, 2) NOT NULL
);
GO

CREATE INDEX IX_TaxiTrips_PULocation_Tip ON dbo.TaxiTrips (PULocationID, TipAmount);

CREATE INDEX IX_TaxiTrips_TripDistance ON dbo.TaxiTrips (TripDistance DESC);

CREATE INDEX IX_TaxiTrips_TravelTime ON dbo.TaxiTrips (
    PickupDateTime,
    DropoffDateTime
);

CREATE INDEX IX_TaxiTrips_PULocation_Search ON dbo.TaxiTrips (PULocationID);
GO