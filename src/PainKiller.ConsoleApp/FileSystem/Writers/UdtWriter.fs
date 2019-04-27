module PainKiller.ConsoleApp.FileSystem.Writers.UdtWriter

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.FileSystem

let writeToFileSystem filePath (tables: Models.UdtInfo list) =
    tables 
        |> List.map Dto.UserDefinedType.FromDomain 
        |> List.iter (fun x -> XmlWriter.writeXml filePath "userDefinedTypes" x.name x)
