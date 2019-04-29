module PainKiller.Abstractions.Contracts
open System.Data
open PainKiller.Abstractions.Models

type IDatabaseReader =
    abstract member GetTables: IDbConnection -> TableInfo list
    abstract member GetFunctions: IDbConnection -> SimpleDatabaseItem list
    abstract member GetViews: IDbConnection -> SimpleDatabaseItem list
    abstract member GetProcedures: IDbConnection -> SimpleDatabaseItem list
    abstract member GetUserDefinedTypes: IDbConnection -> UdtInfo list
    abstract member GetSchemas: IDbConnection -> SimpleDatabaseItem list
    abstract member GetSequences: IDbConnection -> SimpleDatabaseItem list
    abstract member GetIndexes: IDbConnection -> SimpleDatabaseItem list

type IDdlStatementGenerator =
    abstract member GetCreateConstraintStatements: TableInfo -> string
    abstract member GetCreateForeignKeyStatements: TableInfo -> string
    abstract member GetCreateTableStatement: TableInfo -> string
    abstract member GetAddColumnsStatement: Column -> string
    abstract member GetDropColumnStatement: Column -> string
    abstract member GetChangeColumnTypeStatement: Column -> string
    abstract member GetChangeColumnDefaultStatement: Column -> string
    abstract member ChangeColumnNullableStatement: Column -> string
    abstract member CreateUdtStatements: UdtInfo -> unit
