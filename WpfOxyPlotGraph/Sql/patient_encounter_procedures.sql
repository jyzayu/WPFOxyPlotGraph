-- PATIENT procedures

CREATE OR REPLACE PROCEDURE patient_insert(
	p_name IN VARCHAR2,
	p_rrn IN VARCHAR2,
	p_rrn_hash IN VARCHAR2,
	p_address IN VARCHAR2,
	p_contact IN VARCHAR2,
	p_id_out OUT NUMBER
) AS
BEGIN
	INSERT INTO patients (name, rrn, rrn_hash, address, contact)
	VALUES (p_name, p_rrn, p_rrn_hash, p_address, p_contact)
	RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE patient_get_all(
	p_cursor OUT SYS_REFCURSOR
) AS
BEGIN
	OPEN p_cursor FOR
		SELECT id, name, rrn, address, contact
		  FROM patients
		 ORDER BY id ASC;
END;
/
GRANT EXECUTE ON system.patient_get_all TO admin;
CREATE OR REPLACE PROCEDURE patient_update(
	p_id IN NUMBER,
	p_name IN VARCHAR2,
	p_rrn IN VARCHAR2,
	p_rrn_hash IN VARCHAR2,
	p_address IN VARCHAR2,
	p_contact IN VARCHAR2
) AS
BEGIN
	UPDATE patients
	   SET name = p_name,
		   rrn = p_rrn,
		   rrn_hash = p_rrn_hash,
		   address = p_address,
		   contact = p_contact,
		   updated_at = CURRENT_TIMESTAMP
	 WHERE id = p_id;
END;
/


-- ENCOUNTER procedures
-- Assumes table ENCOUNTERS(patient_id FK -> PATIENTS.id) exists
-- Columns used: id, patient_id, visit_at, diagnosis, notes, updated_at

CREATE OR REPLACE PROCEDURE encounter_insert(
	p_patient_id IN NUMBER,
	p_visit_at IN TIMESTAMP,
	p_diagnosis IN VARCHAR2,
	p_notes IN VARCHAR2,
	p_id_out OUT NUMBER
) AS
BEGIN
	INSERT INTO encounters (patient_id, visit_at, diagnosis, notes)
	VALUES (p_patient_id, p_visit_at, p_diagnosis, p_notes)
	RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE encounter_update(
	p_id IN NUMBER,
	p_patient_id IN NUMBER,
	p_visit_at IN TIMESTAMP,
	p_diagnosis IN VARCHAR2,
	p_notes IN VARCHAR2
) AS
BEGIN
	UPDATE encounters
	   SET patient_id = p_patient_id,
		   visit_at = p_visit_at,
		   diagnosis = p_diagnosis,
		   notes = p_notes,
		   updated_at = CURRENT_TIMESTAMP
	 WHERE id = p_id;
END;
/

CREATE OR REPLACE PROCEDURE encounter_get_by_patient(
	p_patient_id IN NUMBER,
	p_cursor OUT SYS_REFCURSOR
) AS
BEGIN
	OPEN p_cursor FOR
		SELECT e.id, e.patient_id, e.visit_at, e.diagnosis, e.notes
		  FROM encounters e
		 WHERE e.patient_id = p_patient_id
		 ORDER BY e.visit_at DESC, e.id DESC;
END;
/

CREATE OR REPLACE PROCEDURE encounter_get_with_patient_by_rrn_hash(
	p_rrn_hash IN VARCHAR2,
	p_cursor OUT SYS_REFCURSOR
) AS
BEGIN
	OPEN p_cursor FOR
		SELECT
			e.id AS encounter_id,
			e.patient_id,
			e.visit_at,
			e.diagnosis,
			e.notes,
			p.id AS patient_id_resolved,
			p.name,
			p.rrn,
			p.address,
			p.contact
		  FROM encounters e
		  JOIN patients p
			ON p.id = e.patient_id
		 WHERE p.rrn_hash = p_rrn_hash
		 ORDER BY e.visit_at DESC, e.id DESC;
END;
/

-- VISIT procedures (for 'visits' table)

CREATE OR REPLACE PROCEDURE visit_insert(
    p_patient_id IN NUMBER,
    p_visit_date IN TIMESTAMP,
    p_doctor_name IN VARCHAR2,
    p_diagnosis IN CLOB,
    p_treatment IN CLOB,
    p_notes IN CLOB,
    p_id_out OUT NUMBER
) AS
BEGIN
    INSERT INTO visits (patient_id, visit_date, doctor_name, diagnosis, treatment, notes)
    VALUES (p_patient_id, p_visit_date, p_doctor_name, p_diagnosis, p_treatment, p_notes)
    RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE visit_update(
    p_id IN NUMBER,
    p_patient_id IN NUMBER,
    p_visit_date IN TIMESTAMP,
    p_doctor_name IN VARCHAR2,
    p_diagnosis IN CLOB,
    p_treatment IN CLOB,
    p_notes IN CLOB
) AS
BEGIN
    UPDATE visits
       SET patient_id = p_patient_id,
           visit_date = p_visit_date,
           doctor_name = p_doctor_name,
           diagnosis = p_diagnosis,
           treatment = p_treatment,
           notes = p_notes,
           updated_at = CURRENT_TIMESTAMP
     WHERE id = p_id;
END;
/

CREATE OR REPLACE PROCEDURE visit_get_by_patient(
    p_patient_id IN NUMBER,
    p_cursor OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_cursor FOR
        SELECT id, patient_id, visit_date, doctor_name, diagnosis, treatment, notes, created_at, updated_at
          FROM visits
         WHERE patient_id = p_patient_id
         ORDER BY visit_date DESC, id DESC;
END;
/


