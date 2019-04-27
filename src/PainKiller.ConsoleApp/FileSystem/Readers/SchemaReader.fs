module PainKiller.ConsoleApp.FileSystem.Readers.SchemaReader

open System.IO
open System.Xml.Serialization
open PainKiller.ConsoleApp.FileSystem

let private readSchema filePath =
    let serializer = XmlSerializer(typeof<Dto.Schema>)
    use fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)
    let data = serializer.Deserialize(fileStream) :?> Dto.Schema
                |> Dto.Schema.ToDomain
    data

let readSchemas basePath =
    let filePath = sprintf "%s/%s" basePath "schemas"
    System.IO.Directory.EnumerateFiles(filePath, "*.xml") 
        |> List.ofSeq
        |> List.map (readSchema)
                
