-- AUTO-GENERATED MATERIALIZED VIEW: v_similar_schools_primary_groups

DROP MATERIALIZED VIEW IF EXISTS v_similar_schools_primary_groups;

CREATE MATERIALIZED VIEW v_similar_schools_primary_groups AS
SELECT
    src_1."urn" AS "URN",
    src_1."neighbour_urn" AS "NeighbourURN",
    src_1."dist" AS "Dist",
    src_1."rank" AS "Rank"
FROM t_2026_01_13_off_sen_p_da7ffb33e2 src_1
;
