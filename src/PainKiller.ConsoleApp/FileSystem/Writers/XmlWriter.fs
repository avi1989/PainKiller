module PainKiller.ConsoleApp.FileSystem.Writer.XmlWriter

open System.IO
open System.Xml.Serialization

let writeXml path itemType itemName (obj: 'A) =
    let basePath = sprintf "%s/%s" path itemType
    Directory.CreateDirectory path |> ignore
    Directory.CreateDirectory basePath |> ignore
    let serializer = XmlSerializer(typeof<'A>)
    let filePath = sprintf "%s/%s.xml" basePath itemName
    use fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)
    serializer.Serialize(fileStream, obj)
