module PainKiller.ConsoleApp.PostgreSQL.Persisters.TableWriter

open Npgsql
open System.Data
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.Abstractions.Models
open System.Text

let private getColString (col: Column) =
    let dbColType = ColumnTypeMapper.mapDomainToDatabase col.``type``
    let defaultVal = match col.defaultValue with
                     | Some value -> sprintf "DEFAULT %s" value
                     | None -> ""
    let nullString = match col.isNullable with
                     | true -> ""
                     | false -> "NOT NULL"

    sprintf "\"%s\" %s %s %s" col.name dbColType nullString defaultVal

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
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s ADD COLUMN \"%s\"" schema table
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

let dropColumns (sqlConnection: NpgsqlConnection) schema table columns =
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s DROP COLUMN \"%s\"" schema table
    let setConn ()= 
        if sqlConnection.State <> ConnectionState.Open
        then sqlConnection.Open() |> ignore

    let executeQuery query =
        use command = sqlConnection.CreateCommand()
        command.CommandText <- query
        command.ExecuteNonQuery() |> ignore
    columns
        |> List.iter(fun (col: Column) -> 
                        let query = buildQueryForTable col.name
                        setConn()
                        executeQuery query )

let alterColumnTypes (sqlConnection: NpgsqlConnection) schema table columns =
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s ALTER COLUMN \"%s\" SET DATA TYPE %s" schema table
    let setConn ()= 
        if sqlConnection.State <> ConnectionState.Open
        then sqlConnection.Open() |> ignore

    let executeQuery query =
        use command = sqlConnection.CreateCommand()
        command.CommandText <- query
        command.ExecuteNonQuery() |> ignore
    columns
        |> List.iter(fun (col: Column) -> 
                        let query = buildQueryForTable col.name (ColumnTypeMapper.mapDomainToDatabase col.``type``)
                        setConn()
                        executeQuery query )


let alterColumnDefaults (sqlConnection: NpgsqlConnection) schema table columns =
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s ALTER COLUMN \"%s\" %s" schema table
    let setConn ()= 
        if sqlConnection.State <> ConnectionState.Open
        then sqlConnection.Open() |> ignore

    let executeQuery query =
        use command = sqlConnection.CreateCommand()
        command.CommandText <- query
        command.ExecuteNonQuery() |> ignore
    columns
        |> List.iter(fun (col: Column) -> 
                        setConn()
                        let query = match col.defaultValue with
                                    | Some s -> buildQueryForTable col.name (sprintf "SET DEFAULT %s" s)
                                    | None -> buildQueryForTable col.name "DROP DEFAULT"
                        executeQuery query )

let alterColumnNulable (sqlConnection: NpgsqlConnection) schema table columns =
    let buildQueryForTable = sprintf "ALTER TABLE %s.%s ALTER COLUMN \"%s\" %s NOT NULL" schema table
    let setConn ()= 
        if sqlConnection.State <> ConnectionState.Open
        then sqlConnection.Open() |> ignore

    let executeQuery query =
        use command = sqlConnection.CreateCommand()
        command.CommandText <- query
        command.ExecuteNonQuery() |> ignore
    columns
        |> List.iter(fun (col: Column) -> 
                        setConn()
                        let query = match col.isNullable with
                                    | true -> buildQueryForTable col.name "SET"
                                    | false -> buildQueryForTable col.name "DROP"
                        executeQuery query )