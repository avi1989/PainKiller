module PainKiller.ConsoleApp.DatabaseWriter.TableWriter

open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.DatabaseWriter.Dto

let writeToFileSystem engine filePath (tables: Models.TableInfo list) =
    tables 
        |> List.map (Dto.TableInfo.FromDomain engine)
        |> List.iter (fun x -> XmlWriter.writeXml filePath "tables" x.name x)
