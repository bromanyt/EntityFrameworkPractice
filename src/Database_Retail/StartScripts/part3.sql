CREATE ROLE administrator WITH LOGIN PASSWORD '7777';
CREATE ROLE visitor WITH LOGIN PASSWORD '1111';
GRANT pg_read_all_data, pg_write_all_data, pg_signal_backend, pg_read_server_files, pg_write_server_files
    TO administrator;
GRANT pg_read_all_data TO visitor;
GRANT ALL ON SCHEMA public TO visitor;
GRANT ALL ON SCHEMA public TO administrator;
