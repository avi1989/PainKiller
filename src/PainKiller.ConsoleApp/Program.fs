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
    let views = connString |> ViewRetriever.loadViews
    let procedures = connString |> ProcedureRetriever.loadProcedures
    let userDefiniedTypes = connString |> UdtRetriever.loadUserDefinedTypes
    result |> DatabaseWriter.TableWriter.writeToFileSystem "postgres" basePath |> ignore
    functions |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "functions" |> ignore
    views |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "views" |> ignore
    procedures |> DatabaseWriter.SimpleScriptWriter.writeToFileSystem basePath "procedures" |> ignore
    userDefiniedTypes |> DatabaseWriter.UdtWriter.writeToFileSystem basePath |> ignore
    0
