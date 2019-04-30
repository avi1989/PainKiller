module PainKiller.Adapters.PostgreSQL.Retrievers.SchemaRetriever

open System.Data
open PainKiller.DataAccess.DatabaseHelpers

let getSchemas (connection: IDbConnection) =
    use reader = getReader connection DatabaseRetrieverQueries.SchemaQueries.getSchemaQuery
    [while reader.Read() do
        yield reader.GetString(reader.GetOrdinal("schema_name"))
    ]
