-- Detail fetch by id for patients
-- Connect as:  CONNECT api/"Api#Pwd1"@XEPDB1

CREATE OR REPLACE PROCEDURE patient_get_by_id
(
  p_id     IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name, rrn, address, contact
      FROM data.patients
     WHERE id = p_id;
END;
/


