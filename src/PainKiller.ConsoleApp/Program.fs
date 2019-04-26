// Learn more about F# at http://fsharp.org

open System
open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.PostgreSQL

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let basePath = "c:\\Temp\\dbGen"
    let connString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=coverme_encounters;";
    let result = connString |> TableRetriever.loadTables
    let functions = connString |> FunctionRetriever.loadFunctions
    DatabaseWriter.TableWriter.writeToFileSystem basePath result |> ignore
    DatabaseWriter.FunctionWriter.writeToFileSystem basePath functions |> ignore
    0
