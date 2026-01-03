-- Grant DATA tables DML privileges to API (required for DEFINER's procedures)
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1

GRANT SELECT, INSERT, UPDATE, DELETE ON patients   TO api;
GRANT SELECT, INSERT, UPDATE, DELETE ON encounters TO api;
GRANT SELECT, INSERT, UPDATE, DELETE ON visits     TO api;

-- Additional objects
GRANT SELECT, INSERT, UPDATE, DELETE ON appointments     TO api;
GRANT SELECT, INSERT, UPDATE, DELETE ON medications      TO api;
GRANT SELECT, INSERT, UPDATE, DELETE ON inventory        TO api;
GRANT SELECT, INSERT, UPDATE, DELETE ON purchase_orders  TO api;
GRANT SELECT ON audit_log TO api;


