-- AUTO-GENERATED MATERIALIZED VIEW: v_la_absence

DROP MATERIALIZED VIEW IF EXISTS v_la_absence;

CREATE MATERIALIZED VIEW v_la_absence AS
WITH
src_1 AS (
    SELECT
        t."old_la_code" AS "Id",
        MAX(CASE WHEN (t."time_period" = '202324') AND (t."education_phase" = 'State-funded secondary') THEN t."sess_overall_percent" END) AS "Abs_Tot_LA_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') AND (t."education_phase" = 'State-funded secondary') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_LA_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') AND (t."education_phase" = 'State-funded secondary') THEN t."sess_authorised_percent" END) AS "Auth_Tot_LA_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') AND (t."education_phase" = 'State-funded secondary') THEN t."sess_unauthorised_percent" END) AS "UnAuth_Tot_LA_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202223') AND (t."education_phase" = 'State-funded secondary') THEN t."sess_overall_percent" END) AS "Abs_Tot_LA_Previous_Pct",
        MAX(CASE WHEN (t."time_period" = '202223') AND (t."education_phase" = 'State-funded secondary') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_LA_Previous_Pct",
        MAX(CASE WHEN (t."time_period" = '202122') AND (t."education_phase" = 'State-funded secondary') THEN t."sess_overall_percent" END) AS "Abs_Tot_LA_Previous2_Pct",
        MAX(CASE WHEN (t."time_period" = '202122') AND (t."education_phase" = 'State-funded secondary') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_LA_Previous2_Pct"
    FROM t_1_absence_3term_nat__2642eb995e t
    GROUP BY t."old_la_code"
)
,
all_ids AS (
    SELECT "Id" FROM src_1
)

SELECT
    a."Id" AS "Id",
    src_1."Abs_Persistent_LA_Current_Pct" AS "Abs_Persistent_LA_Current_Pct",
    src_1."Abs_Persistent_LA_Previous_Pct" AS "Abs_Persistent_LA_Previous_Pct",
    src_1."Abs_Persistent_LA_Previous2_Pct" AS "Abs_Persistent_LA_Previous2_Pct",
    src_1."Abs_Tot_LA_Current_Pct" AS "Abs_Tot_LA_Current_Pct",
    src_1."Abs_Tot_LA_Previous_Pct" AS "Abs_Tot_LA_Previous_Pct",
    src_1."Abs_Tot_LA_Previous2_Pct" AS "Abs_Tot_LA_Previous2_Pct",
    src_1."Auth_Tot_LA_Current_Pct" AS "Auth_Tot_LA_Current_Pct",
    src_1."UnAuth_Tot_LA_Current_Pct" AS "UnAuth_Tot_LA_Current_Pct"
FROM all_ids a
LEFT JOIN src_1 ON src_1."Id" = a."Id"
;
