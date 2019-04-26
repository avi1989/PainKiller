// Learn more about F# at http://fsharp.org

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.PostgreSQL
open PainKiller.ConsoleApp.Contracts

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let basePath = "c:\\Temp\\dbGen"
    let connString = "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=coverme_encounters;";
    let databaseRetriever = new DatabaseRetriever() :> IDatabaseRetriever
    let database = databaseRetriever.GetDatabase connString

    database.tables |> DatabaseWriter.TableWriter.writeToFileSystem "postgres" basePath |> ignore
    database.functions |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "functions" |> ignore
    database.views |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "views" |> ignore
    database.procedures |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "procedures" |> ignore
    database.userDefinedTypes |> DatabaseWriter.UdtWriter.writeToFileSystem basePath |> ignore
    0
