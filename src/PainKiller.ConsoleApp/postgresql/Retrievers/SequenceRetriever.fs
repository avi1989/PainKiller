module PainKiller.ConsoleApp.PostgreSQL.Retrievers.SequenceRetriever
open Npgsql
open System.Data
open PainKiller.ConsoleApp.Models

let getSequenceQuery = """
SELECT sequence_schema, sequence_name, data_type, increment, minimum_value, start_value 
FROM information_schema.sequences
"""

let getSequences (connection: NpgsqlConnection) =
    if connection.State <> ConnectionState.Open
    then connection.Open() |> ignore

    use command = connection.CreateCommand()
    command.CommandText <- getSequenceQuery

    use reader = command.ExecuteReader()
    [while reader.Read() do
        let schema = reader.GetString(reader.GetOrdinal("sequence_schema"))
        let name = reader.GetString(reader.GetOrdinal("sequence_name"))
        let dataType = reader.GetString(reader.GetOrdinal("data_type"))
        let increment = reader.GetString(reader.GetOrdinal("increment")) |> int
        let minVal = reader.GetString(reader.GetOrdinal("minimum_value")) |> int
        let startVal = reader.GetString(reader.GetOrdinal("start_value")) |> int

        let query = sprintf "CREATE SEQUENCE IF NOT EXISTS %s.%s AS %s
                             INCREMENT BY %i
                             MINVALUE %i
                             START WITH %i" schema name dataType increment minVal startVal
        yield { SimpleDatabaseItem.name = name 
                SimpleDatabaseItem.schema = schema
                SimpleDatabaseItem.definition = query }
    ]