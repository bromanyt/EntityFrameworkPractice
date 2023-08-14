-- Представление pg_roles открывает доступ к информации о ролях в базах данных.
SELECT *
FROM pg_roles;

CREATE ROLE administrator WITH LOGIN PASSWORD '7777';
CREATE ROLE visitor WITH LOGIN PASSWORD '1111';
GRANT pg_read_all_data, pg_write_all_data, pg_signal_backend, pg_read_server_files, pg_write_server_files
    TO administrator;
GRANT pg_read_all_data TO visitor;
GRANT ALL ON SCHEMA public TO visitor;
GRANT ALL ON SCHEMA public TO administrator;

-- Представление pg_user открывает доступ к информации о пользователях базы данных.
SELECT *
FROM pg_catalog.pg_user;
SELECT *
FROM pg_roles;
-- DROP ROLE administrator, visitor;

-- Test
/*
psql -d retail
SELECT current_user;
SET ROLE visitor;
SELECT current_user;
SELECT COUNT(*) FROM view_periods;
INSERT INTO personal_information VALUES (100000, 'Alex', 'Romanov', 'imperator@mail.ru', '+79990001907');
DELETE FROM personal_information WHERE customer_id = 1;

SELECT current_user;
SET ROLE administrator;
SELECT current_user;
SELECT COUNT(*) FROM view_periods;
INSERT INTO personal_information VALUES (100000, 'Alex', 'Romanov', 'imperator@mail.ru', '+79990001907');
SELECT * FROM personal_information WHERE customer_id = 100000;
DELETE FROM personal_information WHERE customer_id = 100000;
SELECT * FROM personal_information WHERE customer_id = 100000;
*/