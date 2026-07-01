CREATE INDEX IX_route_airplane_lookup
ON dbo.[route] (airplane_id, is_deleted);
GO
