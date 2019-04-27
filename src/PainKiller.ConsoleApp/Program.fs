// Learn more about F# at http://fsharp.org

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Contracts
open PainKiller.ConsoleApp.FileSystem
open Npgsql

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let basePath = "c:\\Temp\\dbGen"
    let connString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=hello_rh2;";
    //let connString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=coverme_encounters;";
    use connection = new NpgsqlConnection(connString)
    //let databaseRetriever = new DatabaseRetriever() :> IDatabaseRetriever
    //let database = databaseRetriever.GetDatabase connString
    //database |> Writer.writeToFileSystem basePath "engine"

    let result = PainKiller.ConsoleApp.FileSystem.Reader.readFromFileSystem basePath "postgres"
    PainKiller.ConsoleApp.PostgreSQL.Persister.SchemaWriter.createSchemas connection result.schemas
    PainKiller.ConsoleApp.PostgreSQL.Persister.TableWriter.createTables connection result.tables
    PainKiller.ConsoleApp.PostgreSQL.Persister.UdtWriter.createUdts connection result.userDefinedTypes
    PainKiller.ConsoleApp.PostgreSQL.Persister.SimpleScriptWriter.writeSimpleScripts connection result.views
    PainKiller.ConsoleApp.PostgreSQL.Persister.SimpleScriptWriter.writeSimpleScripts connection result.functions
    PainKiller.ConsoleApp.PostgreSQL.Persister.SimpleScriptWriter.writeSimpleScripts connection result.procedures
    PainKiller.ConsoleApp.PostgreSQL.Persister.ConstraintPersister.createConstraintsForTables connection result.tables |> ignore

    0
