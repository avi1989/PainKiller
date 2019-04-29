module PainKiller.Adapters.PostgreSQL.TableLoader
open System.Data
open PainKiller.Adapters.PostgreSQL.DatabaseRetrieverQueries
open PainKiller.Abstractions.Models

let getCommand (connection: IDbConnection) =
    if (connection.State <> ConnectionState.Open)
    then connection.Open() |> ignore
    connection.CreateCommand()

let loadColumns connection schemaName tableName =
    use command = getCommand(connection)
    command.CommandText <- TableQueries.getColumns schemaName tableName
    use reader = command.ExecuteReader()
    [ while reader.Read() do
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
                isNullable = isNullable
                tableName = tableName
                schemaName = schemaName }
    ]

let loadTable (connection: IDbConnection) (connectionFactory: unit -> IDbConnection) =
    let secondConnection = connectionFactory()
    use command = getCommand connection
    command.CommandText <- TableQueries.getAllTablesQuery
    use reader = command.ExecuteReader()
    [while reader.Read() do
        let tableName = reader.GetString(reader.GetOrdinal("table_name"))
        let schemaName = reader.GetString(reader.GetOrdinal("table_schema"))
        let columns = loadColumns secondConnection schemaName tableName
        let constraints = loadConstraints tableName schemaName
        let foreignKeys = loadForeignKey tableName schemaName
        yield { name = tableName
                schema = schemaName
                columns = columns 
                constraints = constraints @ foreignKeys }
    ]