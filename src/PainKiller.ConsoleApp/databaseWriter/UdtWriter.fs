module PainKiller.ConsoleApp.DatabaseWriter.UdtWriter

open System.Xml.Serialization
open PainKiller.ConsoleApp

[<CLIMutable>]
[<XmlRoot("attribute")>]
[<XmlType("attribute")>]
type Attribute = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    ``type``: string;
    [<XmlAttribute>] 
    isNullable: bool 
}

[<CLIMutable>]
[<XmlRoot("type")>]
[<XmlType("type")>]
type UserDefinedType = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    schema: string;
    [<XmlArray>]
    attributes: System.Collections.Generic.List<Attribute>;
}

let private convertDomainColumnToDto (item: Models.UdtAttributes) = 
    {
        name = item.name
        ``type`` = item.``type``
        isNullable = item.isNullable
    }

let private convertDomainTableToDto (item: Models.UdtInfo) =
    { name = item.name
      schema = item.schema
      attributes = item.attributes 
                |> List.sortBy (fun x -> x.position) 
                |> List.map convertDomainColumnToDto 
                |> List.toArray 
                |> System.Collections.Generic.List<Attribute>
    }

let writeToFileSystem filePath (tables: Models.UdtInfo list) =
    tables 
        |> List.map convertDomainTableToDto 
        |> List.iter (fun x -> XmlWriter.writeXml filePath "userDefinedTypes" x.name x)
