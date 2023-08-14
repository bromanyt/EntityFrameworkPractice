CREATE OR REPLACE FUNCTION fn_create_groups_view(days_count BIGINT DEFAULT 1700, transactions_count BIGINT DEFAULT 1700)
    RETURNS TABLE
    (
        "Customer_id" BIGINT,
        "Group_id" BIGINT,
        "Group_Affinity_Index" NUMERIC,
        "Group_Churn_Rate" NUMERIC,
        "Group_Stability_Index" NUMERIC,
        "Group_Margin" NUMERIC,
        "Group_Discount_Share" NUMERIC,
        "Group_Minimum_Discount" NUMERIC,
        "Group_Average_Discount" NUMERIC
    )
    AS
    $$
    DECLARE
day_interval INTERVAL;
        analysis_day
TIMESTAMP = (SELECT MAX(analysis_formation) FROM date_of_analysis_formation);
BEGIN
        IF
transactions_count IS NULL THEN transactions_count := 1700;
END IF;
        IF
days_count IS NULL THEN days_count = 1700;
END IF;
        day_interval
= days_count::text || ' day';

RETURN QUERY WITH groups AS
                (SELECT c.customer_id, ch.sku_id, t.customer_card_id
                FROM checks ch
                JOIN transactions t on ch.transaction_id = t.transaction_id
                JOIN cards c on t.customer_card_id = c.customer_card_id
                JOIN personal_information pi on c.customer_id = pi.customer_id
                ORDER BY 2, c.customer_id)

            , uniq_groups AS
                (SELECT g.customer_id, pg.group_id
                 FROM groups g JOIN product_grid pg ON g.sku_id = pg.sku_id GROUP BY 1,2 ORDER BY 1,2)

            , affinity AS
                (SELECT ug.customer_id, ug.group_id
                , group_purchase::NUMERIC / COUNT(vph.transaction_id)::NUMERIC AS Group_Affinity_Index
                FROM uniq_groups ug
                JOIN view_periods vp ON ug.group_id = vp.group_id AND ug.customer_id = vp.customer_id
                JOIN view_purchase_history vph ON ug.customer_id = vph.customer_id
                    AND vph.transaction_datetime BETWEEN first_group_purchase_date AND last_group_purchase_date
                GROUP BY 1,2,group_purchase
                ORDER BY 1,2)

            , churn_rate AS
                (SELECT ug.customer_id, ug.group_id,
                     (EXTRACT( epoch FROM analysis_day - last_group_purchase_date)
                          /86400/vp.group_frequency) AS Group_Churn_Rate
                FROM uniq_groups ug
                    JOIN view_periods vp ON ug.group_id = vp.group_id AND ug.customer_id = vp.customer_id)

            , intervals AS
                (SELECT customer_id, group_id, transaction_datetime,
                (EXTRACT(epoch FROM transaction_datetime - LAG(transaction_datetime)
                    OVER (PARTITION BY customer_id, group_id ORDER BY transaction_datetime)) / 86400) AS days
                FROM view_purchase_history h ORDER BY 1, 2, 3)

            , frequency AS
                (SELECT i.customer_id, i.group_id
                     , AVG(CASE WHEN (days - p.group_frequency) < 0 THEN ((days - p.group_frequency) * - 1)/group_frequency
                         ELSE (days - p.group_frequency)/group_frequency END) AS Group_Stability_Index
                FROM intervals i
                JOIN view_periods p ON i.customer_id = p.customer_id AND i.group_id = p.group_id
                WHERE i.days IS NOT NULL
                GROUP BY 1, 2)

            , transactions_with_count AS
                (SELECT customer_id, group_id
                     , ROW_NUMBER() OVER (PARTITION BY customer_id, group_id ORDER BY customer_id,group_id, transaction_datetime) AS transaction_number
                     , transaction_id, group_summ_paid, group_cost, transaction_datetime
                FROM view_purchase_history ph
                ORDER BY 1, 2, 3)

            , margin_with_date_limit AS
                (SELECT customer_id, group_id, COUNT(ph.transaction_id), SUM(group_summ_paid) - SUM(group_cost) AS Group_Margin
                FROM transactions_with_count ph
                WHERE ph.transaction_datetime >= analysis_day - day_interval
                    AND transaction_number <= transactions_count
                GROUP BY 1, 2
                ORDER BY 1, 2)

            , discount AS (
                WITH total_discount AS
                    (SELECT vph.customer_id, vph.group_id, COUNT(sku_discount) AS number_of_purchase
                    FROM view_purchase_history vph
                    JOIN checks c on vph.transaction_id = c.transaction_id
                    WHERE sku_discount != 0
                    GROUP BY 1, 2
                    ORDER BY 1, 2)

                , discount_share AS
                    (SELECT vp.customer_id, vp.group_id
                        , (number_of_purchase / vp.group_purchase::numeric) AS Group_Discount_Share
                    FROM total_discount td
                    JOIN view_periods vp ON td.customer_id = vp.customer_id AND td.group_id = vp.group_id)

                , minimum_discount AS
                    (SELECT ds.customer_id, ds.group_id, MIN(vp.group_min_discount) AS Group_Minimum_Discount
                    FROM discount_share ds
                    JOIN view_periods vp ON ds.customer_id = vp.customer_id AND ds.group_id = vp.group_id
                    GROUP BY 1, 2)

                , average_discount AS
                    (SELECT customer_id, group_id, AVG(group_summ_paid::numeric / group_summ::numeric) AS Group_Average_Discount
                     FROM view_purchase_history
                     WHERE group_summ_paid != group_summ
                     GROUP BY 1, 2)

                SELECT av.customer_id, av.group_id, Group_Discount_Share, Group_Minimum_Discount, Group_Average_Discount
                FROM discount_share ug
                JOIN minimum_discount md ON ug.group_id = md.group_id AND ug.customer_id = md.customer_id
                JOIN average_discount av ON ug.group_id = av.group_id AND ug.customer_id = av.customer_id
            )

SELECT ug.customer_id
     , ug.group_id
     , Group_Affinity_Index
     , Group_Churn_Rate
     , Group_Stability_Index
     , Group_Margin
     , Group_Discount_Share
     , Group_Minimum_Discount
     , Group_Average_Discount
FROM uniq_groups ug
         JOIN affinity a ON ug.group_id = a.group_id AND ug.customer_id = a.customer_id
         JOIN churn_rate ch ON ug.group_id = ch.group_id AND ug.customer_id = ch.customer_id
         JOIN frequency f ON ug.group_id = f.group_id AND ug.customer_id = f.customer_id
         LEFT JOIN margin_with_date_limit m ON ug.group_id = m.group_id AND ug.customer_id = m.customer_id
         LEFT JOIN discount tmp ON ug.customer_id = tmp.customer_id AND ug.group_id = tmp.group_id
ORDER BY 1, 2;
END;
$$
LANGUAGE plpgsql;

CREATE
MATERIALIZED VIEW view_groups AS
SELECT *
FROM fn_create_groups_view(null, null);

