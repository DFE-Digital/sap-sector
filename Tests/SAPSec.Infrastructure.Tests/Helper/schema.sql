-- schema.sql
-- Creates public.establishment (table) and public.v_establishment (plain view)
-- Safely drops v_establishment regardless of whether it's currently a table/view/matview.

DO $$
DECLARE
k "char";
BEGIN
SELECT c.relkind
INTO k
FROM pg_class c
         JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public'
  AND c.relname = 'v_establishment';

IF k = 'm' THEN
    EXECUTE 'DROP MATERIALIZED VIEW public.v_establishment';
  ELSIF k = 'v' THEN
    EXECUTE 'DROP VIEW public.v_establishment';
  ELSIF k = 'r' THEN
    EXECUTE 'DROP TABLE public.v_establishment';
END IF;
END $$;

DROP TABLE IF EXISTS public.establishment;

CREATE TABLE public.establishment (
                                      "URN" TEXT PRIMARY KEY,
                                      "UKPRN" TEXT NULL,
                                      "DfENumberSearchable" TEXT NULL,
                                      "EstablishmentName" TEXT NULL
);

CREATE VIEW public.v_establishment AS
SELECT
    "URN",
    "UKPRN",
    "DfENumberSearchable",
    "EstablishmentName"
FROM public.establishment;
