-- AUTO-GENERATED MATERIALIZED VIEW: v_similar_schools_primary_values

DROP MATERIALIZED VIEW IF EXISTS v_similar_schools_primary_values;

CREATE MATERIALIZED VIEW v_similar_schools_primary_values AS
SELECT
    src_1."urn" AS "URN",
    src_1."pp_perc" AS "PPPerc",
    src_1."polar4quintile_pupils" AS "Polar4QuintilePupils",
    src_1."p_stability" AS "PStability",
    src_1."percent_sch_support" AS "PercentSchSupport",
    src_1."percent_eal" AS "PercentEAL",
    src_1."idaci_pupils" AS "IdaciPupils",
    src_1."percent_statement_or_ehp" AS "PercentageStatementOrEhp",
    src_1."number_of_pupils" AS "NumberOfPupils",
    src_1."readmat_average" AS "ReadMatAverage"
FROM t_2026_01_13_off_sen_p_e0ebbdc786 src_1
;
