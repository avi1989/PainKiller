module PainKiller.ConsoleApp.PostgreSQL.Persister.UdtWriter

open Npgsql
open System.Data
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Models
open System.Text

let private getColString (col: UdtAttributes) =
    let dbColType = ColumnTypeMapper.mapDomainToDatabase col.``type``

    sprintf "\"%s\" %s" col.name dbColType

let private generateCreateStatement (udt: UdtInfo) =
    let columnInserts = udt.attributes
                        |> List.map getColString
                        |> String.concat ",\n"
                        //|> List.fold (fun acc i -> sprintf "%s\n%s," acc (getColString i)) ""
    sprintf "CREATE TYPE %s.%s AS (\n%s\n)" udt.schema udt.name columnInserts

let createUdt (sqlConnection: NpgsqlConnection) udt =
    let sqlStatement = udt |> generateCreateStatement
    if sqlConnection.State <> ConnectionState.Open
    then sqlConnection.Open() |> ignore

    use command = sqlConnection.CreateCommand()
    command.CommandText <- sqlStatement
    command.ExecuteNonQuery() |> ignore

let createUdts sqlConnection udts =
    udts
    |> List.iter (createUdt sqlConnection)