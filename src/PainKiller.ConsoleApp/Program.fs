// Learn more about F# at http://fsharp.org

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Contracts
open PainKiller.ConsoleApp.FileSystem

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let basePath = "c:\\Temp\\dbGen"
    let connString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=coverme_encounters;";
    let databaseRetriever = new DatabaseRetriever() :> IDatabaseRetriever
    let database = databaseRetriever.GetDatabase connString
    
    let result = PainKiller.ConsoleApp.FileSystem.Reader.readFromFileSystem basePath "postgres"
    //database |> Writer.writeToFileSystem basePath "engine"
    0
