module PainKiller.ConsoleApp.PostgreSQL.Persisters.TableWriter

open Npgsql
open System.Data
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Models
open System.Text

let private getColString (col: Column) =
    let dbColType = ColumnTypeMapper.mapDomainToDatabase col.``type``
    let defaultVal = match col.defaultValue with
                     | Some value -> value
                     | None -> ""

    sprintf "\"%s\" %s %s" col.name dbColType defaultVal

let private generateCreateStatement (table: TableInfo) =
    let columnInserts = table.columns
                        |> List.map getColString
                        |> String.concat ",\n"
                        //|> List.fold (fun acc i -> sprintf "%s\n%s," acc (getColString i)) ""
    sprintf "CREATE TABLE %s.%s \n(\n%s\n)" table.schema table.name columnInserts

let createTable (sqlConnection: NpgsqlConnection) table =
    let sqlStatement = table |> generateCreateStatement
    if sqlConnection.State <> ConnectionState.Open
    then sqlConnection.Open() |> ignore

    use command = sqlConnection.CreateCommand()
    command.CommandText <- sqlStatement
    command.ExecuteNonQuery() |> ignore

let createTables sqlConnection tables =
    tables
        |> List.iter (createTable sqlConnection)
    tables

let addColumns (sqlConnection: NpgsqlConnection) schema table columns =
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s ADD COLUMN %s" schema table
    let setConn ()= 
        if sqlConnection.State <> ConnectionState.Open
        then sqlConnection.Open() |> ignore

    let executeQuery query =
        use command = sqlConnection.CreateCommand()
        command.CommandText <- query
        command.ExecuteNonQuery() |> ignore
    columns
        |> List.iter(fun col -> 
                        let query = buildQueryForTable (getColString col)
                        setConn()
                        executeQuery query )