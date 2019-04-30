module PainKiller.Adapters.PostgreSQL.ColumnTypeMapper

open PainKiller.Abstractions.Models
open System

let typeMap = 
    Map.empty
    |> Map.add "timestamp without time zone" "timestampz"
    |> Map.add "timestamp with time zone" "timestamp"
    |> Map.add "uuid" "guid"
    |> Map.add "character varying" "varchar"

let private mapColumns columnType =
    let mapping = typeMap |> Map.tryFind columnType
    match mapping with
    | Some mapping -> mapping
    | None -> columnType

let private reverseMapColumn columnType =
    let mapping = typeMap |> Map.tryFindKey (fun k v -> v = columnType)
    match mapping with
    | Some key -> key
    | None -> columnType

let mapDatabaseToDomain dataType charMaxLength =
    let newColType = mapColumns dataType
    match charMaxLength with
    | None -> TypeWithoutLength newColType
    | Some a -> TypeWithLength (newColType, a)

let mapDomainToDatabase domainType =
    match domainType with
    | TypeWithoutLength s -> reverseMapColumn s
    | TypeWithLength (s, len) -> sprintf "%s(%i)" (reverseMapColumn s) len