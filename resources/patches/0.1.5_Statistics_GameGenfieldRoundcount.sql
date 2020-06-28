ALTER TABLE Game
ADD [roundcount] SMALLINT;

UPDATE Game
SET roundcount = length(rounds) - length(replace(rounds, ',', '')) + 1