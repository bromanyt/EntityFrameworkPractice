CREATE OR REPLACE FUNCTION fn_increasing_frequency_of_visits(
    p_first_date DATE, p_last_date DATE,
    p_added_number_transactions BIGINT,
    p_max_index NUMERIC,
    p_max_share_transactions NUMERIC,
    p_margin_share NUMERIC
)
    RETURNS TABLE
            (
                Customer_ID                 BIGINT,
                Start_Date                  DATE,
                End_Date                    DATE,
                Required_Transactions_Count NUMERIC,
                Group_Name                  VARCHAR,
                Offer_Discount_Depth        NUMERIC
            )
AS
$BODY$
BEGIN
RETURN QUERY(WITH choose_group AS
                           (SELECT "Customer_id",
                                   "Group_id",
                                   "Group_Affinity_Index",
                                   sku_group.group_name,
                                   CEILING((FLOOR("Group_Minimum_Discount" * 100)) / 5) * 5                                       AS min_discount,
                                   DENSE_RANK()
                                   OVER (PARTITION BY view_groups."Customer_id" ORDER BY view_groups."Group_Affinity_Index" DESC) AS rank,
                                   "Group_Margin" * p_margin_share                                                                AS limit_discount
                            FROM view_groups
                                     JOIN sku_group ON view_groups."Group_id" = sku_group."group_id"
                            WHERE "Group_Churn_Rate" <= p_max_index
                              AND "Group_Discount_Share" * 100 < p_max_share_transactions), group_with_index AS
                           (SELECT "Customer_id",
                                   sku_group."group_name" AS "Group_Name",
                                   min_discount           AS "Offer_Discount_Depth",
                                   "Group_Affinity_Index"
                            FROM choose_group
                                     JOIN sku_group ON choose_group."Group_id" = sku_group."group_id"
                            WHERE choose_group.min_discount <= choose_group.limit_discount
                              AND choose_group.min_discount != 0),
                                                                                            group_with_max_index AS
                           (SELECT "Customer_id",
                                   "Group_Name",
                                   "Offer_Discount_Depth",
                                   "Group_Affinity_Index",
                                   MIN("Group_Affinity_Index") OVER (PARTITION BY "Customer_id") AS max_index
                            FROM group_with_index)

                  SELECT view_customers."Customer_id",
                         p_first_date AS "Start_Date",
                         p_last_date AS "End_Date",
                         ROUND((p_last_date - p_first_date) / "Customer_Frequency") +
                         p_added_number_transactions AS "Required_Transactions_Count",
                         "Group_Name",
                         "Offer_Discount_Depth" FROM view_customers
                           JOIN group_with_max_index
                                ON view_customers."Customer_id" = group_with_max_index."Customer_id"
                  WHERE max_index = "Group_Affinity_Index");
END;
$BODY$
LANGUAGE plpgsql;
