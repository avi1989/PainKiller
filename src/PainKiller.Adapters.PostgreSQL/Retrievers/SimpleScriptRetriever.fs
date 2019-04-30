module PainKiller.Adapters.PostgreSQL.Retrievers.SimpleScriptRetriever


open PainKiller.DataAccess.DatabaseHelpers
open PainKiller.Abstractions.Models
open System.Data

let retrieveSimpleScript (conn: IDbConnection) query =
    use reader = getReader conn query
    [ while reader.Read() do
        let name = reader.GetString(reader.GetOrdinal("name"))
        let schema = reader.GetString(reader.GetOrdinal("schema"))
        let definition = reader.GetString(reader.GetOrdinal("definition"))

        yield { name = name 
                schema = schema
                definition = definition }]
