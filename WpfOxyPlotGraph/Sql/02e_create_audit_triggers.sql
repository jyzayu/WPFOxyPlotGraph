-- Create row-level audit triggers for DATA tables
-- Connect as:  CONNECT data/"Data#Pwd1"@XEPDB1

-- PATIENTS audit trigger
CREATE OR REPLACE TRIGGER trg_patients_audit
AFTER INSERT OR UPDATE OR DELETE ON patients
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'PATIENTS',
      'I',
      :NEW.id,
      'name=' || :NEW.name ||
      ';rrn_hash=' || :NEW.rrn_hash ||
      ';address=' || :NEW.address ||
      ';contact=' || :NEW.contact
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'PATIENTS',
      'U',
      :NEW.id,
      'name=' || :OLD.name ||
      ';rrn_hash=' || :OLD.rrn_hash ||
      ';address=' || :OLD.address ||
      ';contact=' || :OLD.contact,
      'name=' || :NEW.name ||
      ';rrn_hash=' || :NEW.rrn_hash ||
      ';address=' || :NEW.address ||
      ';contact=' || :NEW.contact
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'PATIENTS',
      'D',
      :OLD.id,
      'name=' || :OLD.name ||
      ';rrn_hash=' || :OLD.rrn_hash ||
      ';address=' || :OLD.address ||
      ';contact=' || :OLD.contact
    );
  END IF;
END;
/

