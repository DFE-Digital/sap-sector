-- AUTO-GENERATED MATERIALIZED VIEW: v_similar_schools_secondary_values_national_sd
-- Computes population standard deviation (stddev_pop) for each metric.
-- Includes ks2_avg to match UI metric Ks2AverageScore = (ks2_rp + ks2_mp) / 2.

DROP MATERIALIZED VIEW IF EXISTS v_similar_schools_secondary_values_national_sd;

CREATE MATERIALIZED VIEW v_similar_schools_secondary_values_national_sd
TABLESPACE pg_default
AS
SELECT
    count(*)::int AS "RowCount",

    -- Keep these if useful for debugging / reference
    stddev_pop(NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric(10,5))::numeric(10,5) AS "KS2RP",
    stddev_pop(NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric(10,5))::numeric(10,5) AS "KS2MP",

    -- SD for the same metric used in UI
    stddev_pop(
        ((
            NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric +
            NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric
        ) / 2.0)::numeric(10,5)
    )::numeric(10,5) AS "KS2AVG",

    stddev_pop(NULLIF(NULLIF(pp_perc, 'NA'), '')::numeric(10,5))::numeric(10,5)                  AS "PPPerc",
    stddev_pop(NULLIF(NULLIF(percent_eal, 'NA'), '')::numeric(10,5))::numeric(10,5)              AS "PercentEAL",
    stddev_pop(NULLIF(NULLIF(polar4quintile_pupils, 'NA'), '')::numeric(10,5))::numeric(10,5)    AS "Polar4QuintilePupils",
    stddev_pop(NULLIF(NULLIF(p_stability, 'NA'), '')::numeric(10,5))::numeric(10,5)              AS "PStability",
    stddev_pop(NULLIF(NULLIF(idaci_pupils, 'NA'), '')::numeric(10,5))::numeric(10,5)             AS "IdaciPupils",
    stddev_pop(NULLIF(NULLIF(percent_sch_support, 'NA'), '')::numeric(10,5))::numeric(10,5)      AS "PercentSchSupport",
    stddev_pop(NULLIF(NULLIF(number_of_pupils, 'NA'), '')::numeric(10,5))::numeric(10,5)         AS "NumberOfPupils",
    stddev_pop(NULLIF(NULLIF(percent_statement_or_ehp, 'NA'), '')::numeric(10,5))::numeric(10,5) AS "PercentageStatementOrEHP",
    stddev_pop(NULLIF(NULLIF(att8scr, 'NA'), '')::numeric(10,5))::numeric(10,5)                  AS "Att8Scr"
FROM public.t_2026_01_13_off_sen_s_19f2b70939
WITH DATA;
