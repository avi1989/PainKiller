module PainKiller.ConsoleApp.DatabaseWriter.ProcedureWriter

open PainKiller.ConsoleApp.PostgreSQL
open System.IO

let private writeFunction basePath (func: ProcedureRetriever.Procedure)  =
    let filePath = sprintf "%s/%s.sql" basePath func.name
    File.WriteAllText(filePath, func.definition)
    
let writeToFileSystem filePath (functions: ProcedureRetriever.Procedure list) =
    let basePath = sprintf "%s/procedures" filePath
    Directory.CreateDirectory(basePath) |> ignore
    functions |> List.iter (writeFunction basePath)