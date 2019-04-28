namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.ConsoleApp.PostgreSQL.Persisters
open PainKiller.ConsoleApp.Models
open PainKiller.ConsoleApp

module Helpers = 
    let getNewUdts (udtsInDatabase: UdtInfo list) (udtsInFileSystem: UdtInfo list) =
        udtsInFileSystem
            |> List.filter (fun x -> not (udtsInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

    let getNewTables (tablesInDatabase: TableInfo list) (tablesInFileSystem: TableInfo list) =
        tablesInFileSystem
            |> List.filter ( fun x -> not (tablesInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

    let getNewlyAddedColumns (tableInDatabase: TableInfo list) (tableInFileSystem: TableInfo list) =
        let pairedTables = ListHelpers.pairListsWithoutUnpairedItems (fun x y -> x.name = y.name && x.schema = y.schema) tableInFileSystem tableInDatabase
        pairedTables
            |> List.map (fun (tableInFs, tableInDb) -> 
                let changedCols = tableInFs.columns|> List.filter (fun x -> not (tableInDb.columns |> List.exists(fun y -> x.name = y.name)))
                (tableInFs, changedCols))
            |> List.filter(fun (_, x) -> not (List.isEmpty x))


    let getRemovedColumns tablesInDatabase tablesInFileSystem =  getNewlyAddedColumns tablesInFileSystem tablesInDatabase

    let getColumnsWhereTypeChanged =
        true

    let getColumnsWhereDefaultAdded =
        true

    let getColumnsWhereIsNullableChanged =
        true


type DatabasePersister() =
    let persistTables connection databaseTables fileSystemTables =
        let newTables = Helpers.getNewTables databaseTables fileSystemTables
        let tablesWithAddedColumns = Helpers.getNewlyAddedColumns databaseTables fileSystemTables
        let tablesWithRemovedColumns = Helpers.getRemovedColumns databaseTables fileSystemTables

        newTables |> (TableWriter.createTables connection >> (ConstraintPersister.createConstraintsForTables connection)) |> ignore
        tablesWithAddedColumns |> List.iter (fun (table, addedCols) -> TableWriter.addColumns connection table.schema table.name addedCols)
        tablesWithRemovedColumns |> List.iter (fun (table, removedCols) -> TableWriter.dropColumns connection table.schema table.name removedCols)
        "" |> ignore

    interface IDatabasePersister with
        member _this.PersistDatabase connectionString currentStateOfDatabase databaseFromFileSystem =
            let connectionFactory = fun () -> new NpgsqlConnection(connectionString)
            use connection = connectionFactory()
            let newUdts = Helpers.getNewUdts currentStateOfDatabase.userDefinedTypes databaseFromFileSystem.userDefinedTypes
            databaseFromFileSystem.schemas |> SchemaWriter.createSchemas connection

            persistTables connection currentStateOfDatabase.tables databaseFromFileSystem.tables

            newUdts |> UdtWriter.createUdts connection
            databaseFromFileSystem.views |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.functions |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.procedures |> SimpleScriptWriter.writeSimpleScripts connection
            
