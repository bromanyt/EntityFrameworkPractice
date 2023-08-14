CREATE OR REPLACE FUNCTION fn_cross_selling(
    p_number_of_groups BIGINT,
    p_maximum_churn_index NUMERIC,
    p_maximum_stability_index NUMERIC,
    p_maximum_SKU_share NUMERIC,
    p_margin_share NUMERIC
)
    RETURNS TABLE
            (
                "Customer_ID"          BIGINT,
                "SKU_Name"             VARCHAR,
                "Offer_Discount_Depth" NUMERIC
            )
AS
$BODY$
BEGIN
RETURN QUERY(WITH collect_group AS
        (SELECT view_groups."Customer_id",
                view_groups."Group_id",
                "Group_Minimum_Discount",
                DENSE_RANK()
                OVER (PARTITION BY view_groups."Customer_id" ORDER BY view_groups."Group_Affinity_Index" DESC) AS rank
         FROM view_groups
         WHERE "Group_Churn_Rate" <= p_maximum_churn_index
           AND "Group_Stability_Index" < p_maximum_stability_index)

                     , choose_group AS
            (SELECT "Customer_id",
                    "Group_id",
                    "Group_Minimum_Discount"
             FROM collect_group
             WHERE rank <= p_number_of_groups)
    , stores_with_dif AS
            (SELECT view_customers."Customer_id",
                    "Group_id",
                    Transaction_Store_ID,
                    SKU_ID,
                    (SKU_Retail_Price - SKU_Purchase_Price) / SKU_Retail_Price AS rate,
                    SKU_Retail_Price - SKU_Purchase_Price                      AS diff,
                    "Group_Minimum_Discount"
             FROM stores
                      JOIN
                  view_customers ON "Customer_Primary_Store" = transaction_store_id
                      JOIN
                  choose_group ON choose_group."Customer_id" = view_customers."Customer_id")
    , sku_with_max_marge AS
            (SELECT "Customer_id",
                    "Group_id",
                    Transaction_Store_ID,
                    SKU_ID,
                    rate,
                    MAX(diff) AS max_diff,
                    "Group_Minimum_Discount"
             FROM stores_with_dif
             GROUP BY "Customer_id", "Group_id", Transaction_Store_ID, SKU_ID, "Group_Minimum_Discount", rate)
    , sku_in_group AS
            (SELECT "transaction_id",
                    checks.SKU_ID,
                    COUNT(*) OVER (PARTITION BY pg.group_id) AS number_trans_in_group,
                    group_id
             FROM checks
                      JOIN product_grid pg ON checks.SKU_ID = pg.SKU_ID)
    , trans_with_sku AS
            (SELECT "transaction_id",
                    sku_id,
                    COUNT(*) OVER (PARTITION BY sku_id) AS number_trans_with_sku,
                    number_trans_in_group,
                    group_id
             FROM sku_in_group
             WHERE sku_id IN (SELECT sku_id FROM sku_with_max_marge))
    , admitted_sku AS
            (SELECT DISTINCT "Customer_id",
                             trans_with_sku.sku_id,
                             "Group_Minimum_Discount",
                             product_grid.group_id,
                             max_diff,
                             (number_trans_with_sku / number_trans_in_group::NUMERIC) * 100 AS SKU_share,
                             product_grid.sku_name,
                             rate
             FROM trans_with_sku
                      JOIN
                  sku_with_max_marge ON trans_with_sku.sku_id = sku_with_max_marge.sku_id
                      JOIN
                  product_grid ON product_grid.sku_id = trans_with_sku.sku_id
             WHERE (number_trans_with_sku / number_trans_in_group::NUMERIC) * 100 <= p_maximum_SKU_share)
    , appropriate_for_marge AS
            (SELECT view_customers."Customer_id", "Group_id", min("Group_Minimum_Discount")
             FROM stores
                      JOIN
                  view_customers ON "Customer_Primary_Store" = transaction_store_id
                      JOIN
                  choose_group ON choose_group."Customer_id" = view_customers."Customer_id"
             GROUP BY view_customers."Customer_id", "Group_id")
    , combine_sku_group AS
            (SELECT admitted_sku.*
             FROM appropriate_for_marge
                      JOIN admitted_sku ON appropriate_for_marge."Group_id" = admitted_sku.group_id)
    , discount AS
            (SELECT "Customer_id",
                    sku_id,
                    "Group_Minimum_Discount",
                    sku_name,
                    group_id,
                    p_margin_share * rate                                    AS compare_field
                     ,
                    CEILING((FLOOR("Group_Minimum_Discount" * 100) / 5)) * 5 AS min_discount
             FROM combine_sku_group)
    , min_discount_in_group AS
            (SELECT "Customer_id",
                    group_id,
                    min(CEILING((FLOOR("Group_Minimum_Discount" * 100) / 5)) * 5) AS min_discount_for_group,
                    compare_field,
                    sku_name
             FROM discount
             GROUP BY "Customer_id", group_id, compare_field, sku_name)
    , min_discount_in_scu AS
            (SELECT "Group_id", (CEILING((FLOOR(min("Group_Minimum_Discount") * 100) / 5)) * 5) AS disc2
             FROM view_groups
             GROUP BY "Group_id")

                  SELECT DISTINCT "Customer_id",
                       sku_name AS "SKU_Name",
                       min_discount_for_group AS "Offer_Discount_Depth"
                  FROM min_discount_in_group
                           JOIN
                       min_discount_in_scu ON min_discount_in_group.group_id = min_discount_in_scu."Group_id"
                  WHERE compare_field > disc2
                    AND min_discount_for_group > 0
                  ORDER BY 1);
END;
$BODY$
LANGUAGE plpgsql;
