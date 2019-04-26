module PainKiller.ConsoleApp.DatabaseWriter.FunctionWriter

open PainKiller.ConsoleApp.PostgreSQL
open System.IO

let private writeFunction basePath (func: FunctionRetriever.Function)  =
    let filePath = sprintf "%s/%s.sql" basePath func.name
    File.WriteAllText(filePath, func.definition)
    
let writeToFileSystem filePath (functions: FunctionRetriever.Function list) =
    let basePath = sprintf "%s/functions" filePath
    Directory.CreateDirectory(basePath) |> ignore
    functions |> List.iter (writeFunction basePath)