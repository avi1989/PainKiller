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

    let getColumnsWhereIsNullableChanged =
        true


type DatabasePersister() =
    let alterTable connection (fsTable, dbTable) =
        let pairedColumnsBetweenFsAndDb = ListHelpers.pairLists (fun (x: Column) (y: Column) -> x.name = y.name) fsTable.columns dbTable.columns
        let pairedColumnsBetweenDbAndFs = ListHelpers.pairLists (fun (x: Column) (y: Column) -> x.name = y.name) dbTable.columns fsTable.columns
        let pairedColumnsBetweenFsAndDbWithoutNulls = pairedColumnsBetweenFsAndDb |> ListHelpers.removeUnpairedItems
        let columnsToBeAdded = pairedColumnsBetweenFsAndDb |> List.filter (fun (fs, db) -> db.IsNone) |> List.map (fun (fs, db) -> fs)
        let columnsToBeRemoved = pairedColumnsBetweenDbAndFs |> List.filter (fun (db, fs) -> fs.IsNone) |> List.map (fun (db, fs) -> db)
        let columnsWhereDataTypeChanged = pairedColumnsBetweenFsAndDbWithoutNulls 
                                            |> List.filter (fun (fs, db) -> fs.``type`` <> db.``type``)
                                            |> List.map (fun (fs, db) -> fs)
        let columnsWhereDefaultChanged = pairedColumnsBetweenFsAndDbWithoutNulls 
                                            |> List.filter (fun (fs, db) -> fs.defaultValue <> db.defaultValue)
                                            |> List.map (fun (fs, db) -> fs)


        columnsToBeAdded |> TableWriter.addColumns connection fsTable.schema fsTable.name
        columnsToBeRemoved |> TableWriter.dropColumns connection fsTable.schema fsTable.name
        columnsWhereDataTypeChanged |> TableWriter.alterColumnTypes connection fsTable.schema fsTable.name
        columnsWhereDefaultChanged |> TableWriter.alterColumnDefaults connection fsTable.schema fsTable.name
        "" |> ignore

    let alterTables connection (pairedTables: (TableInfo * TableInfo) list) = 
        pairedTables |> List.iter (alterTable connection)
        ""

    let persistTables connection databaseTables fileSystemTables =
        let newTables = Helpers.getNewTables databaseTables fileSystemTables
        let pairedTables = ListHelpers.pairListsWithoutUnpairedItems (fun a b -> a.name = b.name && a.schema = b.schema) fileSystemTables databaseTables
        newTables |> (TableWriter.createTables connection >> (ConstraintPersister.createConstraintsForTables connection)) |> ignore
        alterTables connection pairedTables |> ignore
        //let tablesWithAddedColumns = Helpers.getNewlyAddedColumns databaseTables fileSystemTables
        //let tablesWithRemovedColumns = Helpers.getRemovedColumns databaseTables fileSystemTables

        //tablesWithAddedColumns |> List.iter (fun (table, addedCols) -> TableWriter.addColumns connection table.schema table.name addedCols)
        //tablesWithRemovedColumns |> List.iter (fun (table, removedCols) -> TableWriter.dropColumns connection table.schema table.name removedCols)
        "" |> ignore

    interface IDatabasePersister with
        member _this.PersistDatabase connectionString currentStateOfDatabase databaseFromFileSystem =
            let connectionFactory = fun () -> new NpgsqlConnection(connectionString)
            use connection = connectionFactory()
            let newUdts = Helpers.getNewUdts currentStateOfDatabase.userDefinedTypes databaseFromFileSystem.userDefinedTypes
            databaseFromFileSystem.schemas |> SchemaWriter.createSchemas connection
            databaseFromFileSystem.sequences |> SimpleScriptWriter.writeSimpleScripts connection
            persistTables connection currentStateOfDatabase.tables databaseFromFileSystem.tables

            newUdts |> UdtWriter.createUdts connection
            databaseFromFileSystem.views |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.functions |> SimpleScriptWriter.writeSimpleScripts connection
            databaseFromFileSystem.procedures |> SimpleScriptWriter.writeSimpleScripts connection
            
