module PainKiller.Adapters.PostgreSQL.Retrievers.SequenceRetriever
open System.Data
open PainKiller.DataAccess.DatabaseHelpers
open PainKiller.Abstractions.Models

let getSequences (connection:IDbConnection) =
    use reader = getReader connection DatabaseRetrieverQueries.SequenceQueries.getSequencesQuery
    [while reader.Read() do
        let schema = reader.GetString("sequence_schema")
        let name = reader.GetString("sequence_name")
        let dataType = reader.GetString("data_type")
        let increment = reader.GetString("increment") |> int
        let minVal = reader.GetString("minimum_value") |> int
        let startVal = reader.GetString("start_value") |> int

        let query = sprintf "CREATE SEQUENCE IF NOT EXISTS %s.%s AS %s
                                INCREMENT BY %i
                                MINVALUE %i
                                START WITH %i" schema name dataType increment minVal startVal
        yield { name = name 
                schema = schema
                definition = query }
    ]
