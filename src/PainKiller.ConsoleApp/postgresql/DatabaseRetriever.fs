namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.ConsoleApp.PostgreSQL.Retrievers

type DatabaseRetriever() =
    interface IDatabaseRetriever with
        member this.GetDatabase connectionString =
            let connectionFactory = fun () -> new NpgsqlConnection(connectionString)
            use connection = connectionFactory()
            {
                tables = TableRetriever.loadTables connection connectionFactory
                functions = FunctionRetriever.loadFunctions connection
                views = ViewRetriever.loadViews connection
                procedures = ProcedureRetriever.loadProcedures connection
                userDefinedTypes = UdtRetriever.loadUserDefinedTypes connection connectionFactory
                schemas = SchemaRetriever.getSchemas connection
                sequences = SequenceRetriever.getSequences connection
            }



