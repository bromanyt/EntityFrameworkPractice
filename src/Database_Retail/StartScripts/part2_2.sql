CREATE MATERIALIZED VIEW view_purchase_history AS
WITH all_checks AS
    (SELECT customer_id, t.transaction_id, transaction_datetime, pg.group_id, s.sku_purchase_price
          , s.sku_retail_price, sku_amount, sku_summ, sku_summ_paid, sku_discount
          , s.sku_purchase_price * sku_amount AS "Group_Cost"
    FROM transactions t
    JOIN cards c on c.customer_card_id = t.customer_card_id
    JOIN checks ch on t.transaction_id = ch.transaction_id
    JOIN product_grid pg on ch.sku_id = pg.sku_id
    LEFT JOIN stores s on pg.sku_id = s.sku_id AND s.transaction_store_id = t.transaction_store_id
    WHERE transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation)
    ORDER BY 1, 2),

    uniq_group_checks AS
        (SELECT customer_id, transaction_id, transaction_datetime, group_id,
                SUM("Group_Cost") AS group_cost, SUM(sku_summ) AS group_summ,
                SUM(sku_summ_paid) AS group_summ_paid
        FROM all_checks
        WHERE transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation)
        GROUP BY customer_id, transaction_id, transaction_datetime, group_id)

SELECT *
FROM uniq_group_checks
ORDER BY 1, 2, 3;
