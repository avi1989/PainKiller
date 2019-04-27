module PainKiller.ConsoleApp.FileSystem.Writer
open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.FileSystem.Writers

let writeToFileSystem basePath engine (database: Models.Database) =
    database.tables |> TableWriter.writeToFileSystem engine basePath |> ignore
    database.functions |> SimpleScriptWriter.writeToFileSystem basePath "functions" |> ignore
    database.views |> SimpleScriptWriter.writeToFileSystem basePath "views" |> ignore
    database.procedures |> SimpleScriptWriter.writeToFileSystem basePath "procedures" |> ignore
    database.userDefinedTypes |> UdtWriter.writeToFileSystem basePath |> ignore