-- ENCOUNTERS audit trigger
CREATE OR REPLACE TRIGGER trg_encounters_audit
AFTER INSERT OR UPDATE OR DELETE ON encounters
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'ENCOUNTERS',
      'I',
      :NEW.id,
      'patient_id=' || :NEW.patient_id ||
      ';visit_at=' || TO_CHAR(:NEW.visit_at, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';diagnosis=' || SUBSTR(:NEW.diagnosis, 1, 1000) ||
      ';notes=' || SUBSTR(:NEW.notes, 1, 1000)
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'ENCOUNTERS',
      'U',
      :NEW.id,
      'patient_id=' || :OLD.patient_id ||
      ';visit_at=' || TO_CHAR(:OLD.visit_at, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';diagnosis=' || SUBSTR(:OLD.diagnosis, 1, 1000) ||
      ';notes=' || SUBSTR(:OLD.notes, 1, 1000),
      'patient_id=' || :NEW.patient_id ||
      ';visit_at=' || TO_CHAR(:NEW.visit_at, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';diagnosis=' || SUBSTR(:NEW.diagnosis, 1, 1000) ||
      ';notes=' || SUBSTR(:NEW.notes, 1, 1000)
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'ENCOUNTERS',
      'D',
      :OLD.id,
      'patient_id=' || :OLD.patient_id ||
      ';visit_at=' || TO_CHAR(:OLD.visit_at, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';diagnosis=' || SUBSTR(:OLD.diagnosis, 1, 1000) ||
      ';notes=' || SUBSTR(:OLD.notes, 1, 1000)
    );
  END IF;
END;
/

-- VISITS audit trigger (exclude large CLOBs from audit payload)
CREATE OR REPLACE TRIGGER trg_visits_audit
AFTER INSERT OR UPDATE OR DELETE ON visits
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'VISITS',
      'I',
      :NEW.id,
      'patient_id=' || :NEW.patient_id ||
      ';visit_date=' || TO_CHAR(:NEW.visit_date, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';doctor_name=' || :NEW.doctor_name
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'VISITS',
      'U',
      :NEW.id,
      'patient_id=' || :OLD.patient_id ||
      ';visit_date=' || TO_CHAR(:OLD.visit_date, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';doctor_name=' || :OLD.doctor_name,
      'patient_id=' || :NEW.patient_id ||
      ';visit_date=' || TO_CHAR(:NEW.visit_date, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';doctor_name=' || :NEW.doctor_name
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'VISITS',
      'D',
      :OLD.id,
      'patient_id=' || :OLD.patient_id ||
      ';visit_date=' || TO_CHAR(:OLD.visit_date, 'YYYY-MM-DD"T"HH24:MI:SS') ||
      ';doctor_name=' || :OLD.doctor_name
    );
  END IF;
END;
/


-- APPOINTMENTS audit trigger
CREATE OR REPLACE TRIGGER trg_appointments_audit
AFTER INSERT OR UPDATE OR DELETE ON appointments
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'APPOINTMENTS',
      'I',
      :NEW.id,
      'patient_id=' || :NEW.patient_id ||
      ';appointment_date=' || TO_CHAR(:NEW.appointment_date, 'YYYY-MM-DD\"T\"HH24:MI:SS') ||
      ';doctor_name=' || :NEW.doctor_name ||
      ';reason=' || :NEW.reason ||
      ';status=' || :NEW.status
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'APPOINTMENTS',
      'U',
      :NEW.id,
      'patient_id=' || :OLD.patient_id ||
      ';appointment_date=' || TO_CHAR(:OLD.appointment_date, 'YYYY-MM-DD\"T\"HH24:MI:SS') ||
      ';doctor_name=' || :OLD.doctor_name ||
      ';reason=' || :OLD.reason ||
      ';status=' || :OLD.status,
      'patient_id=' || :NEW.patient_id ||
      ';appointment_date=' || TO_CHAR(:NEW.appointment_date, 'YYYY-MM-DD\"T\"HH24:MI:SS') ||
      ';doctor_name=' || :NEW.doctor_name ||
      ';reason=' || :NEW.reason ||
      ';status=' || :NEW.status
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'APPOINTMENTS',
      'D',
      :OLD.id,
      'patient_id=' || :OLD.patient_id ||
      ';appointment_date=' || TO_CHAR(:OLD.appointment_date, 'YYYY-MM-DD\"T\"HH24:MI:SS') ||
      ';doctor_name=' || :OLD.doctor_name ||
      ';reason=' || :OLD.reason ||
      ';status=' || :OLD.status
    );
  END IF;
END;
/

-- MEDICATIONS audit trigger
CREATE OR REPLACE TRIGGER trg_medications_audit
AFTER INSERT OR UPDATE OR DELETE ON medications
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'MEDICATIONS',
      'I',
      :NEW.id,
      'name=' || :NEW.name ||
      ';sku=' || :NEW.sku ||
      ';min_stock=' || :NEW.min_stock ||
      ';reorder_point=' || :NEW.reorder_point ||
      ';reorder_qty=' || :NEW.reorder_qty
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'MEDICATIONS',
      'U',
      :NEW.id,
      'name=' || :OLD.name ||
      ';sku=' || :OLD.sku ||
      ';min_stock=' || :OLD.min_stock ||
      ';reorder_point=' || :OLD.reorder_point ||
      ';reorder_qty=' || :OLD.reorder_qty,
      'name=' || :NEW.name ||
      ';sku=' || :NEW.sku ||
      ';min_stock=' || :NEW.min_stock ||
      ';reorder_point=' || :NEW.reorder_point ||
      ';reorder_qty=' || :NEW.reorder_qty
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'MEDICATIONS',
      'D',
      :OLD.id,
      'name=' || :OLD.name ||
      ';sku=' || :OLD.sku ||
      ';min_stock=' || :OLD.min_stock ||
      ';reorder_point=' || :OLD.reorder_point ||
      ';reorder_qty=' || :OLD.reorder_qty
    );
  END IF;
END;
/

-- INVENTORY audit trigger
CREATE OR REPLACE TRIGGER trg_inventory_audit
AFTER INSERT OR UPDATE OR DELETE ON inventory
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'INVENTORY',
      'I',
      :NEW.medication_id,
      'medication_id=' || :NEW.medication_id ||
      ';quantity=' || :NEW.quantity
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'INVENTORY',
      'U',
      :NEW.medication_id,
      'medication_id=' || :OLD.medication_id ||
      ';quantity=' || :OLD.quantity,
      'medication_id=' || :NEW.medication_id ||
      ';quantity=' || :NEW.quantity
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'INVENTORY',
      'D',
      :OLD.medication_id,
      'medication_id=' || :OLD.medication_id ||
      ';quantity=' || :OLD.quantity
    );
  END IF;
END;
/

-- PURCHASE_ORDERS audit trigger
CREATE OR REPLACE TRIGGER trg_purchase_orders_audit
AFTER INSERT OR UPDATE OR DELETE ON purchase_orders
FOR EACH ROW
BEGIN
  IF INSERTING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, new_values)
    VALUES (
      'PURCHASE_ORDERS',
      'I',
      :NEW.id,
      'medication_id=' || :NEW.medication_id ||
      ';quantity=' || :NEW.quantity ||
      ';status=' || :NEW.status
    );
  ELSIF UPDATING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values, new_values)
    VALUES (
      'PURCHASE_ORDERS',
      'U',
      :NEW.id,
      'medication_id=' || :OLD.medication_id ||
      ';quantity=' || :OLD.quantity ||
      ';status=' || :OLD.status,
      'medication_id=' || :NEW.medication_id ||
      ';quantity=' || :NEW.quantity ||
      ';status=' || :NEW.status
    );
  ELSIF DELETING THEN
    INSERT INTO audit_log (table_name, operation, pk_id, old_values)
    VALUES (
      'PURCHASE_ORDERS',
      'D',
      :OLD.id,
      'medication_id=' || :OLD.medication_id ||
      ';quantity=' || :OLD.quantity ||
      ';status=' || :OLD.status
    );
  END IF;
END;
/

