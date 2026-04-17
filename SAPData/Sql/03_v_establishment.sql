-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment

DROP MATERIALIZED VIEW IF EXISTS v_establishment CASCADE;

CREATE MATERIALIZED VIEW v_establishment AS
SELECT
    t."urn"                                      AS "URN",
    t."la__code_"                                AS "LAId",
    t."la__name_"                                AS "LAName",
    t."gor__code_"                               AS "RegionId",
    t."gor__name_"                               AS "RegionName",
    t."establishmentname"                        AS "EstablishmentName",
    t."establishmentnumber"                      AS "EstablishmentNumber",

    t."trusts__code_"                            AS "TrustId",
    t."trusts__name_"                            AS "TrustName",

    t."admissionspolicy__code_"                  AS "AdmissionsPolicyId",
    t."admissionspolicy__name_"                  AS "AdmissionsPolicyName",

    t."districtadministrative__code_"            AS "DistrictAdministrativeId",
    t."districtadministrative__name_"            AS "DistrictAdministrativeName",

    t."phaseofeducation__code_"                  AS "PhaseOfEducationId",
    t."phaseofeducation__name_"                  AS "PhaseOfEducationName",

    t."gender__code_"                            AS "GenderId",
    t."gender__name_"                            AS "GenderName",

    t."religiouscharacter__code_"                AS "ReligiousCharacterId",
    t."religiouscharacter__name_"                AS "ReligiousCharacterName",

    t."telephonenum"                             AS "TelephoneNum",
    clean_int(t."schoolcapacity")                AS "TotalCapacity",
    clean_int(t."numberofpupils")                AS "TotalPupils",

    t."typeofestablishment__code_"               AS "TypeOfEstablishmentId",
    t."typeofestablishment__name_"               AS "TypeOfEstablishmentName",

    t."establishmenttypegroup__code_"            AS "EstablishmentTypeGroupId",
    t."establishmenttypegroup__name_"            AS "EstablishmentTypeGroupName",

    t."resourcedprovisiononroll"                 AS "ResourcedProvisionId",
    t."typeofresourcedprovision__name_"          AS "ResourcedProvisionName",

    t."nurseryprovision__name_"                  AS "NurseryProvisionName",

    t."officialsixthform__code_"                 AS "OfficialSixthFormId",
    t."officialsixthform__name_"                 AS "OfficialSixthFormName",

    t."trustschoolflag__code_"                   AS "TrustSchoolFlagId",
    t."trustschoolflag__name_"                   AS "TrustSchoolFlagName",

    t."ukprn"                                    AS "UKPRN",

    t."street"                                   AS "Street",
    t."locality"                                 AS "Locality",
    t."address3"                                 AS "Address3",
    t."town"                                     AS "Town",
    t."county__name_"                            AS "County",
    t."postcode"                                 AS "Postcode",

    t."headtitle__name_"                         AS "HeadTitle",
    t."headfirstname"                            AS "HeadFirstName",
    t."headlastname"                             AS "HeadLastName",
    t."headpreferredjobtitle"                    AS "HeadPreferredJobTitle",

    t."urbanrural__code_"                        AS "UrbanRuralId",
    t."urbanrural__name_"                        AS "UrbanRuralName",

    t."schoolwebsite"                            AS "Website",

    clean_int(t."easting")                       AS "Easting",
    clean_int(t."northing")                      AS "Northing",

    clean_int(t."statutorylowage")               AS "AgeRangeLow",
    clean_int(t."statutoryhighage")              AS "AgeRangeHigh"
FROM t_edubasealldata202603_301c86d488 t;

CREATE UNIQUE INDEX idx_v_establishment_urn ON v_establishment ("URN");
