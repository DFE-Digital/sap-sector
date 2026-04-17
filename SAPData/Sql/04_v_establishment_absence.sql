-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment_absence

DROP MATERIALIZED VIEW IF EXISTS v_establishment_absence;

CREATE MATERIALIZED VIEW v_establishment_absence AS
WITH
src_1 AS (
    SELECT
        t."school_urn" AS "Id",
        MAX(CASE WHEN (t."time_period" = '202324') THEN t."sess_overall_percent" END) AS "Abs_Tot_Est_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_Est_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') THEN t."sess_authorised_percent" END) AS "Auth_Tot_Est_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202324') THEN t."sess_unauthorised_percent" END) AS "UnAuth_Tot_Est_Current_Pct",
        MAX(CASE WHEN (t."time_period" = '202223') THEN t."sess_overall_percent" END) AS "Abs_Tot_Est_Previous_Pct",
        MAX(CASE WHEN (t."time_period" = '202223') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_Est_Previous_Pct",
        MAX(CASE WHEN (t."time_period" = '202122') THEN t."sess_overall_percent" END) AS "Abs_Tot_Est_Previous2_Pct",
        MAX(CASE WHEN (t."time_period" = '202122') THEN t."enrolments_pa_10_exact_percent" END) AS "Abs_Persistent_Est_Previous2_Pct"
    FROM t_1a_absence_3term_sch_d1b51341e3 t
    GROUP BY t."school_urn"
)
,
all_ids AS (
    SELECT "Id" FROM src_1
)

SELECT
    a."Id" AS "Id",
    e."LAId" AS "LAId",
    e."LAName" AS "LAName",
    e."RegionId" AS "RegionId",
    e."RegionName" AS "RegionName",
    src_1."Abs_Persistent_Est_Current_Pct" AS "Abs_Persistent_Est_Current_Pct",
    src_1."Abs_Persistent_Est_Previous_Pct" AS "Abs_Persistent_Est_Previous_Pct",
    src_1."Abs_Persistent_Est_Previous2_Pct" AS "Abs_Persistent_Est_Previous2_Pct",
    src_1."Abs_Tot_Est_Current_Pct" AS "Abs_Tot_Est_Current_Pct",
    src_1."Abs_Tot_Est_Previous_Pct" AS "Abs_Tot_Est_Previous_Pct",
    src_1."Abs_Tot_Est_Previous2_Pct" AS "Abs_Tot_Est_Previous2_Pct",
    src_1."Auth_Tot_Est_Current_Pct" AS "Auth_Tot_Est_Current_Pct",
    src_1."UnAuth_Tot_Est_Current_Pct" AS "UnAuth_Tot_Est_Current_Pct"
FROM all_ids a
LEFT JOIN src_1 ON src_1."Id" = a."Id"
LEFT JOIN v_establishment e ON e."URN" = a."Id"
;
