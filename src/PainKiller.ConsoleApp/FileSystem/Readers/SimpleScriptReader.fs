module PainKiller.ConsoleApp.FileSystem.Readers.SimpleScriptReader

open System.IO
open PainKiller.Abstractions
open PainKiller.ConsoleApp

let private readScript filePath =
    let itemBody = System.IO.File.ReadAllText(filePath)
    let fileName = Path.GetFileNameWithoutExtension(filePath);
    let index = fileName.IndexOf('.')
    let schema = fileName.Substring(0, index);
    let tableName = fileName.Substring(index+1);

    { Models.SimpleDatabaseItem.definition = itemBody
      Models.SimpleDatabaseItem.schema = schema
      Models.SimpleDatabaseItem.name = tableName }

let readScripts _ basePath itemType = 
    let filesPath = sprintf "%s/%s" basePath itemType
    System.IO.Directory.EnumerateFiles(filesPath, "*.sql") 
        |> List.ofSeq
        |> List.map readScript

