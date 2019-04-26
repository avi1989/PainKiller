module PainKiller.ConsoleApp.DatabaseWriter.TableWriter

open System.Xml.Serialization
open System.IO
open PainKiller.ConsoleApp.PostgreSQL

[<CLIMutable>]
[<XmlRoot("default")>]
[<XmlType("default")>]
type DefaultValue = {
    [<XmlAttribute>] 
    engine: string
    query: string
}

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
}

[<CLIMutable>]
[<XmlRoot("table")>]
[<XmlType("table")>]
type TableInfo = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    schema: string;
    [<XmlArray>]
    columns: System.Collections.Generic.List<ColumnInfo>;
}

let private convertDomainColumnToDto engine (item: TableRetriever.Column) = 
    {
        name = item.name
        ``type`` = item.``type``
        defaults = match item.defaultValue with
                       | Some a -> [{engine = engine; query = a }] |> List.toArray |> System.Collections.Generic.List<DefaultValue>
                       | None -> null
        isNullable = item.isNullable
    }

let private convertDomainTableToDto engine (item: TableRetriever.TableInfo) =
    { name = item.name
      schema = item.schema
      columns = item.columns 
                |> List.sortBy (fun x -> x.position) 
                |> List.map (convertDomainColumnToDto engine)
                |> List.toArray 
                |> System.Collections.Generic.List<ColumnInfo>
    }

let writeToFileSystem engine filePath (tables: TableRetriever.TableInfo list) =
    tables 
        |> List.map (convertDomainTableToDto engine)
        |> List.iter (fun x -> XmlWriter.writeXml filePath "tables" x.name x)
