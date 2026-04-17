-- ================================================================
-- Indexes for materialized views (AUTO-GENERATED)
-- ================================================================

-- NOTE:
-- - Uses quoted identifiers to respect case-sensitive columns

CREATE INDEX IF NOT EXISTS idx_v_similar_schools_primary_groups_urn
ON public.v_similar_schools_primary_groups ("URN");

CREATE INDEX IF NOT EXISTS idx_v_similar_schools_primary_values_urn
ON public.v_similar_schools_primary_values ("URN");

CREATE INDEX IF NOT EXISTS idx_v_similar_schools_secondary_groups_urn
ON public.v_similar_schools_secondary_groups ("URN");

CREATE INDEX IF NOT EXISTS idx_v_similar_schools_secondary_values_urn
ON public.v_similar_schools_secondary_values ("URN");

