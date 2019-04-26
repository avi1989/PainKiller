module PainKiller.ConsoleApp.DatabaseWriter

open System.Runtime.Serialization
open System.Xml.Serialization
open System.IO

[<CLIMutable>]
[<XmlRoot("column")>]
type ColumnInfo = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    ``type``: string;
    defaultValue: string;
    [<XmlAttribute>] 
    isNullable: bool 
}

[<CLIMutable>]
[<XmlRoot("tables")>]
type TableInfo = {
    [<XmlAttribute>] 
    name: string;
    [<XmlAttribute>] 
    schema: string;
    [<XmlArray>]
    columns: System.Collections.Generic.List<ColumnInfo>;
}

let private writeTableToDisk filePath (table: TableInfo) =
    let baseDir = sprintf "%s/tables" filePath
    baseDir |> Directory.CreateDirectory |> ignore
    let serializer = new XmlSerializer(typeof<TableInfo>)
    let filePath = sprintf "%s/%s.xml" baseDir table.name
    use fileStream = new FileStream((filePath), FileMode.Create, FileAccess.Write)
    serializer.Serialize(fileStream, table) |> ignore

let private convertDomainColumnToDto (item: TableRetriever.Column) = 
    {
        name = item.name
        ``type`` = item.``type``
        defaultValue = match item.defaultValue with
                       | Some a -> a
                       | None -> null
        isNullable = item.isNullable
    }

let private convertDomainTableToDto (item: TableRetriever.TableInfo) =
    { name = item.name
      schema = item.schema
      columns = item.columns 
                |> List.sortBy (fun x -> x.position) 
                |> List.map convertDomainColumnToDto 
                |> List.toArray 
                |> System.Collections.Generic.List<ColumnInfo>
    }

let writeToFileSystem filePath (tables: TableRetriever.TableInfo list) =
    tables 
        |> List.map convertDomainTableToDto 
        |> List.iter (writeTableToDisk filePath)
    0
