-- V16_adding-airplane-data.sql
-- Adds sample airplane data to enable flight assignments
-- Airplane table currently only has id (identity), so using default values

--INSERT INTO airplane DEFAULT VALUES; -- Airplane 1
--INSERT INTO airplane DEFAULT VALUES; -- Airplane 2
--INSERT INTO airplane DEFAULT VALUES; -- Airplane 3

DELETE FROM airplane WHERE id IN (1, 2, 3);

INSERT INTO airplane (model, tourist_rows, tourist_columns, firstClass_rows, firstClass_columns, max_weight) VALUES
('Boeing737', 25, 6, 5, 4, 79016),
('Boeing747', 40, 6, 8, 4, 412775),
('AirbusA320', 28, 6, 4, 4, 78000);

