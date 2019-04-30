module PainKiller.ConsoleApp.FileSystem.Writers.SimpleScriptWriter

open System.IO
open PainKiller.Abstractions

let private writeItem basePath (func: Models.SimpleDatabaseItem)  =
    let filePath = sprintf "%s/%s.%s.sql" basePath func.schema func.name
    File.WriteAllText(filePath, func.definition)
    
let writeToFileSystem filePath itemType (functions: Models.SimpleDatabaseItem list) =
    let basePath = sprintf "%s/%s" filePath itemType
    Directory.CreateDirectory(basePath) |> ignore
    functions |> List.iter (writeItem basePath)