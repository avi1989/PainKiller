module PainKiller.ConsoleApp.PostgreSQL.Retrievers.IndexRetriever
open Npgsql
open System.Data
open PainKiller.ConsoleApp.Models

let getIndexQuery = """
SELECT
n.nspname  as "schema"
    ,t.relname  as "table"
    ,c.relname  as "index"
    ,c.relkind as "kind"
    ,pg_get_indexdef(indexrelid) as "def"
FROM pg_catalog.pg_class c
    JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
    JOIN pg_catalog.pg_index i ON i.indexrelid = c.oid
    JOIN pg_catalog.pg_class t ON i.indrelid   = t.oid
WHERE  c.relkind = 'i' AND
  n.nspname not in ('pg_catalog', 'pg_toast') AND
  pg_catalog.pg_table_is_visible(c.oid) AND
  pg_get_indexdef(indexrelid) NOT LIKE '%UNIQUE%btree%'
ORDER BY
n.nspname
    ,t.relname
    ,c.relname;
"""
let getAllIndexes (connection: NpgsqlConnection) (tables: TableInfo list) =
    ""