-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment_email

DROP MATERIALIZED VIEW IF EXISTS v_establishment_email;

CREATE MATERIALIZED VIEW v_establishment_email AS
WITH
src_1 AS (
    SELECT
        t."urn" AS "Id",
        MAX(CASE WHEN TRUE THEN t."urn" END) AS "URN",
        MAX(CASE WHEN TRUE THEN t."establishmentnumber" END) AS "EstablishmentNumber",
        MAX(CASE WHEN TRUE THEN t."establishmentname" END) AS "EstablishmentName",
        MAX(CASE WHEN TRUE THEN t."typeofestablishment__name_" END) AS "TypeOfEstablishmentName",
        MAX(CASE WHEN TRUE THEN t."establishmenttypegroup__name_" END) AS "EstablishmentTypeGroupName",
        MAX(CASE WHEN TRUE THEN t."establishmentstatus__name_" END) AS "EstablishmentStatusName",
        MAX(CASE WHEN TRUE THEN t."closedate" END) AS "CloseDate",
        MAX(CASE WHEN TRUE THEN t."phaseofeducation__name_" END) AS "PhaseOfEducationName",
        MAX(CASE WHEN TRUE THEN t."mainemail" END) AS "MainEmail"
    FROM t_secondary_email_addr_c1553d4c65 t
    GROUP BY t."urn"
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
    src_1."CloseDate" AS "CloseDate",
    src_1."EstablishmentName" AS "EstablishmentName",
    src_1."EstablishmentNumber" AS "EstablishmentNumber",
    src_1."EstablishmentStatusName" AS "EstablishmentStatusName",
    src_1."EstablishmentTypeGroupName" AS "EstablishmentTypeGroupName",
    src_1."MainEmail" AS "MainEmail",
    src_1."PhaseOfEducationName" AS "PhaseOfEducationName",
    src_1."TypeOfEstablishmentName" AS "TypeOfEstablishmentName",
    src_1."URN" AS "URN"
FROM all_ids a
LEFT JOIN src_1 ON src_1."Id" = a."Id"
LEFT JOIN v_establishment e ON e."URN" = a."Id"
;
