module PainKiller.ConsoleApp.PostgreSQL.FunctionRetriever

open Npgsql

type Function = {
    name: string;
    schema: string;
    definition: string;
}

let getFunctionQuery = """
SELECT 
    proc.proname as name,
    ns.nspname as schema,
    pg_get_functiondef(proc.oid) as definition,
    prosrc as function_body
FROM pg_proc proc
LEFT JOIN pg_namespace ns on ns.oid = proc.pronamespace
    WHERE proc.prokind = 'f'
    AND ns.nspname NOT IN ('pg_catalog', 'information_schema')
    AND probin is null;
"""

let loadFunctions connectionString =
    use conn = new NpgsqlConnection(connectionString)
    conn.Open()
    use command = conn.CreateCommand()
    command.CommandText <- getFunctionQuery
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let name = reader.GetString(reader.GetOrdinal("name"))
        let schema = reader.GetString(reader.GetOrdinal("schema"))
        let definition = reader.GetString(reader.GetOrdinal("definition"))

        yield { name = name 
                schema = schema
                definition = definition }
    ]