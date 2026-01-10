-- Grant execute permissions to APP and restrict direct table access
-- Execute as SYS/DBA (or API owner where applicable).

-- [1] Allow APP to execute API procedures
GRANT EXECUTE ON api.patient_insert  TO app;
GRANT EXECUTE ON api.patient_get_all TO app;
GRANT EXECUTE ON api.patient_update  TO app;
GRANT EXECUTE ON api.patient_get_page TO app;
GRANT EXECUTE ON api.patient_get_page_min TO app;
GRANT EXECUTE ON api.patient_get_by_id TO app;
GRANT EXECUTE ON api.patient_search_page_min TO app;

GRANT EXECUTE ON api.encounter_insert  TO app;
GRANT EXECUTE ON api.encounter_update  TO app;
GRANT EXECUTE ON api.encounter_get_by_patient TO app;
GRANT EXECUTE ON api.encounter_get_with_patient_by_rrn_hash TO app;
GRANT EXECUTE ON api.encounter_get_by_patient_page TO app;

GRANT EXECUTE ON api.visit_insert TO app;
GRANT EXECUTE ON api.visit_update TO app;
GRANT EXECUTE ON api.visit_get_by_patient TO app;
GRANT EXECUTE ON api.visit_get_by_patient_page TO app;

-- Appointments
GRANT EXECUTE ON api.appointment_get_distinct_doctor_names TO app;
GRANT EXECUTE ON api.appointment_get_by_patient TO app;
GRANT EXECUTE ON api.appointment_get_by_doctor_and_date TO app;
GRANT EXECUTE ON api.appointment_insert TO app;
GRANT EXECUTE ON api.appointment_update TO app;
GRANT EXECUTE ON api.appointment_cancel TO app;

-- Medications
GRANT EXECUTE ON api.medication_get_all TO app;
GRANT EXECUTE ON api.medication_get_by_id TO app;
GRANT EXECUTE ON api.medication_insert TO app;
GRANT EXECUTE ON api.medication_update TO app;

-- Inventory
GRANT EXECUTE ON api.inventory_get_quantity TO app;
GRANT EXECUTE ON api.inventory_upsert_quantity TO app;
GRANT EXECUTE ON api.inventory_increase TO app;
GRANT EXECUTE ON api.inventory_decrease TO app;

-- Purchase Orders
GRANT EXECUTE ON api.purchase_order_insert_or_increase_pending TO app;
GRANT EXECUTE ON api.purchase_order_get_by_id TO app;
GRANT EXECUTE ON api.purchase_order_update_status TO app;

-- Audit readers
GRANT EXECUTE ON api.audit_get_recent TO app;
GRANT EXECUTE ON api.audit_get_by_table TO app;

-- [2] Ensure APP cannot access DATA tables directly (ignore errors if not granted)
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.patients FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.encounters FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.visits FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.appointments FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.medications FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.inventory FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN
  EXECUTE IMMEDIATE 'REVOKE SELECT, INSERT, UPDATE, DELETE ON data.purchase_orders FROM app';
EXCEPTION WHEN OTHERS THEN NULL; END;
/

