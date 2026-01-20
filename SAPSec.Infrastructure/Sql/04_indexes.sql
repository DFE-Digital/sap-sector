-- ================================================================
-- 04_indexes.sql
-- Indexes for materialized views (AUTO-GENERATED)
-- ================================================================

-- NOTE:
-- - Uses quoted identifiers to respect case-sensitive columns

CREATE INDEX IF NOT EXISTS idx_v_england_destinations_id
ON public.v_england_destinations ("Id");

CREATE INDEX IF NOT EXISTS idx_v_england_performance_id
ON public.v_england_performance ("Id");

CREATE INDEX IF NOT EXISTS idx_v_establishment_urn
ON public.v_establishment ("URN");

CREATE INDEX IF NOT EXISTS idx_v_establishment_absence_id
ON public.v_establishment_absence ("Id");

CREATE INDEX IF NOT EXISTS idx_v_establishment_destinations_id
ON public.v_establishment_destinations ("Id");

CREATE INDEX IF NOT EXISTS idx_v_establishment_performance_id
ON public.v_establishment_performance ("Id");

CREATE INDEX IF NOT EXISTS idx_v_establishment_workforce_id
ON public.v_establishment_workforce ("Id");

CREATE INDEX IF NOT EXISTS idx_v_la_destinations_id
ON public.v_la_destinations ("Id");

CREATE INDEX IF NOT EXISTS idx_v_la_performance_id
ON public.v_la_performance ("Id");

