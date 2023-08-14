CREATE OR REPLACE FUNCTION fn_average_check_segment()
    RETURNS TABLE (
                    "Customer_id" BIGINT,
                    "Customer_Average_Check" NUMERIC,
                    "Customer_Average_Check_Segment" TEXT
                  )
AS
$BODY$
DECLARE
size INTEGER = (SELECT COUNT(c.customer_id) FROM cards c);
    high
INTEGER = ROUND(size * 0.1);
    medium
INTEGER = ROUND((size - high) * 0.25);
BEGIN
RETURN QUERY
    (
    WITH average_check AS
            (SELECT c.customer_id, (SUM(t.transaction_summ) / COUNT(t.transaction_summ))::NUMERIC AS Customer_Average_Check
        FROM cards c
        JOIN transactions t on c.customer_card_id = t.customer_card_id
        GROUP BY c.customer_id
        ORDER BY Customer_Average_Check DESC)

        SELECT *, CASE
                          WHEN ROW_NUMBER() OVER (ORDER BY Customer_Average_Check DESC) < high THEN 'High'
                          WHEN ROW_NUMBER() OVER (ORDER BY Customer_Average_Check DESC) < medium THEN 'Medium'
                          ELSE 'Low'
            END FROM average_check
    );
END;
$BODY$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_frequency_segment()
    RETURNS TABLE (
                    "Customer_id" BIGINT,
                    "Customer_Frequency" NUMERIC,
                    "Customer_Frequency_Segment" TEXT
                  )
AS
$BODY$
DECLARE
size INTEGER = (SELECT COUNT(c.customer_id) FROM cards c);
    high
INTEGER = ROUND(size * 0.1);
    medium
INTEGER = ROUND((size - high) * 0.25);
BEGIN
RETURN QUERY
    (
    WITH customers_transactions AS
                (SELECT c.customer_id,
                        (MAX(t.transaction_datetime)::date - MIN(t.transaction_datetime)::date)
                            / COUNT(t.transaction_datetime)::NUMERIC AS customer_frequency
                FROM transactions t
                JOIN cards c on c.customer_card_id = t.customer_card_id
                GROUP BY c.customer_id
                ORDER BY customer_frequency)

            SELECT ct.customer_id
                , ct.customer_frequency
    , CASE
          WHEN ROW_NUMBER() OVER (ORDER BY customer_frequency) < high THEN 'Often'
          WHEN ROW_NUMBER() OVER (ORDER BY customer_frequency) < medium THEN 'Occasionally'
          ELSE 'Rarely'
                          END FROM customers_transactions ct
            ORDER BY 2
    );
END;
$BODY$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_segment_number(Average TEXT, Frequency TEXT, Churn TEXT)
    RETURNS SETOF INTEGER
AS
$BODY$
DECLARE
code INTEGER = 0;
BEGIN
CASE Average
        WHEN 'Low' THEN code := 0;
WHEN 'Medium' THEN code := 9;
WHEN 'High' THEN code := 18;
END
CASE;
CASE
    WHEN Frequency = 'Rarely' AND Churn = 'Low' THEN code := code + 1;
    WHEN Frequency = 'Rarely' AND Churn = 'Medium' THEN code := code + 2;
    WHEN Frequency = 'Rarely' AND Churn = 'High' THEN code := code + 3;
    WHEN Frequency = 'Occasionally' AND Churn = 'Low' THEN code := code + 4;
    WHEN Frequency = 'Occasionally' AND Churn = 'Medium' THEN code := code + 5;
    WHEN Frequency = 'Occasionally' AND Churn = 'High' THEN code := code + 6;
    WHEN Frequency = 'Often' AND Churn = 'Low' THEN code := code + 7;
    WHEN Frequency = 'Often' AND Churn = 'Medium' THEN code := code + 8;
    WHEN Frequency = 'Often' AND Churn = 'High' THEN code := code + 9;
END CASE;
    RETURN
NEXT code;
END
$BODY$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_get_primary_store(customer BIGINT)
    RETURNS SETOF INTEGER
AS
$BODY$
    DECLARE
value INTEGER = 0;
        date_of_analysis
