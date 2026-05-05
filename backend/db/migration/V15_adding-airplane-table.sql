ALTER TABLE airplane
ADD 
	model VARCHAR(50) NOT NULL, --convert to primary key
	tourist_rows INT NOT NULL,
	tourist_columns INT NOT NULL,
	firstClass_rows INT NOT NULL,
	firstClass_columns INT NOT NULL,
	max_weight FLOAT NOT NULL,

	CONSTRAINT CHK_airplane_tourist_rows CHECK (tourist_rows > 0),
	CONSTRAINT CHK_airplane_tourist_columns CHECK (tourist_columns > 0),
	CONSTRAINT CHK_airplane_firstClass_rows CHECK (firstClass_rows > 0),
	CONSTRAINT CHK_airplane_firstClass_columns CHECK (firstClass_columns > 0),
	CONSTRAINT CHK_airplane_max_weight CHECK (max_weight > 0),

	CONSTRAINT CHK_airplane_total_seats CHECK ((tourist_rows * tourist_columns) +
				(firstClass_rows * firstClass_columns) < 999),

	CONSTRAINT CHK_model_spaces CHECK (model NOT LIKE '% %');
