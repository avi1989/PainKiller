module PainKiller.ConsoleApp.PostgreSQL.Retrievers.TableRetriever

open Npgsql;
open System.Data;
open PainKiller.ConsoleApp.PostgreSQL.ColumnTypeMapper
open PainKiller.ConsoleApp.Models

let getTableQuery = """
SELECT table_name, table_schema
FROM information_schema.tables
WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
AND table_schema NOT LIKE 'pg_toast%'
AND table_type = 'BASE TABLE';
"""

let getColumnQuery = """
SELECT column_name, ordinal_position, column_default, is_nullable, udt_name::regtype::text as data_type, character_maximum_length 
FROM information_schema.columns
WHERE table_name = @tableName AND table_schema = @schemaName
"""

let getTableConstraintQuery = """
SELECT c.conname                              AS constraint_name,
c.contype                                     AS constraint_type,
sch.nspname                                   AS table_schema,
tbl.relname                                   AS table_name,
ARRAY_AGG(col.attname ORDER BY u.attposition) AS columns,
pg_get_constraintdef(c.oid)                   AS definition
FROM pg_constraint c
JOIN LATERAL UNNEST(c.conkey) WITH ORDINALITY AS u(attnum, attposition) ON TRUE
JOIN pg_class tbl ON tbl.oid = c.conrelid
JOIN pg_namespace sch ON sch.oid = tbl.relnamespace
JOIN pg_attribute col ON (col.attrelid = tbl.oid AND col.attnum = u.attnum)
WHERE contype <> 'f' AND sch.nspname = @schemaName AND tbl.relname = @tableName
GROUP BY constraint_name, constraint_type, table_schema, table_name, definition
ORDER BY table_schema, table_name;
"""

let getForeignKeysQuery = """
SELECT
tc.table_schema,
tc.constraint_name,
tc.table_name,
ARRAY_AGG(kcu.column_name),
ccu.table_schema AS foreign_table_schema,
ccu.table_name AS foreign_table_name,
ARRAY_AGG(ccu.column_name) AS foreign_column_name
FROM
information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
  ON tc.constraint_name = kcu.constraint_name
  AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
  ON ccu.constraint_name = tc.constraint_name
  AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' --AND tc.table_name='test_fk_ref';
GROUP BY tc.table_schema, tc.constraint_name, tc.table_name, foreign_table_name, ccu.table_schema
"""

let loadColumnsForTable (conn:NpgsqlConnection) tableName schemaName =
    if (conn.State <> ConnectionState.Open)
    then conn.Open() |> ignore

    use command = conn.CreateCommand()
    command.CommandText <- getColumnQuery
    command.Parameters.AddWithValue("tableName", tableName) |> ignore
    command.Parameters.AddWithValue("schemaName", schemaName) |> ignore
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let ordinalPosition = reader.GetInt32(reader.GetOrdinal("ordinal_position"))

        let defaultVal = if reader.IsDBNull(reader.GetOrdinal("column_default"))
                            then None
                            else Some (reader.GetString(reader.GetOrdinal("column_default")))

        let isNullable = if reader.GetString(reader.GetOrdinal("is_nullable")) = "YES" 
                            then true 
                            else false
        let dataTypeStr = reader.GetString(reader.GetOrdinal("data_type"))

        let charMaxLength = if reader.IsDBNull(reader.GetOrdinal("character_maximum_length")) 
                            then None 
                            else Some (reader.GetInt32(reader.GetOrdinal("character_maximum_length")))

        yield { name = reader.GetString(reader.GetOrdinal("column_name"))
                position = ordinalPosition
                ``type`` = mapDatabaseToDomain dataTypeStr charMaxLength
                defaultValue = defaultVal
                isNullable = isNullable}
    ]

let loadConstraintsForTable (conn: NpgsqlConnection) tableName schemaName =
    if (conn.State <> ConnectionState.Open)
    then conn.Open() |> ignore

    use command = conn.CreateCommand()
    command.CommandText <- getTableConstraintQuery
    command.Parameters.AddWithValue("tableName", tableName) |> ignore
    command.Parameters.AddWithValue("schemaName", schemaName) |> ignore
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let columns = reader.GetValue(reader.GetOrdinal("columns")) :?> string[] |> List.ofSeq
        let name = reader.GetString(reader.GetOrdinal("constraint_name"))
        let constraintType = match reader.GetChar(reader.GetOrdinal("constraint_type")) with
                                | 'p' -> PrimaryKey columns
                                | 'u' -> Unique columns
                                | 'c' -> Check (reader.GetString(reader.GetOrdinal("definition")))
                                | _ -> raise (System.Exception("Unknown constraint Type"))
        yield { name = name
                ``type`` = constraintType }
    ]

let loadTables (connection: NpgsqlConnection) connectionFactory = 
    use columnConnection = connectionFactory()
    connection.Open()
    let loadColumn = loadColumnsForTable columnConnection
    let loadConstraints = loadConstraintsForTable columnConnection
    use command = connection.CreateCommand();
    command.CommandText <- getTableQuery;
    use reader = command.ExecuteReader();
    [while reader.Read() do
        let tableName = reader.GetString(reader.GetOrdinal("table_name"))
        let schemaName = reader.GetString(reader.GetOrdinal("table_schema"))
        let columns = loadColumn tableName schemaName
        let constraints = loadConstraints tableName schemaName
        yield { name = tableName
                schema = schemaName
                columns = columns 
                constraints = constraints }
    ]
        
    

