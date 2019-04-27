module PainKiller.ConsoleApp.PostgreSQL.Persisters.SimpleScriptWriter

open PainKiller.ConsoleApp.Models
open Npgsql;
open System.Data;

let writeSimpleScript (connection: NpgsqlConnection) (simpleScript: SimpleDatabaseItem) =
    if connection.State <> ConnectionState.Open
    then connection.Open() |> ignore

    use command = connection.CreateCommand()
    command.CommandText <- simpleScript.definition
    command.ExecuteNonQuery() |> ignore

let writeSimpleScripts connection simpleScript =
    simpleScript
    |> List.iter (writeSimpleScript connection)