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

    database.tables |> FileSystem.Writer.TableWriter.writeToFileSystem "postgres" basePath |> ignore
    database.functions |> FileSystem.Writer.SimpleScriptWriter.writeToFileSystem basePath "functions" |> ignore
    database.views |> FileSystem.Writer.SimpleScriptWriter.writeToFileSystem basePath "views" |> ignore
    database.procedures |> FileSystem.Writer.SimpleScriptWriter.writeToFileSystem basePath "procedures" |> ignore
    database.userDefinedTypes |> FileSystem.Writer.UdtWriter.writeToFileSystem basePath |> ignore
    0
