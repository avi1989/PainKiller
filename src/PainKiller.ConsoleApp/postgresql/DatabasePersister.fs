namespace PainKiller.ConsoleApp.PostgreSQL

open PainKiller.ConsoleApp.Contracts
open Npgsql
open PainKiller.ConsoleApp.PostgreSQL.Persisters

type DatabasePersister() =
    interface IDatabasePersister with
        member this.PersistDatabase connectionString database =
            let connectionFactory = fun () -> new NpgsqlConnection(connectionString)
            use connection = connectionFactory()
            database.schemas |> SchemaWriter.createSchemas connection
            database.tables |> TableWriter.createTables connection
            database.userDefinedTypes |> UdtWriter.createUdts connection
            database.views |> SimpleScriptWriter.writeSimpleScripts connection
            database.functions |> SimpleScriptWriter.writeSimpleScripts connection
            database.procedures |> SimpleScriptWriter.writeSimpleScripts connection
            database.tables |> ConstraintPersister.createConstraintsForTables connection



