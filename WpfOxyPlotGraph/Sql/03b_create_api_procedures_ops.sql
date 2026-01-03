го-- Additional API procedures for appointments, medications, inventory, and purchase orders
-- Connect as:  CONNECT api/"Api#Pwd1"@XEPDB1
-- All procedures use AUTHID DEFINER and fully-qualified DATA schema

-- ============ AUDIT READ ============
CREATE OR REPLACE PROCEDURE audit_get_recent
(
  p_limit IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT audit_id, table_name, operation, pk_id, changed_at, changed_by, old_values, new_values
      FROM data.audit_log
     ORDER BY audit_id DESC
     FETCH FIRST NVL(p_limit, 100) ROWS ONLY;
END;
/

CREATE OR REPLACE PROCEDURE audit_get_by_table
(
  p_table_name IN VARCHAR2,
  p_limit IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT audit_id, table_name, operation, pk_id, changed_at, changed_by, old_values, new_values
      FROM data.audit_log
     WHERE table_name = UPPER(p_table_name)
     ORDER BY audit_id DESC
     FETCH FIRST NVL(p_limit, 100) ROWS ONLY;
END;
/

-- ============ APPOINTMENTS ============
CREATE OR REPLACE PROCEDURE appointment_get_distinct_doctor_names
(
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT DISTINCT doctor_name
      FROM data.appointments
     ORDER BY doctor_name;
END;
/

CREATE OR REPLACE PROCEDURE appointment_get_by_patient
(
  p_patient_id IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, patient_id, appointment_date, doctor_name, reason, status, notes, created_at, updated_at
      FROM data.appointments
     WHERE patient_id = p_patient_id
     ORDER BY appointment_date DESC, id DESC;
END;
/

CREATE OR REPLACE PROCEDURE appointment_get_by_doctor_and_date
(
  p_doctor_name IN VARCHAR2,
  p_date IN DATE,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, patient_id, appointment_date, doctor_name, reason, status, notes, created_at, updated_at
      FROM data.appointments
     WHERE doctor_name = p_doctor_name
       AND TRUNC(appointment_date) = TRUNC(p_date)
     ORDER BY appointment_date ASC, id ASC;
END;
/

CREATE OR REPLACE PROCEDURE appointment_insert
(
  p_patient_id IN NUMBER,
  p_appointment_date IN TIMESTAMP,
  p_doctor_name IN VARCHAR2,
  p_reason IN VARCHAR2,
  p_status IN VARCHAR2,
  p_notes IN CLOB,
  p_id_out OUT NUMBER
) AUTHID DEFINER
AS
BEGIN
  INSERT INTO data.appointments (patient_id, appointment_date, doctor_name, reason, status, notes)
  VALUES (p_patient_id, p_appointment_date, p_doctor_name, p_reason, NVL(p_status, 'Scheduled'), p_notes)
  RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE appointment_update
(
  p_id IN NUMBER,
  p_appointment_date IN TIMESTAMP,
  p_doctor_name IN VARCHAR2,
  p_reason IN VARCHAR2,
  p_status IN VARCHAR2,
  p_notes IN CLOB
) AUTHID DEFINER
AS
BEGIN
  UPDATE data.appointments
     SET appointment_date = p_appointment_date,
         doctor_name = p_doctor_name,
         reason = p_reason,
         status = NVL(p_status, 'Scheduled'),
         notes = p_notes,
         updated_at = CURRENT_TIMESTAMP
   WHERE id = p_id;
END;
/

CREATE OR REPLACE PROCEDURE appointment_cancel
(
  p_id IN NUMBER
) AUTHID DEFINER
AS
BEGIN
  UPDATE data.appointments
     SET status = 'Cancelled',
         updated_at = CURRENT_TIMESTAMP
   WHERE id = p_id;
END;
/

-- ============ MEDICATIONS ============
CREATE OR REPLACE PROCEDURE medication_get_all
(
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name, sku, min_stock, reorder_point, reorder_qty, created_at, updated_at
      FROM data.medications
     ORDER BY name ASC;
END;
/

CREATE OR REPLACE PROCEDURE medication_get_by_id
(
  p_id IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, name, sku, min_stock, reorder_point, reorder_qty, created_at, updated_at
      FROM data.medications
     WHERE id = p_id;
END;
/

CREATE OR REPLACE PROCEDURE medication_insert
(
  p_name IN VARCHAR2,
  p_sku IN VARCHAR2,
  p_min_stock IN NUMBER,
  p_reorder_point IN NUMBER,
  p_reorder_qty IN NUMBER,
  p_id_out OUT NUMBER
) AUTHID DEFINER
AS
BEGIN
  INSERT INTO data.medications (name, sku, min_stock, reorder_point, reorder_qty)
  VALUES (p_name, p_sku, p_min_stock, p_reorder_point, p_reorder_qty)
  RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE medication_update
(
  p_id IN NUMBER,
  p_name IN VARCHAR2,
  p_sku IN VARCHAR2,
  p_min_stock IN NUMBER,
  p_reorder_point IN NUMBER,
  p_reorder_qty IN NUMBER
) AUTHID DEFINER
AS
BEGIN
  UPDATE data.medications
     SET name = p_name,
         sku = p_sku,
         min_stock = p_min_stock,
         reorder_point = p_reorder_point,
         reorder_qty = p_reorder_qty,
         updated_at = CURRENT_TIMESTAMP
   WHERE id = p_id;
END;
/

-- ============ INVENTORY ============
CREATE OR REPLACE PROCEDURE inventory_get_quantity
(
  p_medication_id IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT quantity
      FROM data.inventory
     WHERE medication_id = p_medication_id;
END;
/

CREATE OR REPLACE PROCEDURE inventory_upsert_quantity
(
  p_medication_id IN NUMBER,
  p_quantity IN NUMBER
) AUTHID DEFINER
AS
BEGIN
  MERGE INTO data.inventory i
  USING (SELECT p_medication_id AS medication_id, p_quantity AS quantity FROM dual) src
     ON (i.medication_id = src.medication_id)
   WHEN MATCHED THEN
     UPDATE SET i.quantity = src.quantity, i.updated_at = CURRENT_TIMESTAMP
   WHEN NOT MATCHED THEN
     INSERT (medication_id, quantity) VALUES (src.medication_id, src.quantity);
END;
/

CREATE OR REPLACE PROCEDURE inventory_increase
(
  p_medication_id IN NUMBER,
  p_delta IN NUMBER
) AUTHID DEFINER
AS
BEGIN
  MERGE INTO data.inventory i
  USING (SELECT p_medication_id AS medication_id, p_delta AS delta FROM dual) src
     ON (i.medication_id = src.medication_id)
   WHEN MATCHED THEN
     UPDATE SET i.quantity = i.quantity + src.delta, i.updated_at = CURRENT_TIMESTAMP
   WHEN NOT MATCHED THEN
     INSERT (medication_id, quantity) VALUES (src.medication_id, src.delta);
END;
/

CREATE OR REPLACE PROCEDURE inventory_decrease
(
  p_medication_id IN NUMBER,
  p_delta IN NUMBER
) AUTHID DEFINER
AS
BEGIN
  UPDATE data.inventory
     SET quantity = quantity - p_delta,
         updated_at = CURRENT_TIMESTAMP
   WHERE medication_id = p_medication_id;
  IF SQL%ROWCOUNT = 0 THEN
    INSERT INTO data.inventory (medication_id, quantity) VALUES (p_medication_id, -p_delta);
  END IF;
END;
/

-- ============ PURCHASE ORDERS ============
CREATE OR REPLACE PROCEDURE purchase_order_insert_or_increase_pending
(
  p_medication_id IN NUMBER,
  p_quantity IN NUMBER,
  p_id_out OUT NUMBER
) AUTHID DEFINER
AS
  v_id NUMBER;
BEGIN
  SELECT id INTO v_id
    FROM data.purchase_orders
   WHERE medication_id = p_medication_id
     AND status = 'Pending'
     FETCH FIRST 1 ROWS ONLY;

  UPDATE data.purchase_orders
     SET quantity = quantity + p_quantity,
         updated_at = CURRENT_TIMESTAMP
   WHERE id = v_id;
  p_id_out := v_id;
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    INSERT INTO data.purchase_orders (medication_id, quantity, status)
    VALUES (p_medication_id, p_quantity, 'Pending')
    RETURNING id INTO p_id_out;
END;
/

CREATE OR REPLACE PROCEDURE purchase_order_get_by_id
(
  p_id IN NUMBER,
  p_cursor OUT SYS_REFCURSOR
) AUTHID DEFINER
AS
BEGIN
  OPEN p_cursor FOR
    SELECT id, medication_id, quantity, status, created_at, updated_at
      FROM data.purchase_orders
     WHERE id = p_id;
END;
/

CREATE OR REPLACE PROCEDURE purchase_order_update_status
(
  p_id IN NUMBER,
  p_status IN VARCHAR2
) AUTHID DEFINER
AS
BEGIN
  UPDATE data.purchase_orders
     SET status = p_status,
         updated_at = CURRENT_TIMESTAMP
   WHERE id = p_id;
END;
/


