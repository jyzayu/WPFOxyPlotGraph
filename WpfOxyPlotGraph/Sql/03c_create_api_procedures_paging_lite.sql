-- Lightweight paging for patients list: only id, name (reduces decryption work)
-- Connect as:  CONNECT api/"Api#Pwd1"@XEPDB1

CREATE OR REPLACE PROCEDURE patient_get_page_min
(
  p_offset    IN NUMBER,
  p_page_size IN NUMBER,
  p_cursor    OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name
      FROM data.patients
     ORDER BY id ASC
     OFFSET NVL(p_offset, 0) ROWS FETCH NEXT NVL(p_page_size, 50) ROWS ONLY;
END;
/

CREATE OR REPLACE PROCEDURE patient_search_page_min
(
  p_name_prefix IN VARCHAR2,
  p_offset      IN NUMBER,
  p_page_size   IN NUMBER,
  p_cursor      OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name
      FROM data.patients
     WHERE (:p_name_prefix IS NULL OR :p_name_prefix = '')
        OR (LOWER(name) LIKE LOWER(:p_name_prefix) || '%')
     ORDER BY id ASC
     OFFSET NVL(p_offset, 0) ROWS FETCH NEXT NVL(p_page_size, 50) ROWS ONLY;
END;
/


