-- This script removes duplicate review records for the same user and book.
-- It keeps the review with the highest ReviewId (the most recent one).

DELETE FROM Reviews 
WHERE ReviewId NOT IN (
    SELECT max_id FROM (
        SELECT MAX(ReviewId) as max_id 
        FROM Reviews 
        GROUP BY UserId, BookId
    ) as tmp
);
