namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.Adapters.PostgreSQL
open PainKiller.Abstractions.Contracts

type DatabaseRetriever() =
    interface IDatabaseRetriever with
        member this.GetDatabase connectionString =
            let actualRetriever = DatabaseReader() :> IDatabaseReader
            { tables = actualRetriever.GetTables connectionString
              functions = actualRetriever.GetFunctions connectionString
              views = actualRetriever.GetViews connectionString
              procedures = actualRetriever.GetProcedures connectionString
              userDefinedTypes = actualRetriever.GetUserDefinedTypes connectionString
              schemas = actualRetriever.GetSchemas connectionString
              sequences = actualRetriever.GetSequences connectionString
              indexes = actualRetriever.GetIndexes connectionString }



