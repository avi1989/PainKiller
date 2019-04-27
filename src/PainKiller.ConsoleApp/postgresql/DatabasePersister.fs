namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.ConsoleApp.PostgreSQL.Persisters
open PainKiller.ConsoleApp.Models

module Helpers = 
    let getNewUdts (udtsInDatabase: UdtInfo list) (udtsInFileSystem: UdtInfo list) =
        udtsInFileSystem
            |> List.filter (fun x -> not (udtsInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

    let getNewTables (tablesInDatabase: TableInfo list) (tablesInFileSystem: TableInfo list) =
        tablesInFileSystem
            |> List.filter ( fun x -> not (tablesInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

type DatabasePersister() =
    interface IDatabasePersister with
        member this.PersistDatabase connectionString currentStateOfDatabase databaseFromFileSystem =
            let connectionFactory = fun () -> new NpgsqlConnection(connectionString)
            use connection = connectionFactory()
            let newTables = Helpers.getNewTables currentStateOfDatabase.tables databaseFromFileSystem.tables
            let newUdts = Helpers.getNewUdts currentStateOfDatabase.userDefinedTypes databaseFromFileSystem.userDefinedTypes

            databaseFromFileSystem.schemas |> SchemaWriter.createSchemas connection
            newTables |> TableWriter.createTables connection
            newUdts |> UdtWriter.createUdts connection
            databaseFromFileSystem.views |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.functions |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.procedures |> SimpleScriptWriter.writeSimpleScripts connection
            newTables |> ConstraintPersister.createConstraintsForTables connection



