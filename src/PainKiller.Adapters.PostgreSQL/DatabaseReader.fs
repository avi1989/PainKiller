namespace PainKiller.Adapters.PostgreSQL
open PainKiller.Abstractions.Contracts
open PainKiller.Adapters.PostgreSQL.Retrievers
open Npgsql
open System.Data

type DatabaseReader() =
    let createConnection connectionString = 
        new NpgsqlConnection(connectionString) :> IDbConnection

    interface IDatabaseReader with
        member this.GetFunctions connectionString =
            SimpleScriptRetriever.retrieveSimpleScript (createConnection connectionString) DatabaseRetrieverQueries.FunctionRetrieverQueries.getFunctionQuery

        member this.GetIndexes connectionString =
            SimpleScriptRetriever.retrieveSimpleScript (createConnection connectionString) DatabaseRetrieverQueries.IndexQueries.getIndexQuery

        member this.GetProcedures connectionString =
            SimpleScriptRetriever.retrieveSimpleScript (createConnection connectionString) DatabaseRetrieverQueries.ProcedureRetrieverQueries.getProcedureQueries

        member this.GetSchemas connectionString =
            SchemaRetriever.getSchemas (createConnection connectionString)

        member this.GetSequences connectionString =
            SequenceRetriever.getSequences (createConnection connectionString)

        member this.GetUserDefinedTypes connectionString =
            UdtRetriever.loadUserDefinedTypes (createConnection connectionString) (fun () -> (createConnection connectionString))

        member this.GetViews connectionString =
            SimpleScriptRetriever.retrieveSimpleScript (createConnection connectionString) DatabaseRetrieverQueries.ViewRetrieverQueries.getViewQuery

        member this.GetTables connectionString =
            TableRetriever.loadTable (createConnection connectionString) (fun () -> (createConnection connectionString))

