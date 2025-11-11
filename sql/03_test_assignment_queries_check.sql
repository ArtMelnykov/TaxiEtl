USE TaxiDb;
GO

-- 1. PULocationID the highest tip_amount on average
SELECT TOP 1 PULocationID, AVG(TipAmount) AS AvgTipAmount
FROM dbo.TaxiTrips
GROUP BY
    PULocationID
ORDER BY AvgTipAmount DESC;
GO

-- 2. top 100 longest fares in terms of trip_distance
SELECT TOP 100 * FROM dbo.TaxiTrips ORDER BY TripDistance DESC;
GO

-- 3. top 100 longest fares in terms of time spent traveling
SELECT TOP 100 *, DATEDIFF(
        SECOND, PickupDateTime, DropoffDateTime
    ) AS TravelSeconds
FROM dbo.TaxiTrips
ORDER BY TravelSeconds DESC;
GO

-- 4. search, where part of the conditions is PULocationId
DECLARE @PULocationId INT = 132;

SELECT *
FROM dbo.TaxiTrips
WHERE
    PULocationID = @PULocationId
ORDER BY PickupDateTime;