-- Paging variants to avoid huge resultsets that can freeze the UI
-- Connect as:  CONNECT api/"Api#Pwd1"@XEPDB1
-- Note: Qualify tables with DATA schema

CREATE OR REPLACE PROCEDURE patient_get_page
(
  p_offset    IN NUMBER,
  p_page_size IN NUMBER,
  p_cursor    OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name, rrn, address, contact
      FROM data.patients
     ORDER BY id ASC
     OFFSET NVL(p_offset, 0) ROWS FETCH NEXT NVL(p_page_size, 50) ROWS ONLY;
END;
/

CREATE OR REPLACE PROCEDURE encounter_get_by_patient_page
(
  p_patient_id IN NUMBER,
  p_offset     IN NUMBER,
  p_page_size  IN NUMBER,
  p_cursor     OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT e.id, e.patient_id, e.visit_at, e.diagnosis, e.notes
      FROM data.encounters e
     WHERE e.patient_id = p_patient_id
     ORDER BY e.visit_at DESC, e.id DESC
     OFFSET NVL(p_offset, 0) ROWS FETCH NEXT NVL(p_page_size, 50) ROWS ONLY;
END;
/

CREATE OR REPLACE PROCEDURE visit_get_by_patient_page
(
  p_patient_id IN NUMBER,
  p_offset     IN NUMBER,
  p_page_size  IN NUMBER,
  p_cursor     OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, patient_id, visit_date, doctor_name, diagnosis, treatment, notes, created_at, updated_at
      FROM data.visits
     WHERE patient_id = p_patient_id
     ORDER BY visit_date DESC, id DESC
     OFFSET NVL(p_offset, 0) ROWS FETCH NEXT NVL(p_page_size, 50) ROWS ONLY;
END;
/


