namespace PainKiller.Adapters.PostgreSQL
open PainKiller.Abstractions.Contracts
open PainKiller.Abstractions.Models

type SqlGenerator() =
    let getTableColumnString (col: Column) =
        let dbColType = ColumnTypeMapper.mapDomainToDatabase col.``type``
        let defaultVal = match col.defaultValue with
                         | Some value -> sprintf "DEFAULT %s" value
                         | None -> ""
        let nullString = match col.isNullable with
                         | true -> ""
                         | false -> "NOT NULL"

        sprintf "\"%s\" %s %s %s" col.name dbColType nullString defaultVal
        
    let getUdtColumnString (col: UdtAttributes) =
        let dbColType = ColumnTypeMapper.mapDomainToDatabase col.``type``
        sprintf "\"%s\" %s" col.name dbColType
        
    interface IDdlStatementGenerator with
        member this.GetCreateConstraintStatements schema table con =
            let concat = String.concat ", "
            match con.``type`` with
            | PrimaryKey cols -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s PRIMARY KEY (%s)" schema table con.name (concat cols)
            | Unique cols -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s UNIQUE (%s)" schema table con.name (concat cols)
            | Check def -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s %s" schema table con.name def
            | ForeignKey fk -> 
                sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s FOREIGN KEY(%s) REFERENCES %s.%s (%s)" schema table con.name (concat fk.sourceColumns) fk.destinationSchema fk.destinationTable (concat fk.destinationColumns)
        member this.GetCreateTableStatement table =
            let columnInserts = table.columns |> List.map getTableColumnString |> String.concat "\n"
            sprintf "CREATE TABLE %s.%s \n(\n%s\n)" table.schema table.name columnInserts
        member this.GetAddColumnStatement column =
            sprintf "ALTER TABLE %s.%s ADD COLUMN %s " column.schemaName column.tableName (getTableColumnString column)
        member this.GetDropColumnStatement column =
            sprintf "ALTER TABLE %s.%s DROP COLUMN %s" column.schemaName column.tableName column.name
        member this.GetChangeColumnTypeStatement column =
            sprintf "ALTER TABLE %s.%s ALTER COLUMN %s SET DATA TYPE %s" column.schemaName column.tableName column.name
                                                                         (ColumnTypeMapper.mapDomainToDatabase column.``type``)
        member this.GetChangeColumnDefaultStatement column =
            let baseQuery = sprintf "ALTER TABLE %s.%s ALTER COLUMN %s" column.schemaName column.tableName column.name
            match column.defaultValue with
            | Some s -> sprintf "%s SET DEFAULT %s" baseQuery s
            | None -> sprintf "%s DROP DEFAULT" baseQuery
        member this.GetChangeColumnNullableStatement column =
            let buildBaseQuery = sprintf "ALTER TABLE %s.%s ALTER COLUMN %s %s NOT NULL" column.schemaName column.tableName column.name
            match column.isNullable with
            | true -> buildBaseQuery "SET"
            | false -> buildBaseQuery "DROP"
        member this.GetCreateUdtStatements udt =
            let udtCols = udt.attributes |> List.map getUdtColumnString |> String.concat ",\n"
            sprintf "CREATE TYPE %s.%s AS (\n%s\n)" udt.schema udt.name udtCols
