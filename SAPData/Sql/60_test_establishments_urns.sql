drop table if exists test_establishments_urns_import;
create unlogged table test_establishments_urns_import (doc text);

drop table if exists test_establishments_urns;
create table test_establishments_urns ("URN" text);

\copy test_establishments_urns_import FROM 'C:\Users\nikki\Source\Repos\sap-sector\SAPSec.Infrastructure\Data\Files\TestEstablishmentUrns.json' with (format text);
insert into test_establishments_urns select jsonb_array_elements_text(string_agg(doc, E' ')::jsonb) from test_establishments_urns_import;