-- ================================================================
-- run_all.sql
-- ================================================================

\set ON_ERROR_STOP on

\ir 00_cleanup.sql
\ir 01_create_raw_tables.sql
\ir 02_copy_into_raw_local.sql
\ir 03_v_england_destinations.sql
\ir 03_v_england_performance.sql
\ir 03_v_establishment.sql
\ir 03_v_establishment_links.sql
\ir 03_v_establishment_group_links.sql
\ir 03_v_establishment_subject_entries.sql
\ir 03_v_establishment_absence.sql
\ir 03_v_establishment_destinations.sql
\ir 03_v_establishment_performance.sql
\ir 03_v_establishment_workforce.sql
\ir 03_v_la_destinations.sql
\ir 03_v_la_performance.sql
\ir 03_v_la_subject_entries.sql
\ir 04_indexes.sql
--\ir 05_validation.sql
\ir 50_v_similar_schools_secondary_groups.sql
\ir 50_v_similar_schools_secondary_values.sql
\ir 50_v_similar_schools_primary_groups.sql
\ir 50_v_similar_schools_primary_values.sql
\ir 51_similar_schools_indexes.sql