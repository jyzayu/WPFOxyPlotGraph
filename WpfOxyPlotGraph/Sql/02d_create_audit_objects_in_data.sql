-- Create audit objects in DATA schema
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1

CREATE TABLE audit_log (
  audit_id   NUMBER PRIMARY KEY,
  table_name VARCHAR2(30) NOT NULL,
  operation  CHAR(1) CHECK (operation IN ('I','U','D')) NOT NULL,
  pk_id      NUMBER,
  changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  changed_by VARCHAR2(128) DEFAULT SYS_CONTEXT('USERENV','SESSION_USER') NOT NULL,
  old_values CLOB,
  new_values CLOB
);

CREATE SEQUENCE seq_audit_log START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

CREATE OR REPLACE TRIGGER trg_audit_log_bi
BEFORE INSERT ON audit_log
FOR EACH ROW
BEGIN
  IF :NEW.audit_id IS NULL THEN
    SELECT seq_audit_log.NEXTVAL INTO :NEW.audit_id FROM dual;
  END IF;
END;
/


