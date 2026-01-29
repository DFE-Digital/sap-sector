-- ================================================================
-- 00_cleanup.sql
-- Drops generated objects so the pipeline can re-run idempotently.
-- Creates functions used by later steps
-- Schema-agnostic: operates on current_schema()
-- Assumptions:
--   - generated tables start with: t_
--   - generated materialized views start with: v_
-- ================================================================

\echo 'Cleaning up generated objects and regenerating reusable functions...'

DO $$
DECLARE
  v_schema text := current_schema();
  r record;
BEGIN
  -- Ensure subsequent CREATEs land in the active schema
  EXECUTE format('SET search_path TO %I', v_schema);

  -- 1) Drop generated materialized views (v_*)
  FOR r IN
    SELECT schemaname, matviewname
    FROM pg_matviews
    WHERE schemaname = v_schema
      AND matviewname LIKE 'v\_%' ESCAPE '\'
  LOOP
    EXECUTE format('DROP MATERIALIZED VIEW IF EXISTS %I.%I CASCADE', r.schemaname, r.matviewname);
  END LOOP;

  -- 2) Drop generated tables (t_*, raw_*)
  FOR r IN
    SELECT schemaname, tablename
    FROM pg_tables
    WHERE schemaname = v_schema
      AND (tablename LIKE 't\_%' ESCAPE '\' OR tablename LIKE 'raw\_%' ESCAPE '\')
  LOOP
    EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE', r.schemaname, r.tablename);
  END LOOP;

  -- 3) Drop helper functions (defensive, in current schema)
  EXECUTE 'DROP FUNCTION IF EXISTS clean_int(TEXT)';
  EXECUTE 'DROP FUNCTION IF EXISTS clean_numeric(TEXT)';
  EXECUTE 'DROP FUNCTION IF EXISTS clean_date(TEXT)';
END $$;

-- =========================
-- Cleaning helpers
-- =========================

CREATE OR REPLACE FUNCTION clean_int(value TEXT)
RETURNS INT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF value IS NULL OR trim(value) IN (
        '', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z'
    ) THEN
        RETURN NULL;
    END IF;

    RETURN value::INT;

EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

CREATE OR REPLACE FUNCTION clean_numeric(value TEXT)
RETURNS NUMERIC
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF value IS NULL OR trim(value) IN (
        '', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z'
    ) THEN
        RETURN NULL;
    END IF;

    RETURN value::NUMERIC;

EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

CREATE OR REPLACE FUNCTION clean_date(value TEXT)
RETURNS DATE
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF value IS NULL OR trim(value) IN (
        '', 'na', 'n/a', 'N/A', '.', '-', '--'
    ) THEN
        RETURN NULL;
    END IF;

    RETURN value::DATE;

EXCEPTION WHEN others THEN
    RETURN NULL;
END;
$$;

\echo 'Cleanup complete.'
