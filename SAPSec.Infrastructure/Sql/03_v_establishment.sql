-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment

DROP MATERIALIZED VIEW IF EXISTS v_establishment CASCADE;

CREATE MATERIALIZED VIEW v_establishment AS
SELECT
    t."URN"                                AS "URN",
    t."LA (code)"                          AS "LAId",
    t."LA (name)"                          AS "LAName",
    t."EstablishmentName"                  AS "EstablishmentName",
    clean_int(t."EstablishmentNumber")     AS "EstablishmentNumber",

    clean_int(t."Trusts (code)")            AS "TrustsId",
    t."Trusts (name)"                      AS "TrustName",

    clean_int(t."AdmissionsPolicy (code)") AS "AdmissionsPolicyId",
    t."AdmissionsPolicy (name)"            AS "AdmissionPolicy",

    t."DistrictAdministrative (code)"      AS "DistrictAdministrativeId",
    t."DistrictAdministrative (name)"      AS "DistrictAdministrativeName",

    clean_int(t."PhaseOfEducation (code)") AS "PhaseOfEducationId",
    t."PhaseOfEducation (name)"             AS "PhaseOfEducationName",

    clean_int(t."Gender (code)")            AS "GenderId",
    t."Gender (name)"                       AS "GenderName",

    clean_int(t."OfficialSixthForm (code)") AS "OfficialSixthFormId",
    clean_int(t."ReligiousCharacter (code)") AS "ReligiousCharacterId",
    t."ReligiousCharacter (name)"           AS "ReligiousCharacterName",

    t."TelephoneNum"                        AS "TelephoneNum",
    clean_int(t."NumberOfPupils")           AS "TotalPupils",

    clean_int(t."TypeOfEstablishment (code)") AS "TypeOfEstablishmentId",
    t."TypeOfEstablishment (name)"          AS "TypeOfEstablishmentName",

    clean_int(t."ResourcedProvisionOnRoll") AS "ResourcedProvision",
    t."TypeOfResourcedProvision (name)"     AS "ResourcedProvisionName",

    clean_int(t."UKPRN")                    AS "UKPRN",

    t."Street"                              AS "Street",
    t."Locality"                            AS "Locality",
    t."Address3"                            AS "Address3",
    t."Town"                                AS "Town",
    t."County (name)"                       AS "County",
    t."Postcode"                            AS "Postcode",

    t."HeadTitle (name)"                    AS "HeadTitle",
    t."HeadFirstName"                       AS "HeadFirstName",
    t."HeadLastName"                        AS "HeadLastName",
    t."HeadPreferredJobTitle"               AS "HeadPreferredJobTitle",

    t."UrbanRural (code)"                   AS "UrbanRuralId",
    t."UrbanRural (name)"                   AS "UrbanRuralName",

    t."SchoolWebsite"                       AS "Website",
    clean_int(t."Easting")                  AS "Easting",
    clean_int(t."Northing")                 AS "Northing"
FROM raw_edubasealldata2025_4db2d83a t;

CREATE UNIQUE INDEX idx_v_establishment_urn ON v_establishment ("URN");
