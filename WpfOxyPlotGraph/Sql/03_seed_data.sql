-- Seed large volumes of test data for performance/tuning
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1
-- This script truncates target tables, then bulk-inserts randomized rows.
-- Tune the counts below to scale up/down.
-- Safe to re-run; uses TRUNCATE (child tables first) to reset data.

DECLARE
  v_patients                NUMBER := 100000; -- total patients
  v_visits_per_patient      NUMBER := 5;    -- visits per patient
  v_encounters_per_patient  NUMBER := 5;     -- encounters per patient
  v_appts_per_patient       NUMBER := 3;     -- appointments per patient
  v_medications             NUMBER := 500;   -- distinct medications
  v_po_per_med              NUMBER := 5;     -- purchase orders per medication
BEGIN
  -- Truncate in FK-safe order
  EXECUTE IMMEDIATE 'TRUNCATE TABLE purchase_orders';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE inventory';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE visits';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE encounters';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE appointments';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE medications';
  EXECUTE IMMEDIATE 'TRUNCATE TABLE patients';
END;
/

-- Patients
INSERT INTO patients (name, rrn, rrn_hash, address, contact)
SELECT
  'Patient ' || LEVEL,
  DBMS_RANDOM.STRING('X', 20), -- arbitrary token for rrn seed
  SUBSTR(RAWTOHEX(SYS_GUID()) || RAWTOHEX(SYS_GUID()), 1, 64), -- unique-ish 64 hex chars
  'Addr ' || DBMS_RANDOM.STRING('U', 40),
  '010-' || TO_CHAR(TRUNC(DBMS_RANDOM.VALUE(1000,9999))) || '-' || TO_CHAR(TRUNC(DBMS_RANDOM.VALUE(1000,9999)))
FROM dual
CONNECT BY LEVEL <=  (SELECT /* scalar subquery to bind pl/sql value */ 100000 FROM dual);
-- Note: the scalar subquery above mirrors v_patients. Adjust both if you change counts manually.

-- If you change v_patients, update the scalar literal above or replace with your desired count.

-- Visits (CLOB-heavy)
INSERT /*+ APPEND */ INTO visits (patient_id, visit_date, doctor_name, diagnosis, treatment, notes)
SELECT
  p.id,
  SYSTIMESTAMP - NUMTODSINTERVAL(TRUNC(DBMS_RANDOM.VALUE(0, 365*3)) * 24 * 3600, 'SECOND'),
  'Dr. ' || CHR(65 + MOD(ABS(DBMS_RANDOM.RANDOM), 26)),
  TO_CLOB('Dx ') || DBMS_RANDOM.STRING('U', 1500),
  TO_CLOB('Tx ') || DBMS_RANDOM.STRING('U', 1500),
  CASE WHEN DBMS_RANDOM.VALUE < 0.35 THEN TO_CLOB(DBMS_RANDOM.STRING('U', 200)) ELSE NULL END
FROM patients p
CROSS JOIN (SELECT LEVEL AS n FROM dual CONNECT BY LEVEL <= 5);

-- Encounters
INSERT /*+ APPEND */ INTO encounters (patient_id, visit_at, diagnosis, notes)
SELECT
  p.id,
  SYSTIMESTAMP - NUMTODSINTERVAL(TRUNC(DBMS_RANDOM.VALUE(0, 365*3)) * 24 * 3600, 'SECOND'),
  DBMS_RANDOM.STRING('U', 1000),
  DBMS_RANDOM.STRING('U', 500)
FROM patients p
CROSS JOIN (SELECT LEVEL AS n FROM dual CONNECT BY LEVEL <= 5);

-- Appointments
INSERT /*+ APPEND */ INTO appointments (patient_id, appointment_date, doctor_name, reason, status, notes)
SELECT
  p.id,
  SYSTIMESTAMP + NUMTODSINTERVAL(TRUNC(DBMS_RANDOM.VALUE(-90, 90)) * 24 * 3600, 'SECOND'),
  'Dr. ' || CHR(65 + MOD(ABS(DBMS_RANDOM.RANDOM), 26)),
  'Reason ' || DBMS_RANDOM.STRING('U', 20),
  CASE MOD(ABS(DBMS_RANDOM.RANDOM), 3)
       WHEN 0 THEN 'Scheduled'
       WHEN 1 THEN 'Completed'
       ELSE 'Cancelled'
  END,
  CASE WHEN DBMS_RANDOM.VALUE < 0.25 THEN TO_CLOB(DBMS_RANDOM.STRING('U', 200)) ELSE NULL END
FROM patients p
CROSS JOIN (SELECT LEVEL AS n FROM dual CONNECT BY LEVEL <= 3);

-- Medications
INSERT INTO medications (name, sku, min_stock, reorder_point, reorder_qty)
SELECT
  'Medication ' || LEVEL,
  'MED' || LPAD(TO_CHAR(LEVEL), 6, '0'),
  TRUNC(DBMS_RANDOM.VALUE(0, 50)),
  TRUNC(DBMS_RANDOM.VALUE(51, 150)),
  TRUNC(DBMS_RANDOM.VALUE(10, 300))
FROM dual
CONNECT BY LEVEL <= 500;

-- Inventory (1:1 with medications)
INSERT INTO inventory (medication_id, quantity, updated_at)
SELECT
  m.id,
  TRUNC(DBMS_RANDOM.VALUE(0, 2000)),
  SYSTIMESTAMP
FROM medications m;

-- Purchase Orders (multiple per medication)
INSERT /*+ APPEND */ INTO purchase_orders (medication_id, quantity, status)
SELECT
  m.id,
  TRUNC(DBMS_RANDOM.VALUE(10, 500)),
  CASE MOD(ABS(DBMS_RANDOM.RANDOM), 4)
       WHEN 0 THEN 'Pending'
       WHEN 1 THEN 'Ordered'
       WHEN 2 THEN 'Received'
       ELSE 'Cancelled'
  END
FROM medications m
CROSS JOIN (SELECT LEVEL AS n FROM dual CONNECT BY LEVEL <= 5);

COMMIT;

-- Tips:
-- 1) To scale up dramatically, increase the literals used in CONNECT BY levels above:
--    - patients: change 10000 -> 50000 / 100000+
--    - visits/encounters/appointments per patient: change 10 / 5 / 3 multipliers
--    - medications and purchase orders counts similarly
-- 2) Consider running DBMS_STATS to refresh optimizer stats after loading:
-- BEGIN
--   DBMS_STATS.GATHER_SCHEMA_STATS(ownname => 'DATA', estimate_percent => DBMS_STATS.AUTO_SAMPLE_SIZE);
-- END;
-- /


