module PainKiller.ConsoleApp.PostgreSQL.Persisters.SimpleScriptWriter

open PainKiller.Abstractions.Models
open Npgsql;
open System.Data;

let writeSimpleScript (connection: NpgsqlConnection) (simpleScript: SimpleDatabaseItem) =
    if connection.State <> ConnectionState.Open
    then connection.Open() |> ignore

    let query = simpleScript.definition.Replace("CREATE INDEX", "CREATE INDEX IF NOT EXISTS")
    use command = connection.CreateCommand()
    command.CommandText <- query;
    command.ExecuteNonQuery() |> ignore

let writeSimpleScripts connection simpleScript =
    simpleScript
    |> List.iter (writeSimpleScript connection)