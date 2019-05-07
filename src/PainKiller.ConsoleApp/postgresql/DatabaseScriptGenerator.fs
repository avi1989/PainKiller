module PainKiller.ConsoleApp.postgresql.DatabaseScriptGenerator
open System
open PainKiller.Abstractions.Contracts
open PainKiller.Abstractions.Models
open PainKiller.ConsoleApp

let private getNewUdts (udtInFs: UdtInfo list) (udtInDb: UdtInfo list) =
    udtInFs
        |> List.filter (fun x -> not (udtInDb |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))
        
let getNewTables (tablesInFs: TableInfo list) (tablesInDb: TableInfo list) =
    tablesInFs
        |> List.filter ( fun x -> not (tablesInDb |> List.exists (fun y -> x.schema = y.schema && x.name = y.name)))
        
let getNewlyAddedColumns (tablesInFs: TableInfo list) (tablesInDb: TableInfo list) =
        let pairedTables = ListHelpers.pairListsWithoutUnpairedItems (fun x y -> x.name = y.name && x.schema = y.schema) tablesInFs tablesInDb
        pairedTables
            |> List.map (fun (tableInFs, tableInDb) -> 
                let changedCols = tableInFs.columns|> List.filter (fun x -> not (tableInDb.columns |> List.exists(fun y -> x.name = y.name)))
                (tableInFs, changedCols))
            |> List.filter(fun (_, x) -> not (List.isEmpty x))

let getRemovedColumns tablesInFs tablesInDb =
    getNewlyAddedColumns tablesInDb tablesInFs
    
let compareScripts script1 script2 =
    let stripWhiteSpaceAndUppercase (str: string) = str |> String.filter (fun x -> x <> ' ') |> String.map Char.ToUpper
    let strippedScript1 = script1 |> stripWhiteSpaceAndUppercase
    let strippedScript2 = script2 |> stripWhiteSpaceAndUppercase
    strippedScript1 = strippedScript2
    
let generateDatabaseScript  (generator: IDdlStatementGenerator)
                            (dbFromFs : Database)
                            (currentDatabase: Database) =
    
    ""