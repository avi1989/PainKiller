module PainKiller.ConsoleApp.PostgreSQL.Persister.ConstraintPersister

open PainKiller.ConsoleApp.Models
open Npgsql
open System.Data

let createConstraint (sqlConnection: NpgsqlConnection) table schema (con: TableConstraint) = 
    let concat = String.concat ", "
    let constraintQuery = match con.``type`` with
                            | PrimaryKey cols -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s PRIMARY KEY (%s)" schema table con.name (concat cols)
                            | Unique cols -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s UNIQUE (%s)" schema table con.name (concat cols)
                            | Check def -> sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s %s" schema table con.name def
                            | ForeignKey fk -> 
                                sprintf "ALTER TABLE %s.%s ADD CONSTRAINT %s FOREIGN KEY(%s) REFERENCES %s.%s (%s)" schema table con.name (concat fk.sourceColumns) fk.destinationSchema fk.destinationTable (concat fk.destinationColumns)
    if (sqlConnection.State <> ConnectionState.Open)
    then sqlConnection.Open() |> ignore
    use command = sqlConnection.CreateCommand()
    command.CommandText <- constraintQuery
    command.ExecuteNonQuery()

let createConstraintsForTable sqlConnection (table: TableInfo) =
    table.constraints
    |> List.map (createConstraint sqlConnection table.name table.schema)

let createConstraintsForTables sqlConnection (tables: TableInfo list) =
    tables
    |> List.map (createConstraintsForTable sqlConnection)