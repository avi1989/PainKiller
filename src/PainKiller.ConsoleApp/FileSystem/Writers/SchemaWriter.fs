module PainKiller.ConsoleApp.FileSystem.Writers.SchemaWriter

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.FileSystem

let writeToFileSystem filePath (schemas: string list) =
    schemas 
        |> List.map (fun x -> { Dto.Schema.name = x })
        |> List.iter (fun x -> XmlWriter.writeXml filePath "schemas" x.name x)