module PainKiller.ConsoleApp.DatabaseWriter.Dto

open System.Xml.Serialization
open PainKiller.ConsoleApp

[<CLIMutable>]
[<XmlRoot("default")>]
[<XmlType("default")>]
type DefaultValue = {
    [<XmlAttribute>] 
    engine: string
    query: string
} with
    static member FromDomain engine item =
        match item with
        | Some a -> [{engine = engine; query = a }] |> List.toArray |> System.Collections.Generic.List<DefaultValue>
        | None -> null 

[<CLIMutable>]
[<XmlRoot("column")>]
[<XmlType("column")>]
type ColumnInfo = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    ``type``: string;
    defaults: System.Collections.Generic.List<DefaultValue>;
    [<XmlAttribute>] 
    isNullable: bool 
} with 
    static member FromDomain engine (item: Models.Column) =
        { name = item.name
          ``type`` = item.``type``
          defaults = DefaultValue.FromDomain engine item.defaultValue
          isNullable = item.isNullable }
    

[<CLIMutable>]
[<XmlRoot("table")>]
[<XmlType("table")>]
type TableInfo = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    schema: string;
    [<XmlArray>]
    columns: System.Collections.Generic.List<ColumnInfo>
} with 
    static member FromDomain engine (item: Models.TableInfo) =
        { name = item.name
          schema = item.schema
          columns = item.columns 
                    |> List.sortBy (fun x -> x.position) 
                    |> List.map (ColumnInfo.FromDomain engine)
                    |> List.toArray 
                    |> System.Collections.Generic.List<ColumnInfo> }

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
} with
    static member FromDomain (item: Models.UdtAttributes) =
        { name = item.name
          ``type`` = item.``type``
          isNullable = item.isNullable }

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
} with
    static member FromDomain (item: Models.UdtInfo) =
        { name = item.name
          schema = item.schema
          attributes = item.attributes 
                  |> List.sortBy (fun x -> x.position) 
                  |> List.map Attribute.FromDomain 
                  |> List.toArray 
                  |> System.Collections.Generic.List<Attribute> }