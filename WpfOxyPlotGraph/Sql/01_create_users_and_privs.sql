-- Create users and grant only necessary direct privileges (no roles for definer's rights)
-- Execute as SYS/DBA. Adjust passwords and PDB/service as appropriate.
-- Note: In Oracle, schema == user.

-- [1] Create users
CREATE USER data IDENTIFIED BY "Data#Pwd1";
CREATE USER api  IDENTIFIED BY "Api#Pwd1";
CREATE USER app  IDENTIFIED BY "App#Pwd1";

-- [2] Minimal privileges
GRANT CREATE SESSION TO data, api, app;
GRANT CREATE TABLE TO data;
GRANT CREATE SEQUENCE TO data;
GRANT CREATE TRIGGER TO data;
GRANT CREATE PROCEDURE TO api;

-- [3] Allow default tablespace quota if needed (adjust for your environment)
ALTER USER data QUOTA UNLIMITED ON USERS;
ALTER USER api  QUOTA UNLIMITED ON USERS;

-- [4] Grant API user direct DML privileges on DATA tables AFTER tables exist
-- (This is deferred; see 03/04 scripts)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON data.<table> TO api;


