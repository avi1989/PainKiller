namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.ConsoleApp.PostgreSQL.Persisters
open PainKiller.ConsoleApp.Models

module Helpers = 
    let getFirstMatchingResult fn = 
        List.head << List.filter fn
        //List.tryHead << List.filter fn

    let createTablePairByPredicate fn l1 l2 =
        l1 |> List.map (fun x -> (x, l2 |> (getFirstMatchingResult (fn x))))

    let getNewUdts (udtsInDatabase: UdtInfo list) (udtsInFileSystem: UdtInfo list) =
        udtsInFileSystem
            |> List.filter (fun x -> not (udtsInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

    let getNewTables (tablesInDatabase: TableInfo list) (tablesInFileSystem: TableInfo list) =
        tablesInFileSystem
            |> List.filter ( fun x -> not (tablesInDatabase |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))

    let getNewlyAddedColumns (tableInDatabase: TableInfo list) (tableInFileSystem: TableInfo list) =
        let pairedTables = createTablePairByPredicate (fun x y -> x.name = y.name && x.schema = y.schema) tableInFileSystem tableInDatabase
        pairedTables
            |> List.map (fun (tableInFs, tableInDb) -> 
                let changedCols = tableInFs.columns|> List.filter (fun x -> not (tableInDb.columns |> List.exists(fun y -> x.name = y.name)))
                (tableInFs, changedCols))
            |> List.filter(fun (_, x) -> not (List.isEmpty x))


    let getRemvoedColumns = 
        true

    let getColumnsWhereTypeChanged =
        true

    let getColumnsWhereDefaultAdded =
        true

    let getColumnsWhereIsNullableChanged =
        true


type DatabasePersister() =
    

    let persistTables connection databaseTables fileSystemTables =
        let newTables = Helpers.getNewTables databaseTables fileSystemTables
        newTables |> (TableWriter.createTables connection >> (ConstraintPersister.createConstraintsForTables connection)) |> ignore
        let changedTables = Helpers.getNewlyAddedColumns databaseTables fileSystemTables
        changedTables |> List.iter (fun (table, addedCols) -> TableWriter.addColumns connection table.schema table.name addedCols)
        
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
            
