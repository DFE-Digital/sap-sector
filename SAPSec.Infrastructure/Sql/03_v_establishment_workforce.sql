-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment_workforce

DROP MATERIALIZED VIEW IF EXISTS v_establishment_workforce;

CREATE MATERIALIZED VIEW v_establishment_workforce AS
WITH
src_1 AS (
    SELECT
        t."school_urn" AS "Id",
        MAX(CASE WHEN t."time_period" = '202425' THEN t."pupil_to_qual_teacher_ratio" END) AS "Workforce_PupTeaRatio_Est_Current_Num",
        MAX(CASE WHEN t."time_period" = '202425' THEN t."pupils_fte" END) AS "Workforce_TotPupils_Est_Current_Num"
    FROM raw_workforce_ptrs_201_8b26fc7d t
    GROUP BY t."school_urn"
)

SELECT
    COALESCE(src_1."Id") AS "Id",
    src_1."Workforce_PupTeaRatio_Est_Current_Num" AS "Workforce_PupTeaRatio_Est_Current_Num",
    src_1."Workforce_TotPupils_Est_Current_Num" AS "Workforce_TotPupils_Est_Current_Num"
FROM src_1
;
