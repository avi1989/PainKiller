﻿module PainKiller.ConsoleApp.PostgreSQL.ColumnTypeMapper
open System

type ColumnType =
    | TypeWithoutLength of string
    | TypeWithLength of string * int

let mapColumnsWithoutLength column =
    match column with
    | "text" | "integer" | "bit" | "boolean" | "date" | "json" | "jsonb" | "smallint" | "bigint" -> column
    | "timestamp without time zone" -> "timestamptz"
    | "timestamp" | "timestamp with time zone" -> "timestamp"
    | _ -> column

let mapColumnWithLength col len =
    match col with
    | "character varying" -> sprintf "varchar(%i)" len
    | "char" -> sprintf "char(%i)" len
    | _ -> raise (Exception "Unknown Type")

let mapColumnType col =
    match col with
    | TypeWithoutLength a -> mapColumnsWithoutLength a 
    | TypeWithLength (name, len) -> mapColumnWithLength name len