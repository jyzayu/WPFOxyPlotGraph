-- Legacy-compatible DDL without IDENTITY (use sequences + triggers)
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1 (or your service)
-- Use this script if 02_create_tables_in_data.sql fails with ORA-00922 on IDENTITY.

-- PATIENTS
CREATE TABLE patients (
  id NUMBER PRIMARY KEY,
  name VARCHAR2(1024) NOT NULL,
  rrn VARCHAR2(1024) NOT NULL,
  rrn_hash VARCHAR2(64) NOT NULL,
  address VARCHAR2(2048) NOT NULL,
  contact VARCHAR2(512) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT uq_patients_rrn_hash UNIQUE (rrn_hash)
);

CREATE SEQUENCE seq_patients START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

CREATE OR REPLACE TRIGGER trg_patients_bi
BEFORE INSERT ON patients
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_patients.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/

-- ENCOUNTERS
CREATE TABLE encounters (
  id NUMBER PRIMARY KEY,
  patient_id NUMBER NOT NULL,
  visit_at TIMESTAMP NOT NULL,
  diagnosis VARCHAR2(4000),
  notes VARCHAR2(4000),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT fk_encounters_patient FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE
);

CREATE SEQUENCE seq_encounters START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

CREATE OR REPLACE TRIGGER trg_encounters_bi
BEFORE INSERT ON encounters
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_encounters.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/

-- VISITS
CREATE TABLE visits (
  id NUMBER PRIMARY KEY,
  patient_id NUMBER NOT NULL,
  visit_date TIMESTAMP NOT NULL,
  doctor_name VARCHAR2(100) NOT NULL,
  diagnosis CLOB NOT NULL,
  treatment CLOB NOT NULL,
  notes CLOB NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT fk_visits_patient FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE
);

CREATE SEQUENCE seq_visits START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;

CREATE OR REPLACE TRIGGER trg_visits_bi
BEFORE INSERT ON visits
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_visits.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/

-- Index for query performance
CREATE INDEX idx_visits_patient_id ON visits(patient_id);


-- Legacy-compatible DDL for additional DATA tables (no IDENTITY; uses sequences + triggers)
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1

-- APPOINTMENTS
CREATE TABLE appointments (
  id NUMBER PRIMARY KEY,
  patient_id NUMBER NOT NULL,
  appointment_date TIMESTAMP NOT NULL,
  doctor_name VARCHAR2(100) NOT NULL,
  reason VARCHAR2(255) NOT NULL,
  status VARCHAR2(20) DEFAULT 'Scheduled' NOT NULL,
  notes CLOB NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT ck_appointments_status CHECK (status IN ('Scheduled','Completed','Cancelled')),
  CONSTRAINT fk_appointments_patient FOREIGN KEY (patient_id) REFERENCES patients(id) ON DELETE CASCADE
);
CREATE SEQUENCE seq_appointments START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE OR REPLACE TRIGGER trg_appointments_bi
BEFORE INSERT ON appointments
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_appointments.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/
CREATE INDEX idx_appointments_patient_id ON appointments(patient_id);
CREATE INDEX idx_appointments_status ON appointments(status);

-- MEDICATIONS
CREATE TABLE medications (
  id NUMBER PRIMARY KEY,
  name VARCHAR2(200) NOT NULL,
  sku VARCHAR2(100) NOT NULL,
  min_stock NUMBER DEFAULT 0 NOT NULL,
  reorder_point NUMBER DEFAULT 0 NOT NULL,
  reorder_qty NUMBER DEFAULT 0 NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT uq_medications_sku UNIQUE (sku)
);
CREATE SEQUENCE seq_medications START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE OR REPLACE TRIGGER trg_medications_bi
BEFORE INSERT ON medications
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_medications.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/

-- INVENTORY
CREATE TABLE inventory (
  medication_id NUMBER PRIMARY KEY,
  quantity NUMBER DEFAULT 0 NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT fk_inventory_medication FOREIGN KEY (medication_id) REFERENCES medications(id) ON DELETE CASCADE
);

-- PURCHASE ORDERS
CREATE TABLE purchase_orders (
  id NUMBER PRIMARY KEY,
  medication_id NUMBER NOT NULL,
  quantity NUMBER NOT NULL,
  status VARCHAR2(20) DEFAULT 'Pending' NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
  CONSTRAINT ck_po_status CHECK (status IN ('Pending','Ordered','Received','Cancelled')),
  CONSTRAINT fk_po_medication FOREIGN KEY (medication_id) REFERENCES medications(id) ON DELETE CASCADE
);
CREATE SEQUENCE seq_purchase_orders START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE OR REPLACE TRIGGER trg_purchase_orders_bi
BEFORE INSERT ON purchase_orders
FOR EACH ROW
BEGIN
  IF :NEW.id IS NULL THEN
    SELECT seq_purchase_orders.NEXTVAL INTO :NEW.id FROM dual;
  END IF;
END;
/
CREATE INDEX idx_po_medication_id ON purchase_orders(medication_id);
CREATE INDEX idx_po_status ON purchase_orders(status);


