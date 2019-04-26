module PainKiller.ConsoleApp.PostgreSQL.TableRetriever

open Npgsql;
open System.Data;
open ColumnTypeMapper
open PainKiller.ConsoleApp.Models

let getTableQuery = """
SELECT table_name, table_schema
FROM information_schema.tables
WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
AND table_schema NOT LIKE 'pg_toast%'
AND table_type = 'BASE TABLE';
"""

let getColumnQuery = """
SELECT column_name, ordinal_position, column_default, is_nullable, data_type, character_maximum_length 
FROM information_schema.columns
WHERE table_name = @tableName AND table_schema = @schemaName
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
        let dataType = match charMaxLength with
                        | None -> ColumnType.TypeWithoutLength dataTypeStr
                        | Some maxLen -> ColumnType.TypeWithLength (dataTypeStr, maxLen)
        yield { name = reader.GetString(reader.GetOrdinal("column_name"))
                position = ordinalPosition
                ``type`` = mapColumnType dataType
                defaultValue = defaultVal
                isNullable = isNullable}
    ]

let loadTables connectionString =
    use connection = new NpgsqlConnection(connectionString)
    use columnConnection = new NpgsqlConnection(connectionString);
    connection.Open()
    let loadColumn = loadColumnsForTable columnConnection
    use command = connection.CreateCommand();
    command.CommandText <- getTableQuery;
    use reader = command.ExecuteReader();
    [while reader.Read() do
        let tableName = reader.GetString(reader.GetOrdinal("table_name"))
        let schemaName = reader.GetString(reader.GetOrdinal("table_schema"))
        let columns = loadColumn tableName schemaName
        yield { name = tableName
                schema = schemaName
                columns = columns }
    ]
        
    

