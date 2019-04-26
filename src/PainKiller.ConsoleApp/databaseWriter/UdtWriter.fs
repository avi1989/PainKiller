module PainKiller.ConsoleApp.DatabaseWriter.UdtWriter

open System.Xml.Serialization
open System.IO
open PainKiller.ConsoleApp.PostgreSQL

[<CLIMutable>]
[<XmlRoot("column")>]
type Attribute = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    ``type``: string;
    [<XmlAttribute>] 
    isNullable: bool 
}

[<CLIMutable>]
[<XmlRoot("tables")>]
type UserDefinedType = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    schema: string;
    [<XmlArray>]
    columns: System.Collections.Generic.List<Attribute>;
}

let private writeTableToDisk filePath (table: UserDefinedType) =
    let baseDir = sprintf "%s/userDefinedTypes" filePath
    baseDir |> Directory.CreateDirectory |> ignore
    let serializer = new XmlSerializer(typeof<UserDefinedType>)
    let filePath = sprintf "%s/%s.xml" baseDir table.name
    use fileStream = new FileStream((filePath), FileMode.Create, FileAccess.Write)
    serializer.Serialize(fileStream, table) |> ignore

let private convertDomainColumnToDto (item: UdtRetriever.UdtAttributes) = 
    {
        name = item.name
        ``type`` = item.``type``
        isNullable = item.isNullable
    }

let private convertDomainTableToDto (item: UdtRetriever.UdtInfo) =
    { name = item.name
      schema = item.schema
      columns = item.attributes 
                |> List.sortBy (fun x -> x.position) 
                |> List.map convertDomainColumnToDto 
                |> List.toArray 
                |> System.Collections.Generic.List<Attribute>
    }

let writeToFileSystem filePath (tables: UdtRetriever.UdtInfo list) =
    tables 
        |> List.map convertDomainTableToDto 
        |> List.iter (writeTableToDisk filePath)
