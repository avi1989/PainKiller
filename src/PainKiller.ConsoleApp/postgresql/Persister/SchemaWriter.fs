module PainKiller.ConsoleApp.PostgreSQL.Persister.SchemaWriter

open Npgsql
open System.Data
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Models
open System.Text

let createSchema (sqlConnection: NpgsqlConnection) schema =
    let sqlStatement = sprintf "CREATE SCHEMA IF NOT EXISTS %s" schema
    if sqlConnection.State <> ConnectionState.Open
    then sqlConnection.Open() |> ignore

    use command = sqlConnection.CreateCommand()
    command.CommandText <- sqlStatement
    command.ExecuteNonQuery() |> ignore

let createSchemas sqlConnection schemas =
    schemas
    |> List.iter (createSchema sqlConnection)