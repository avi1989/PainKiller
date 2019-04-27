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
    let databaseRetriever = new DatabaseRetriever() :> IDatabaseRetriever
    let database = databaseRetriever.GetDatabase connString
    //database |> Writer.writeToFileSystem basePath "engine"

    //PENDING
    // TABLE DEFAULTS (SHould be easy)
    // Custom Constraints
    // Extensions
    // Sequences
    // Enums
    // Indexes
    // Preprocessing
    // Postprocesing

    let result = PainKiller.ConsoleApp.FileSystem.Reader.readFromFileSystem basePath "postgres"
    let databasePersister = new DatabasePersister() :> IDatabasePersister
    result |> databasePersister.PersistDatabase connString database
    0
