CREATE TABLE date_of_analysis_formation
(
    Analysis_Formation TIMESTAMP NOT NULL PRIMARY KEY
);

CREATE TABLE sku_group
(
    Group_ID   BIGINT NOT NULL PRIMARY KEY,
    Group_Name VARCHAR NOT NULL
);

CREATE TABLE product_grid
(
    SKU_ID   BIGINT NOT NULL PRIMARY KEY,
    SKU_Name VARCHAR NOT NULL,
    Group_ID BIGINT NOT NULL,
    CONSTRAINT fk_product_grid_Group_ID FOREIGN KEY (Group_ID) REFERENCES sku_group (Group_ID),
    CONSTRAINT ch_product_grid_SKU_Name CHECK (SKU_Name SIMILAR TO '[A-ZА-Яa-zа-я\- 0-9]*')
);

CREATE TABLE stores
(
    Transaction_Store_ID BIGINT NOT NULL,
    SKU_ID               BIGINT NOT NULL,
    SKU_Purchase_Price   NUMERIC NOT NULL,
    SKU_Retail_Price     NUMERIC NOT NULL,
    PRIMARY KEY (Transaction_Store_ID, SKU_ID),
    CONSTRAINT fk_stores_SKU_ID FOREIGN KEY (SKU_ID) REFERENCES product_grid (SKU_ID)
);

CREATE TABLE personal_information
(
    Customer_ID            BIGINT  NOT NULL PRIMARY KEY,
    Customer_Name          VARCHAR NOT NULL,
    Customer_Surname       VARCHAR NOT NULL,
    Customer_Primary_Email VARCHAR,
    Customer_Primary_Phone VARCHAR(12),
    CONSTRAINT ch_personal_information_Customer_Primary_Email CHECK (Customer_Primary_Email SIMILAR TO '_+@{1}[a-z0-9]+.{1}[a-z]+') ,
    CONSTRAINT ch_personal_information_Customer_Primary_Phone CHECK (Customer_Primary_Phone SIMILAR TO '[+]{1}7[0-9]{10}'),
    CONSTRAINT ch_personal_information_Customer_Name CHECK (Customer_Name SIMILAR TO '[A-ZА-Я]{1}[a-zа-я\- ]*')
);

CREATE TABLE cards
(
    Customer_Card_ID BIGINT NOT NULL PRIMARY KEY,
    Customer_ID      BIGINT NOT NULL,
    CONSTRAINT fk_cards_Customer_ID FOREIGN KEY (Customer_ID) REFERENCES personal_information (Customer_ID)
);

CREATE OR REPLACE FUNCTION ch_stores_have_store_id(store_id BIGINT) RETURNS BOOL AS
$$
BEGIN
    IF( SELECT transaction_store_id
        FROM stores
        WHERE store_id = transaction_store_id LIMIT 1)
        IS NOT NULL THEN RETURN TRUE;
ELSE RETURN FALSE;
END IF;
END;
$$
LANGUAGE plpgsql;

CREATE TABLE transactions
(
    Transaction_ID       BIGINT UNIQUE NOT NULL PRIMARY KEY,
    Customer_Card_ID     BIGINT        NOT NULL,
    Transaction_Summ     NUMERIC NOT NULL,
    Transaction_DateTime TIMESTAMP NOT NULL,
    Transaction_Store_ID BIGINT        NOT NULL,
    CONSTRAINT fk_transactions_Customer_Card_ID FOREIGN KEY (Customer_Card_ID) REFERENCES cards (Customer_Card_ID),
    CONSTRAINT fk_transactions_Transaction_Store_ID CHECK (ch_stores_have_store_id(transaction_store_id))
);

CREATE TABLE checks
(
    Transaction_ID BIGINT NOT NULL,
    SKU_ID         BIGINT NOT NULL,
    SKU_Amount     NUMERIC NOT NULL,
    SKU_Summ       NUMERIC NOT NULL,
    SKU_Summ_Paid  NUMERIC NOT NULL,
    SKU_Discount   NUMERIC NOT NULL,
    PRIMARY KEY (Transaction_ID, SKU_ID),
    CONSTRAINT fk_transaction_id_SKU_ID FOREIGN KEY (Transaction_ID) REFERENCES transactions (Transaction_ID),
    CONSTRAINT fk_checks_SKU_ID FOREIGN KEY (SKU_ID) REFERENCES product_grid (SKU_ID)
);


CREATE OR REPLACE PROCEDURE csv_import(table_name TEXT, file_name TEXT, delimiter TEXT) AS
$BODY$
BEGIN
EXECUTE FORMAT('CREATE TEMPORARY TABLE IF NOT EXISTS  temp (LIKE %I INCLUDING DEFAULTS);', table_name);
EXECUTE FORMAT('COPY temp FROM %L (FORMAT CSV, DELIMITER %L)', file_name, delimiter);
EXECUTE FORMAT('INSERT INTO %I SELECT * FROM temp ON CONFLICT DO NOTHING', table_name);
DROP TABLE IF EXISTS temp;
END;
$BODY$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE import_mini_dataset(path VARCHAR) AS
$BODY$
BEGIN
SET
DATESTYLE = DMY;
EXECUTE FORMAT('call csv_import(''personal_information'', %L, E''\t'');', CONCAT(path, 'Personal_Data_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''cards'', %L, E''\t'');', CONCAT(path, 'Cards_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''sku_group'', %L, E''\t'');', CONCAT(path, 'Groups_SKU_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''product_grid'', %L, E''\t'');', CONCAT(path, 'SKU_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''stores'', %L, E''\t'');', CONCAT(path, 'Stores_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''transactions'', %L, E''\t'');', CONCAT(path, 'Transactions_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''checks'', %L, E''\t'');', CONCAT(path, 'Checks_Mini.tsv'));
EXECUTE FORMAT('call csv_import(''date_of_analysis_formation'', %L, E''\t'');',
               CONCAT(path, 'Date_Of_Analysis_Formation.tsv'));
END;
$BODY$
LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE import_data (in_file_name varchar, in_table_name varchar, in_delim varchar)
AS $import$
    BEGIN
        IF in_file_name LIKE '%.csv' THEN
        EXECUTE('COPY ' || in_table_name || ' FROM ''' || in_file_name || ''' DELIMITER '''
                           || in_delim || ''' ' );
        ELSE
             EXECUTE('COPY ' || in_table_name || ' FROM ''' || in_file_name || '''  ' );
        END IF;
    END;
$import$ LANGUAGE plpgsql;

CALL import_mini_dataset('/docker-entrypoint-initdb.d/');


