-- ================================================================
-- 00_cleanup.sql
-- Drops generated objects so the pipeline can re-run idempotently.
-- Creates functions used by later steps
-- ================================================================

\echo 'Cleaning up existing objects and regenerating reusable functions...'

-- Drop pub schema objects
DROP SCHEMA IF EXISTS pub CASCADE;

-- Drop known dimensions/facts/staging
DO $$
DECLARE r RECORD;
BEGIN
  FOR r IN
    SELECT schemaname, tablename
    FROM pg_tables
    WHERE schemaname = 'public'
       AND tablename LIKE 'raw_%'
  LOOP
    EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE', r.schemaname, r.tablename);
  END LOOP;
END $$;

-- Drop helper functions
DROP FUNCTION IF EXISTS clean_int(TEXT);
DROP FUNCTION IF EXISTS clean_numeric(TEXT);
DROP FUNCTION IF EXISTS clean_date(TEXT);

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
