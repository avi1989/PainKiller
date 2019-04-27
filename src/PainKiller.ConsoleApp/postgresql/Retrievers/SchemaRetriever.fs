module PainKiller.ConsoleApp.PostgreSQL.Retrievers.SchemaRetriever
open Npgsql
open System.Data

let getSchemaQuery = """
SELECT * FROM information_schema.schemata
WHERE schema_name not in ('pg_catalog', 'pg_toast')
AND schema_name NOT LIKE 'pg_toast_%'
AND schema_name NOT LIKE 'pg_temp_%';
"""

let getSchemas (connection: NpgsqlConnection) =
    if (connection.State <> ConnectionState.Open)
    then connection.Open() |> ignore

    use command = connection.CreateCommand()
    command.CommandText <- getSchemaQuery

    use reader = command.ExecuteReader()
    [while reader.Read() do
        yield reader.GetString(reader.GetOrdinal("schema_name"))
    ]
    
