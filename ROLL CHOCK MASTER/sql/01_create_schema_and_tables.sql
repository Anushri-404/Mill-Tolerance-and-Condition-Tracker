-- ============================================================
-- Roll Chock Master — schema + table setup
-- Run this against your Oracle XE instance (XEPDB1), as a DBA
-- user (e.g. SYSTEM), using SQL Developer or sqlplus.
-- This gives Roll Chock its OWN schema, separate from spmuser.
-- ============================================================

ALTER SESSION SET CONTAINER = XEPDB1;

-- 1. Create a dedicated schema/user for Roll Chock Master.
--    Change the password before running in anything but local dev.
CREATE USER chockuser IDENTIFIED BY chockpass
  DEFAULT TABLESPACE USERS
  TEMPORARY TABLESPACE TEMP;

ALTER USER chockuser QUOTA UNLIMITED ON USERS;

GRANT CONNECT, RESOURCE TO chockuser;
GRANT CREATE SESSION, CREATE TABLE, CREATE VIEW, CREATE SEQUENCE TO chockuser;

-- ============================================================
-- Everything below this line: connect AS chockuser / chockpass
-- (new connection in SQL Developer) before running.
-- ============================================================
