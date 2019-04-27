namespace PainKiller.ConsoleApp.Models

type ColumnType =
    | TypeWithoutLength of string
    | TypeWithLength of string * int

type TableConstraintType =
    | PrimaryKey of string list
    | Unique of string list
    | Check of string

type TableConstraint = {
    name: string
    ``type``: TableConstraintType
}

type ReferentialConstraint = {
    name: string
}

type UdtAttributes = {
    name: string;
    position: int;
    ``type``: ColumnType;
    isNullable: bool;
}

type UdtInfo = {
    name: string;
    schema: string;
    attributes: UdtAttributes list;
}

type SimpleDatabaseItem = {
    name: string;
    schema: string;
    definition: string;
}

type Column = {
    name: string;
    position: int;
    ``type``: ColumnType;
    defaultValue: string option;
    isNullable: bool;
}

type TableInfo = {
    name: string;
    schema: string;
    columns: Column list;
    constraints: TableConstraint list;
}

type Database = {
    tables: TableInfo list
    functions: SimpleDatabaseItem list
    views: SimpleDatabaseItem list
    procedures: SimpleDatabaseItem list
    userDefinedTypes: UdtInfo list
}