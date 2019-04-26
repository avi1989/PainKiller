module PainKiller.ConsoleApp.DatabaseWriter.UdtWriter

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.DatabaseWriter.Dto

let writeToFileSystem filePath (tables: Models.UdtInfo list) =
    tables 
        |> List.map Dto.UserDefinedType.FromDomain 
        |> List.iter (fun x -> XmlWriter.writeXml filePath "userDefinedTypes" x.name x)
