module PainKiller.ConsoleApp.PostgreSQL.UdtRetriever

open Npgsql
open System.Data
open ColumnTypeMapper


let getUserDefinedTypesQuery = """
SELECT
    user_defined_type_schema as schema ,
    user_defined_type_name as name
FROM information_schema.user_defined_types t
"""

let getUserColumnsForUserDefinitedType = """
SELECT
    attribute_name,
    ordinal_position,
    is_nullable,
    data_type,
    character_maximum_length
FROM information_schema.attributes
WHERE udt_schema = @schema and udt_name = @name;
"""

type UdtAttributes = {
    name: string;
    position: int;
    ``type``: string;
    isNullable: bool;
}

type UdtInfo = {
    name: string;
    schema: string;
    attributes: UdtAttributes list;
}

let loadColumnsForUserDefinedType (connection: NpgsqlConnection) schema column =
    if (connection.State <> ConnectionState.Open)
    then connection.Open()
    use command = connection.CreateCommand()
    command.CommandText <- getUserColumnsForUserDefinitedType
    command.Parameters.AddWithValue("schema", schema) |> ignore
    command.Parameters.AddWithValue("name", column) |> ignore
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let ordinalPosition = reader.GetInt32(reader.GetOrdinal("ordinal_position"))
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
        yield { name = reader.GetString(reader.GetOrdinal("attribute_name"))
                position = ordinalPosition
                ``type`` = mapColumnType dataType
                isNullable = isNullable}
    ]

let loadUserDefinedTypes connectionString = 
    use connection = new NpgsqlConnection(connectionString)
    use attributeConnection = new NpgsqlConnection(connectionString);
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandText <- getUserDefinedTypesQuery
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let schema = reader.GetString(reader.GetOrdinal("schema"))
        let name = reader.GetString(reader.GetOrdinal("name"))
        let attributes = loadColumnsForUserDefinedType attributeConnection schema name
        yield {
            name = name
            schema = schema
            attributes = attributes
        }
    ]