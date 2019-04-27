module PainKiller.ConsoleApp.PostgreSQL.Retrievers.ColumnTypeMapper

open PainKiller.ConsoleApp.Models
open System

let typeMap = 
    Map.empty
    |> Map.add "timestamp without time zone" "timestampz"
    |> Map.add "timestamp with time zone" "timestamp"
    |> Map.add "uuid" "guid"
    |> Map.add "character varying" "varchar"

let mapColumns columnType =
    let mapping = typeMap |> Map.tryFind columnType
    match mapping with
    | Some mapping -> mapping
    | None -> columnType

let mapColumnType dataType charMaxLength =
    let newColType = mapColumns dataType

    match charMaxLength with
    | None -> TypeWithoutLength newColType
    | Some a -> TypeWithLength (newColType, a)