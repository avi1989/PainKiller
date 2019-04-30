module PainKiller.ConsoleApp.FileSystem.Writer
open PainKiller.Abstractions
open PainKiller.ConsoleApp.FileSystem.Writers

let writeToFileSystem basePath engine (database: Models.Database) =
    database.schemas |> SchemaWriter.writeToFileSystem basePath |> ignore
    database.tables |> TableWriter.writeToFileSystem engine basePath |> ignore
    database.functions |> SimpleScriptWriter.writeToFileSystem basePath "functions" |> ignore
    database.views |> SimpleScriptWriter.writeToFileSystem basePath "views" |> ignore
    database.procedures |> SimpleScriptWriter.writeToFileSystem basePath "procedures" |> ignore
    database.userDefinedTypes |> UdtWriter.writeToFileSystem basePath |> ignore
    database.sequences |> SimpleScriptWriter.writeToFileSystem basePath "sequences" |> ignore
    database.indexes |> SimpleScriptWriter.writeToFileSystem basePath "indexes" |> ignore