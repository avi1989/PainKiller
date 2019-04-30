module PainKiller.DataAccess.DatabaseHelpers
open System.Data

let getReader (connection: IDbConnection) (query: string) =
    if (connection.State <> ConnectionState.Open)
    then connection.Open() |> ignore
    use command = connection.CreateCommand()
    command.CommandText <- query
    command.ExecuteReader()

