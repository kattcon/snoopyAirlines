CREATE FUNCTION dbo.route_flights_at (
    @departure_date DATE
)
RETURNS TABLE
AS
RETURN
    SELECT
        route.id AS route_id,
        DATEADD(
            SECOND,
            DATEDIFF(SECOND, CONVERT(time(0), '00:00:00'), route.departure_time),
            CONVERT(datetime2(0), @departure_date)
        ) AS departure_at,
        DATEADD(
            SECOND,
            DATEDIFF(SECOND, CONVERT(time(0), '00:00:00'), route.arrival_time),
            DATEADD(
                DAY,
                CASE WHEN route.arrival_time >= route.departure_time THEN 0 ELSE 1 END,
                CONVERT(datetime2(0), @departure_date)
            )
        ) AS arrival_at
    FROM dbo.[route] AS route
    WHERE (route.frequency & CASE ((DATEDIFF(DAY, CONVERT(date, '19000101'), @departure_date) % 7) + 7) % 7
        WHEN 0 THEN 64 -- Monday
        WHEN 1 THEN 32 -- Tuesday
        WHEN 2 THEN 16 -- Wednesday
        WHEN 3 THEN 8  -- Thursday
        WHEN 4 THEN 4  -- Friday
        WHEN 5 THEN 2  -- Saturday
        WHEN 6 THEN 1  -- Sunday
    END) <> 0;
GO
