module PainKiller.ConsoleApp.FileSystem.Readers.TableReader

open System.IO
open System.Xml.Serialization
open PainKiller.ConsoleApp.FileSystem

let private readTable engine filePath =
    let serializer = XmlSerializer(typeof<Dto.TableInfo>)
    use fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)
    let data = serializer.Deserialize(fileStream) :?> Dto.TableInfo
                |> Dto.TableInfo.ToDomain engine
    data

let readTables engine basePath =
    let filePath = sprintf "%s/%s" basePath "tables"
    System.IO.Directory.EnumerateFiles(filePath, "*.xml") 
        |> List.ofSeq
        |> List.map (readTable engine)
                
