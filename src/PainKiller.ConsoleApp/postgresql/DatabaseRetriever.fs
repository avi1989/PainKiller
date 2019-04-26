namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql

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
            }



