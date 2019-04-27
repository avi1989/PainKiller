module PainKiller.ConsoleApp.FileSystem.Dto

open System.Xml.Serialization
open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.ActivePatterns

type Models.ColumnType with
    member this.FromDomain() =
        match this with
        | Models.TypeWithoutLength s -> s
        | Models.TypeWithLength (s, l) -> sprintf "%s(%i)" s l

    static member ToDomain (item: string) =
        match item with
        | RegexMatch "([A-Za-z]*)\((\d+)\)" [dType; tPrecision] -> Models.TypeWithLength (dType, (tPrecision |> int))
        | _ -> Models.TypeWithoutLength item

[<CLIMutable>]
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
          ``type`` = item.``type``.FromDomain()
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
          Models.Column.``type`` = Models.ColumnType.ToDomain(item.``type``)
          Models.Column.isNullable = item.isNullable }

[<CLIMutable>]
[<XmlType("constraint")>]
type Constraint = {
    [<XmlAttribute>] 
    ``type``: string
    [<XmlAttribute>] 
    name: string
    [<XmlArray>]
    [<XmlArrayItem("column")>]
    columns : System.Collections.Generic.List<string>
    [<XmlAttribute>] 
    definition: string
} with 
    static member FromDomain (item: Models.TableConstraint) =
        match item.``type`` with
        | Models.TableConstraintType.PrimaryKey cols -> 
                { ``type`` = "PrimaryKey"; 
                  definition = null; 
                  columns = cols |> List.toArray |> System.Collections.Generic.List<string>
                  name = item.name }
        | Models.TableConstraintType.Unique cols -> 
                { ``type`` = "Unique"
                  definition = null
                  columns = cols |> List.toArray |> System.Collections.Generic.List<string>
                  name = item.name }
        | Models.TableConstraintType.Check def -> 
                { ``type`` = "Check"; 
                  definition = def; 
                  columns = null; 
                  name = item.name }

    static member ToDomain (item: Constraint) =
        let cols = if item.columns = null then List.empty else item.columns |> List.ofSeq
        match item.``type`` with
        | "PrimaryKey" -> 
            { Models.TableConstraint.name = item.name
              Models.TableConstraint.``type`` = Models.TableConstraintType.PrimaryKey cols }
        | "Unique" ->
            { Models.TableConstraint.name = item.name
              Models.TableConstraint.``type`` = Models.TableConstraintType.Unique cols }
        | "Check" ->
            { Models.TableConstraint.name = item.name
              Models.TableConstraint.``type`` = Models.TableConstraintType.Check item.definition }
        | _ -> raise (System.Exception("Unknown constraint type"))

[<CLIMutable>]
[<XmlRoot("table")>]
type TableInfo = {
    [<XmlAttribute>] 
    name: string;

    [<XmlAttribute>] 
    schema: string;

    [<XmlArray>]
    columns: System.Collections.Generic.List<ColumnInfo>

    [<XmlArray>]
    constraints: System.Collections.Generic.List<Constraint>
} with 
    static member FromDomain engine (item: Models.TableInfo) =
        { name = item.name
          schema = item.schema
          columns = item.columns 
                    |> List.sortBy (fun x -> x.position) 
                    |> List.map (ColumnInfo.FromDomain engine)
                    |> List.toArray 
                    |> System.Collections.Generic.List<ColumnInfo> 
          constraints = item.constraints
                        |> List.map (Constraint.FromDomain)
                        |> List.toArray
                        |> System.Collections.Generic.List<Constraint> }
          
    static member ToDomain engine (item: TableInfo) =
        { Models.TableInfo.name = item.name
          Models.TableInfo.schema = item.schema 
          Models.TableInfo.columns = item.columns
                                        |> List.ofSeq
                                        |> List.mapi (fun i x -> ColumnInfo.ToDomain i engine x) 
          Models.TableInfo.constraints = item.constraints 
                                            |> List.ofSeq
                                            |> List.map Constraint.ToDomain}

[<CLIMutable>]
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
          ``type`` = item.``type``.FromDomain()
          isNullable = item.isNullable }

    static member ToDomain index (item: Attribute) =
        { Models.UdtAttributes.name = item.name
          Models.UdtAttributes.isNullable = item.isNullable
          Models.UdtAttributes.position = index
          Models.UdtAttributes.``type`` = Models.ColumnType.ToDomain(item.``type``) }

[<CLIMutable>]
[<XmlRoot("type")>]
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