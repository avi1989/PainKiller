module PainKiller.ConsoleApp.DatabaseWriter.ViewWriter

open PainKiller.ConsoleApp.PostgreSQL
open System.IO

let private writeView basePath (func: ViewRetriever.View)  =
    let filePath = sprintf "%s/%s.sql" basePath func.name
    File.WriteAllText(filePath, func.definition)
    
let writeToFileSystem filePath (functions: ViewRetriever.View list) =
    let basePath = sprintf "%s/views" filePath
    Directory.CreateDirectory(basePath) |> ignore
    functions |> List.iter (writeView basePath)