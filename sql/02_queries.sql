USE [TaxiDb];
GO

USE master;

-- Count total amount of rows
SELECT COUNT(*) AS TotalRows FROM dbo.TaxiTrips;

-- Check duplicates, if not exist than output is empty
SELECT
    PickupDateTime,
    DropoffDateTime,
    PassengerCount,
    COUNT(*) AS Cnt
FROM dbo.TaxiTrips
GROUP BY
    PickupDateTime,
    DropoffDateTime,
    PassengerCount
HAVING
    COUNT(*) > 1;

-- Check if TaxiTrips table is not empty, if yes than drop table
IF OBJECT_ID('dbo.TaxiTrips', 'U') IS NOT NULL
DROP TABLE dbo.TaxiTrips;
GO

-- Create new TaxiTrips table
CREATE TABLE dbo.TaxiTrips (
    Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
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