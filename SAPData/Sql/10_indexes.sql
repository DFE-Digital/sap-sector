-- ================================================================
-- 04_indexes.sql
-- Indexes for materialized views (AUTO-GENERATED)
-- ================================================================

-- NOTE:
-- - Uses quoted identifiers to respect case-sensitive columns
-- - Uses current_schema() so it works in any schema (local + pipeline)
-- - Guards index creation when a view is not present
-- - Emits NOTICE messages so execution output is descriptive

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_urn not created)', v_schema, 'v_establishment';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_urn')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("URN")', v_schema, 'idx_v_establishment_urn', v_schema, 'v_establishment';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("URN")', 'idx_v_establishment_urn', v_schema, 'v_establishment');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_urn';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_links')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_links_urn not created)', v_schema, 'v_establishment_links';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_links_urn')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("urn")', v_schema, 'idx_v_establishment_links_urn', v_schema, 'v_establishment_links';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("urn")', 'idx_v_establishment_links_urn', v_schema, 'v_establishment_links');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_links_urn';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_group_links')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_group_links_group_id not created)', v_schema, 'v_establishment_group_links';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_group_links_group_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("group_id")', v_schema, 'idx_v_establishment_group_links_group_id', v_schema, 'v_establishment_group_links';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("group_id")', 'idx_v_establishment_group_links_group_id', v_schema, 'v_establishment_group_links');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_group_links_group_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_subject_entries')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_subject_entries_school_urn not created)', v_schema, 'v_establishment_subject_entries';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_subject_entries_school_urn')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("school_urn")', v_schema, 'idx_v_establishment_subject_entries_school_urn', v_schema, 'v_establishment_subject_entries';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("school_urn")', 'idx_v_establishment_subject_entries_school_urn', v_schema, 'v_establishment_subject_entries');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_subject_entries_school_urn';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_absence')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_absence_id not created)', v_schema, 'v_establishment_absence';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_absence_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_establishment_absence_id', v_schema, 'v_establishment_absence';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_establishment_absence_id', v_schema, 'v_establishment_absence');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_absence_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_destinations')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_destinations_id not created)', v_schema, 'v_establishment_destinations';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_destinations_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_establishment_destinations_id', v_schema, 'v_establishment_destinations';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_establishment_destinations_id', v_schema, 'v_establishment_destinations');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_destinations_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_performance')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_performance_id not created)', v_schema, 'v_establishment_performance';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_performance_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_establishment_performance_id', v_schema, 'v_establishment_performance';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_establishment_performance_id', v_schema, 'v_establishment_performance');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_performance_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_establishment_workforce')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_establishment_workforce_id not created)', v_schema, 'v_establishment_workforce';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_establishment_workforce_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_establishment_workforce_id', v_schema, 'v_establishment_workforce';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_establishment_workforce_id', v_schema, 'v_establishment_workforce');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_establishment_workforce_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_england_absence')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_england_absence_id not created)', v_schema, 'v_england_absence';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_england_absence_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_england_absence_id', v_schema, 'v_england_absence';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_england_absence_id', v_schema, 'v_england_absence');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_england_absence_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_england_destinations')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_england_destinations_id not created)', v_schema, 'v_england_destinations';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_england_destinations_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_england_destinations_id', v_schema, 'v_england_destinations';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_england_destinations_id', v_schema, 'v_england_destinations');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_england_destinations_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_england_performance')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_england_performance_id not created)', v_schema, 'v_england_performance';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_england_performance_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_england_performance_id', v_schema, 'v_england_performance';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_england_performance_id', v_schema, 'v_england_performance');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_england_performance_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_la_destinations')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_la_destinations_id not created)', v_schema, 'v_la_destinations';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_la_destinations_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_la_destinations_id', v_schema, 'v_la_destinations';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_la_destinations_id', v_schema, 'v_la_destinations');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_la_destinations_id';
    END IF;
  END IF;
END $$;

DO $$
DECLARE
  v_schema text := current_schema();
BEGIN
  IF to_regclass(format('%I.%I', v_schema, 'v_la_performance')) IS NULL THEN
    RAISE NOTICE 'SKIP: view %.% does not exist (index idx_v_la_performance_id not created)', v_schema, 'v_la_performance';
  ELSE
    IF to_regclass(format('%I.%I', v_schema, 'idx_v_la_performance_id')) IS NULL THEN
      RAISE NOTICE 'CREATE: index %.% on %.% ("Id")', v_schema, 'idx_v_la_performance_id', v_schema, 'v_la_performance';
      EXECUTE format('CREATE INDEX %I ON %I.%I ("Id")', 'idx_v_la_performance_id', v_schema, 'v_la_performance');
    ELSE
      RAISE NOTICE 'OK: index %.% already exists', v_schema, 'idx_v_la_performance_id';
    END IF;
  END IF;
END $$;

