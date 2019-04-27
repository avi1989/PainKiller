module PainKiller.ConsoleApp.PostgreSQL.Retrievers.ViewRetriever

open Npgsql
open PainKiller.ConsoleApp.Models

let getViewQuery = """
SELECT 
        table_schema as schema, 
        table_name as name, 
        view_definition as definition 
FROM information_schema.views
WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
AND table_schema NOT LIKE 'pg_toast%';
"""

let parseDefinition schema name definition = 
    sprintf """
CREATE OR REPLACE VIEW %s.%s AS (
%s
)""" schema name definition

let loadViews (connection: NpgsqlConnection) =
    if (connection.State <> System.Data.ConnectionState.Open) 
    then connection.Open() |> ignore
    use command = connection.CreateCommand()
    command.CommandText <- getViewQuery
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let name = reader.GetString(reader.GetOrdinal("name"))
        let schema = reader.GetString(reader.GetOrdinal("schema"))
        let definition = reader.GetString(reader.GetOrdinal("definition"))
        yield { 
            name = name
            schema = schema
            definition = definition |> parseDefinition schema name
        }
    ]