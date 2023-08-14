CREATE
MATERIALIZED VIEW view_periods AS
WITH all_checks AS
    (SELECT customer_id, pg.group_id, t.transaction_id, transaction_datetime, s.sku_purchase_price
          , s.sku_retail_price, sku_amount, sku_summ, sku_summ_paid, sku_discount, sku_discount / sku_summ AS Min_Discount
    FROM transactions t
    JOIN cards c on c.customer_card_id = t.customer_card_id
    JOIN checks ch on t.transaction_id = ch.transaction_id
    JOIN product_grid pg on ch.sku_id = pg.sku_id
    LEFT JOIN stores s on pg.sku_id = s.sku_id AND s.transaction_store_id = t.transaction_store_id
    WHERE transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation)
    ORDER BY 1, 2)

    ,uniq_group AS
        (SELECT customer_id, group_id, MIN(transaction_datetime) AS First_Group_Purchase_Date
                ,MAX(transaction_datetime) AS Last_Group_Purchase_Date, COUNT(transaction_id) AS Group_Purchase
        FROM all_checks
        WHERE transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation)
        GROUP BY customer_id, group_id)

    ,min_diskount AS
        (SELECT customer_id, group_id, COALESCE(MIN(Min_Discount), 0) AS Group_Min_Discount
         FROM all_checks
         WHERE sku_discount != 0 AND transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation)
        GROUP BY 1, 2)

SELECT u.customer_id
     , u.group_id
     , First_Group_Purchase_Date
     , Last_Group_Purchase_Date
     , Group_Purchase
     , (EXTRACT(epoch FROM Last_Group_Purchase_Date - First_Group_Purchase_Date) / 86400 + 1) /
       Group_Purchase AS Group_Frequency
     , Group_Min_Discount
FROM uniq_group u
         JOIN min_diskount m ON u.customer_id = m.customer_id AND u.group_id = m.group_id
ORDER BY 1, 2;

-- Test 1
UPDATE date_of_analysis_formation
SET analysis_formation = (analysis_formation - INTERVAL '15 day');
refresh
materialized view view_periods;
SELECT *
FROM view_periods;
UPDATE date_of_analysis_formation
SET analysis_formation = (analysis_formation + INTERVAL '15 day');
refresh
materialized view view_periods;
-- Test 2
SELECT *
FROM view_periods
WHERE Group_Purchase >= 6;
-- Test 3
SELECT *
FROM view_periods;