module PainKiller.Adapters.PostgreSQL.Retrievers.TableRetriever
open System.Data
open PainKiller.Adapters.PostgreSQL.Retrievers.DatabaseRetrieverQueries
open PainKiller.Abstractions.Models
open PainKiller.Adapters.PostgreSQL.ColumnTypeMapper
open PainKiller.DataAccess.DatabaseHelpers

let private loadForeignKeysForTable (conn: IDbConnection) tableName schemaName =
    use reader = getReader conn (TableQueries.getForeignKeys schemaName tableName)
    [while reader.Read() do
        let columns = reader.GetValue("from_cols") :?> string[] |> List.ofSeq
        let destinationColumns = reader.GetValue("to_cols") :?> string[] |> List.ofSeq

        let constraintType = ForeignKey { sourceColumns = columns
                                          destinationColumns = destinationColumns
                                          destinationSchema = reader.GetString("to_table_schema")
                                          destinationTable = reader.GetString("to_table") }
        yield { name = reader.GetString("constraint_name")
                ``type`` = constraintType } ]

let private loadConstraintsForTable (conn: IDbConnection) tableName schemaName =
    use reader = getReader conn (TableQueries.getTableConstraints schemaName tableName)
    [while reader.Read() do
        let columns = reader.GetValue("columns") :?> string[] |> List.ofSeq
        let name = reader.GetString("constraint_name")
        let constraintType = match reader.GetChar("constraint_type") with
                             | 'p' -> PrimaryKey columns
                             | 'u' -> Unique columns
                             | 'c' -> Check (reader.GetString("definition"))
                             | _ -> raise (System.Exception("Unknown constraint Type"))
        yield { name = name
                ``type`` = constraintType }]

let loadColumns connection schemaName tableName =
    use reader = getReader connection (TableQueries.getColumns schemaName tableName)
    [ while reader.Read() do
        let dataTypeStr = reader.GetString("data_type")
        let charMaxLength = reader.GetNullableInt32("character_maximum_length") 

        yield { name = reader.GetString("column_name")
                position = reader.GetInt32("ordinal_position")
                ``type`` = mapDatabaseToDomain dataTypeStr charMaxLength
                defaultValue = reader.GetNullableString("column_default")
                isNullable = reader.GetString("is_nullable") = "YES"
                tableName = tableName
                schemaName = schemaName }
    ]

let loadTable (connection: IDbConnection) (connectionFactory: unit -> IDbConnection) =
    use secondConnection = connectionFactory()
    use reader = getReader connection (TableQueries.getAllTablesQuery)
    [while reader.Read() do
        let tableName = reader.GetString("table_name")
        let schemaName = reader.GetString("table_schema")
        let columns = loadColumns secondConnection schemaName tableName
        let constraints = loadConstraintsForTable secondConnection tableName schemaName
        let foreignKeys = loadForeignKeysForTable secondConnection tableName schemaName
        yield { name = tableName
                schema = schemaName
                columns = columns 
                constraints = constraints @ foreignKeys }]
