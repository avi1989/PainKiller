// Learn more about F# at http://fsharp.org

open System
open PainKiller.ConsoleApp

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    let result = TableRetriever.loadTables "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=coverme_encounters;"
    DatabaseWriter.writeToFileSystem "c:\\Temp\\dbGen" result |> ignore
    0 // return an integer exit code
