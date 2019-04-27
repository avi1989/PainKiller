module PainKiller.ConsoleApp.FileSystem.Dto

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

    static member ToDomain (item: DefaultValue option) =
        match item with
        | Some o -> Some o.query
        | None -> None

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

    static member ToDomain index engine (item: ColumnInfo) =
        let defaultValue = item.defaults
                            |> List.ofSeq
                            |> List.filter (fun x -> x.engine = engine)
                            |> List.tryHead
                            |> DefaultValue.ToDomain

        { Models.Column.name = item.name
          Models.Column.defaultValue = defaultValue 
          Models.Column.position = index 
          Models.Column.``type`` = item.``type`` 
          Models.Column.isNullable = item.isNullable }
    

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
          
    static member ToDomain engine (item: TableInfo) =
        { Models.TableInfo.name = item.name
          Models.TableInfo.schema = item.schema 
          Models.TableInfo.columns = item.columns
                                        |> List.ofSeq
                                        |> List.mapi (fun i x -> ColumnInfo.ToDomain i engine x) }

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

    static member ToDomain index (item: Attribute) =
        { Models.UdtAttributes.name = item.name
          Models.UdtAttributes.isNullable = item.isNullable
          Models.UdtAttributes.position = index
          Models.UdtAttributes.``type`` = item.``type`` }

[<CLIMutable>]
[<XmlRoot("type")>]
[<XmlType("type")>]
type UserDefinedType = {
    [<XmlAttribute>] 
    name: string

    [<XmlAttribute>] 
    schema: string

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

    static member ToDomain (item: UserDefinedType) =
        { Models.UdtInfo.name = item.name
          Models.UdtInfo.schema = item.schema
          Models.UdtInfo.attributes = item.attributes
                               |> List.ofSeq
                               |> List.mapi (fun i x -> Attribute.ToDomain i x)
        }