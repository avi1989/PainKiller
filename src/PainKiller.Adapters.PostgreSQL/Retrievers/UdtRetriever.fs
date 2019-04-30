module PainKiller.Adapters.PostgreSQL.Retrievers.UdtRetriever

open System.Data
open PainKiller.Adapters.PostgreSQL.ColumnTypeMapper
open PainKiller.DataAccess.DatabaseHelpers
open PainKiller.Abstractions.Models


let loadColumnsForUserDefinedType (connection: IDbConnection) schema column =
    use reader = getReader connection (DatabaseRetrieverQueries.UdtQueries.getUserColumnsForUserDefinitedType schema column)
    [while reader.Read() do
        let ordinalPosition = reader.GetInt32(reader.GetOrdinal("ordinal_position"))
        let isNullable = if reader.GetString(reader.GetOrdinal("is_nullable")) = "YES" 
                            then true 
                            else false
        let dataTypeStr = reader.GetString(reader.GetOrdinal("data_type"))
        let charMaxLength = if reader.IsDBNull(reader.GetOrdinal("character_maximum_length")) 
                            then None 
                            else Some (reader.GetInt32(reader.GetOrdinal("character_maximum_length")))
        yield { name = reader.GetString(reader.GetOrdinal("attribute_name"))
                position = ordinalPosition
                ``type`` = mapDatabaseToDomain dataTypeStr charMaxLength
                isNullable = isNullable}
    ]

let loadUserDefinedTypes (connection: IDbConnection) connectionFactory = 
    use reader = getReader connection DatabaseRetrieverQueries.UdtQueries.getUserDefinedTypesQuery
    let alternateConnection = connectionFactory()
    [ while reader.Read() do
        let schema = reader.GetString(reader.GetOrdinal("schema"))
        let name = reader.GetString(reader.GetOrdinal("name"))
        let attributes = loadColumnsForUserDefinedType alternateConnection schema name
        yield {
            name = name
            schema = schema
            attributes = attributes
        }
    ]