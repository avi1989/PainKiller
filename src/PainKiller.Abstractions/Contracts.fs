module PainKiller.Abstractions.Contracts
open System.Data
open PainKiller.Abstractions.Models

type IDatabaseReader =
    abstract member GetTables: string -> TableInfo list
    abstract member GetFunctions: string -> SimpleDatabaseItem list
    abstract member GetViews: string -> SimpleDatabaseItem list
    abstract member GetProcedures: string -> SimpleDatabaseItem list
    abstract member GetUserDefinedTypes: string -> UdtInfo list
    abstract member GetSchemas: string -> string list
    abstract member GetSequences: string -> SimpleDatabaseItem list
    abstract member GetIndexes: string -> SimpleDatabaseItem list

type IDdlStatementGenerator =
    abstract member GetCreateConstraintStatements: string -> string -> TableConstraint -> string
    abstract member GetCreateTableStatement: TableInfo -> string
    abstract member GetAddColumnStatement: Column -> string
    abstract member GetDropColumnStatement: Column -> string
    abstract member GetChangeColumnTypeStatement: Column -> string
    abstract member GetChangeColumnDefaultStatement: Column -> string
    abstract member GetChangeColumnNullableStatement: Column -> string
    abstract member GetCreateUdtStatements: UdtInfo -> string
