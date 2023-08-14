-- offers by Average Check
SELECT *
FROM fn_grow_check(1, '2018-01-20', '2022-08-20', null, 1.15, 3, 70, 30);
--no result
SELECT *
FROM fn_grow_check(1, '2023-01-20', '2023-08-20', null, 1.35, 2, 60, 30);

SELECT *
FROM fn_grow_check(2, null, null, 100, 1.15, 3, 70, 30);
--no result
SELECT *
FROM fn_grow_check(2, '2022-01-01', '2023-07-02', 5, 0.55, 3, 30, 60);


-- offers by Frequency Visits
SELECT *
FROM fn_increasing_frequency_of_visits('2018-01-20 00:00:00', '2022-08-20 00:00:00', 1, 3, 70, 30);
-- no result
SELECT *
FROM fn_increasing_frequency_of_visits('2022-08-18 00:00:00', '2022-08-18 00:00:00', 1, 1, 30, 60);


-- offers by Cross-Selling
SELECT *
FROM fn_cross_selling(5, 3, 0.5, 100, 30);
-- no result
SELECT *
FROM fn_cross_selling(1, 1, 0.7, 30, 30);