TIMESTAMP = (SELECT MAX(analysis_formation) FROM date_of_analysis_formation LIMIT 1);
BEGIN
value :=
        (WITH temp AS (SELECT transaction_datetime, transaction_store_id AS num
                    FROM cards
                    JOIN transactions t on cards.customer_card_id = t.customer_card_id
                    WHERE t.transaction_datetime <= date_of_analysis
                        AND customer_id = customer
                    ORDER BY 1 DESC LIMIT 3)
        SELECT
               CASE
                WHEN (SELECT num FROM temp WHERE t.num != num LIMIT 1) IS NULL
                    THEN (SELECT num FROM temp LIMIT 1)
                ELSE 0
               END
        FROM temp t LIMIT 1);

    IF
value != 0 THEN RETURN NEXT value;
END IF;
END;
$BODY$
LANGUAGE plpgsql;

CREATE MATERIALIZED VIEW view_customers AS
        WITH
        inactive AS
        (
            SELECT c.customer_id as "Customer_id", MAX(t.transaction_datetime),
            (date_part('epoch',(SELECT MAX(analysis_formation) FROM date_of_analysis_formation LIMIT 1) - MAX(t.transaction_datetime)) / 86400)::numeric AS "Inactive_Period"
            FROM transactions t
            JOIN cards c on c.customer_card_id = t.customer_card_id
            GROUP BY c.customer_id, (SELECT MAX(analysis_formation) FROM date_of_analysis_formation LIMIT 1)
            ORDER BY 1
        ),
        parts AS
        (
            SELECT cs."Customer_id", "Customer_Average_Check", "Customer_Average_Check_Segment",
                "Customer_Frequency", "Customer_Frequency_Segment", ip."Inactive_Period"
                , (ip."Inactive_Period" / "Customer_Frequency") AS "Customer_Churn_Rate",
                        (CASE
                            WHEN ip."Inactive_Period" / "Customer_Frequency" BETWEEN 0 AND 2 THEN 'Low'
                            WHEN ip."Inactive_Period" / "Customer_Frequency" BETWEEN 2 AND 5 THEN 'Medium'
                            WHEN ip."Inactive_Period" / "Customer_Frequency" > 5 THEN 'High'
                        END) AS "Customer_Churn_Segment"
            FROM fn_average_check_segment() cs
            JOIN fn_frequency_segment() fs ON cs."Customer_id" = fs."Customer_id"
            JOIN inactive ip ON cs."Customer_id" = ip."Customer_id"
            ORDER BY 1
        ),
        all_stores AS
        (WITH stores AS
            (SELECT customer_id, transaction_store_id, COUNT(transaction_id) AS shop_transactions,
            MAX(t.transaction_datetime) AS last
            FROM cards
            JOIN transactions t on cards.customer_card_id = t.customer_card_id
            WHERE t.transaction_datetime <= (SELECT MAX(analysis_formation) FROM date_of_analysis_formation LIMIT 1)
            GROUP BY 1, 2
            ORDER BY 1, 2),

            total_transactions AS
                (SELECT customer_id, SUM(shop_transactions) AS total FROM stores GROUP BY customer_id ORDER BY 1)

            SELECT s.customer_id, transaction_store_id, shop_transactions / tt.total AS transactions_part, last
            FROM stores s
            JOIN total_transactions tt ON tt.customer_id = s.customer_id
            ORDER BY 1, 3 DESC, 4 DESC
        ),
        primary_store AS
            (SELECT * , fn_segment_number("Customer_Average_Check_Segment","Customer_Frequency_Segment", "Customer_Churn_Segment") AS "Customer_Segment"
                    , fn_get_primary_store("Customer_id") AS visits
                    , (SELECT s.transaction_store_id FROM all_stores s WHERE p."Customer_id" = s.customer_id LIMIT 1) AS transactions_part
            FROM parts p)

SELECT "Customer_id",
       "Customer_Average_Check",
       "Customer_Average_Check_Segment",
       "Customer_Frequency",
       "Customer_Frequency_Segment",
       "Inactive_Period",
       "Customer_Churn_Rate",
       "Customer_Churn_Segment",
       "Customer_Segment",
       COALESCE(visits, transactions_part) AS "Customer_Primary_Store"
FROM primary_store;
