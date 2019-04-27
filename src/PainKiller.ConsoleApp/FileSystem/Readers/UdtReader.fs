module PainKiller.ConsoleApp.FileSystem.Readers.UdtReader

open System.IO
open System.Xml.Serialization
open PainKiller.ConsoleApp.FileSystem

let private readUdt engine filePath =
    let serializer = XmlSerializer(typeof<Dto.UserDefinedType>)
    use fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)
    let data = serializer.Deserialize(fileStream) :?> Dto.UserDefinedType
                |> Dto.UserDefinedType.ToDomain
    data

let readUdts engine basePath =
    let filePath = sprintf "%s/%s" basePath "userDefinedTypes"
    System.IO.Directory.EnumerateFiles(filePath, "*.xml") 
        |> List.ofSeq
        |> List.map (readUdt engine)
                
