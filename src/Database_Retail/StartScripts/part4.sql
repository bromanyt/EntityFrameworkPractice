CREATE OR REPLACE FUNCTION fn_customer_average_check(CUSTOMER_ID_ BIGINT, DATE_FROM DATE default null,
                                                  DATE_TILL DATE default null,
                                                  TRANSACTIONS_NUMBER INTEGER DEFAULT null)
    RETURNS DECIMAL
AS
$$
DECLARE
    v_result DECIMAL;
BEGIN
    CASE
        WHEN DATE_FROM IS NOT NULL AND DATE_TILL IS NOT NULL THEN
            SELECT (SUM(transaction_summ) / COUNT(*)) ::DECIMAL
            FROM checks
            JOIN transactions t on t.transaction_id = checks.transaction_id
            JOIN cards c on c.customer_card_id = t.customer_card_id
            JOIN personal_information pi on pi.customer_id = c.customer_id
            WHERE pi.customer_id = CUSTOMER_ID_ AND t.transaction_datetime BETWEEN DATE_FROM AND DATE_TILL
            INTO v_result;
        WHEN TRANSACTIONS_NUMBER IS NOT NULL THEN WITH customer_transactions AS
                            (SELECT *
                             FROM transactions t
                                      JOIN cards c on c.customer_card_id = t.customer_card_id
                                      JOIN personal_information pi on pi.customer_id = c.customer_id
                             WHERE pi.customer_id = CUSTOMER_ID_
                             ORDER BY t.transaction_datetime DESC
                             LIMIT TRANSACTIONS_NUMBER)
            SELECT ROUND((SUM(transaction_summ) / COUNT(*)):: DECIMAL, 2)
            FROM customer_transactions
            INTO v_result;
        END CASE;
    RETURN v_result;
END;
$$
    LANGUAGE plpgsql;

CREATE
    OR REPLACE FUNCTION fn_round_five(AMOUNT DECIMAL) RETURNS DECIMAL AS
$$
BEGIN
    AMOUNT
        = FLOOR(AMOUNT / 5) * 5;
    IF
        AMOUNT = 0 THEN
        AMOUNT = 5;
    END IF;
    RETURN AMOUNT;
END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_offer_discount(CUSTOMER_ID_ BIGINT, ALLOWABLE_DISCOUNT DECIMAL, OFFERED_GROUPS BIGINT[])
    RETURNS BIGINT[]
AS
$$
DECLARE
    v_minimal_discount DECIMAL = 1;
    v_maximal_discount
                       DECIMAL = 0;
    v_result
                       BIGINT[];
BEGIN


    WHILE
        v_minimal_discount > v_maximal_discount
        LOOP
            SELECT "Group_Average_Discount" * (ALLOWABLE_DISCOUNT)
            FROM view_groups
            WHERE "Group_id" = OFFERED_GROUPS[1]
              AND "Customer_id" = CUSTOMER_ID_
            INTO v_maximal_discount;

            SELECT fn_round_five("Group_Minimum_Discount" * 100)
            FROM view_groups
            WHERE "Group_id" = OFFERED_GROUPS[2]
              AND "Customer_id" = CUSTOMER_ID_
            INTO v_minimal_discount;
            IF
                v_minimal_discount > v_maximal_discount THEN
                OFFERED_GROUPS = array_remove(OFFERED_GROUPS, OFFERED_GROUPS[1]);
            END IF;
        END LOOP;
    v_result
        := array_append(v_result, v_minimal_discount);
    v_result
        := array_append(v_result, OFFERED_GROUPS[2]);
    RETURN v_result;

END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_offer_group_list(CUSTOMER_ID_ BIGINT, CHURN_INDEX DECIMAL, SHARE_WITH_DISCOUNT DECIMAL)
    RETURNS BIGINT[]
AS
$$
DECLARE
    group_affinity_list BIGINT[];

BEGIN
    WITH groups AS (SELECT "Group_id", "Group_Affinity_Index"
                    FROM view_groups
                    WHERE CUSTOMER_ID_ = "Customer_id"
                      AND "Group_Churn_Rate" < CHURN_INDEX
                      AND "Group_Discount_Share" < SHARE_WITH_DISCOUNT / 100:: DECIMAL
                    GROUP BY "Group_Affinity_Index", "Group_id"
                    ORDER BY "Group_Affinity_Index" DESC)
    SELECT array_agg("Group_id")
    INTO group_affinity_list
    FROM groups;
    RETURN group_affinity_list;
END;
$$
    LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION fn_grow_check(METHOD INTEGER, DATE_FROM DATE default null, DATE_TILL DATE default null,
                                      TRANSACTIONS_NUMBER INTEGER default 0,
                                      COEFFICIENT DECIMAL default 0, MAX_CHURN_INDEX INTEGER default 0,
                                      SHARE_DISCOUNT DECIMAL default 0, MAX_DISCOUNT DECIMAL default 0)
    RETURNS TABLE
            (
                "Customer_Id"            BIGINT,
                "Required_Check_Measure" DECIMAL,
                "Offer_Group"            TEXT,
                "Offer_Discount_Depth"   DECIMAL
            )
AS
$BODY$
DECLARE
    v_customer_id BIGINT;
    v_offer_group_list
                  BIGINT[];
    v_offer_group
                  TEXT;
    v_offer_discount
                  BIGINT[];
BEGIN
    CREATE
        TEMP TABLE t_result
    (
        "Customer_Id"            BIGINT,
        "Required_Check_Measure" DECIMAL,
        "Offer_Group"            TEXT,
        "Offer_Discount_Depth"   DECIMAL
    );
    FOR v_customer_id IN
        SELECT view_customers."Customer_id"
        FROM view_customers
        LOOP
            CASE
                WHEN METHOD = 1
                    THEN v_offer_group_list := fn_offer_group_list(v_customer_id, MAX_CHURN_INDEX, SHARE_DISCOUNT);
                         v_offer_discount
                             := fn_offer_discount(v_customer_id, MAX_DISCOUNT, v_offer_group_list);
                         v_offer_group
                             := (SELECT group_name FROM sku_group WHERE group_id = v_offer_discount[2]);
                         INSERT INTO t_result
                         SELECT v_customer_id,
                                fn_customer_average_check(v_customer_id,
                                                          DATE_FROM, DATE_TILL,
                                                          null) * COEFFICIENT,
                                v_offer_group,
                                v_offer_discount[1];
                WHEN METHOD = 2
                    THEN v_offer_group_list := fn_offer_group_list(v_customer_id, MAX_CHURN_INDEX, SHARE_DISCOUNT);
                         v_offer_discount
                             := fn_offer_discount(v_customer_id, MAX_DISCOUNT, v_offer_group_list);
                         v_offer_group
                             := (SELECT group_name FROM sku_group WHERE group_id = v_offer_discount[2]);
                         INSERT INTO t_result
                         SELECT v_customer_id,
                                fn_customer_average_check(v_customer_id,
                                                          null, null,
                                                          TRANSACTIONS_NUMBER) * COEFFICIENT,
                                v_offer_group,
                                v_offer_discount[1];
            END CASE;
        END LOOP;
    RETURN QUERY SELECT *
                 FROM t_result
                 WHERE t_result."Required_Check_Measure" IS NOT NULL
                   AND t_result."Offer_Group" IS NOT NULL;
    DROP TABLE t_result;
END
$BODY$
    LANGUAGE plpgsql;
