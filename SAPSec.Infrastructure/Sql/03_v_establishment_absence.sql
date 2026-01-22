-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment_absence

DROP MATERIALIZED VIEW IF EXISTS v_establishment_absence;

CREATE MATERIALIZED VIEW v_establishment_absence AS
WITH
src_1 AS (
    SELECT
        t."school_urn" AS "Id",
        MAX(CASE WHEN t."time_period" = '202122' THEN clean_numeric(t."sess_overall_percent") END) AS "Abs_Tot_Est_Previous2_Pct",
        MAX(CASE WHEN t."time_period" = '202122' THEN clean_numeric(t."enrolments_pa_10_exact_percent") END) AS "Abs_Persistent_Est_Previous2_Pct",
        MAX(CASE WHEN t."time_period" = '202223' THEN clean_numeric(t."sess_overall_percent") END) AS "Abs_Tot_Est_Previous_Pct",
        MAX(CASE WHEN t."time_period" = '202223' THEN clean_numeric(t."enrolments_pa_10_exact_percent") END) AS "Abs_Persistent_Est_Previous_Pct",
        MAX(CASE WHEN t."time_period" = '202324' THEN clean_numeric(t."sess_overall_percent") END) AS "Abs_Tot_Est_Current_Pct",
        MAX(CASE WHEN t."time_period" = '202324' THEN clean_numeric(t."enrolments_pa_10_exact_percent") END) AS "Abs_Persistent_Est_Current_Pct",
        MAX(CASE WHEN t."time_period" = '202324' THEN clean_numeric(t."sess_authorised_percent") END) AS "Auth_Tot_Est_Current_Pct",
        MAX(CASE WHEN t."time_period" = '202324' THEN clean_numeric(t."sess_unauthorised_percent") END) AS "UnAuth_Tot_Est_Current_Pct"
    FROM raw_1a_absence_3term_s_d1b51341 t
    GROUP BY t."school_urn"
)

SELECT
    COALESCE(src_1."Id") AS "Id",
    src_1."Abs_Persistent_Est_Current_Pct" AS "Abs_Persistent_Est_Current_Pct",
    src_1."Abs_Persistent_Est_Previous_Pct" AS "Abs_Persistent_Est_Previous_Pct",
    src_1."Abs_Persistent_Est_Previous2_Pct" AS "Abs_Persistent_Est_Previous2_Pct",
    src_1."Abs_Tot_Est_Current_Pct" AS "Abs_Tot_Est_Current_Pct",
    src_1."Abs_Tot_Est_Previous_Pct" AS "Abs_Tot_Est_Previous_Pct",
    src_1."Abs_Tot_Est_Previous2_Pct" AS "Abs_Tot_Est_Previous2_Pct",
    src_1."Auth_Tot_Est_Current_Pct" AS "Auth_Tot_Est_Current_Pct",
    src_1."UnAuth_Tot_Est_Current_Pct" AS "UnAuth_Tot_Est_Current_Pct"
FROM src_1
;
