module PainKiller.ConsoleApp.FileSystem.Reader
open PainKiller.ConsoleApp
open PainKiller.ConsoleApp.Models

let readFromFileSystem basePath engine =
    let readSimpleScripts = Readers.SimpleScriptReader.readScripts engine basePath
    { tables = Readers.TableReader.readTables engine basePath
      functions = readSimpleScripts "functions"
      views = readSimpleScripts "views"
      procedures = readSimpleScripts "procedures"
      userDefinedTypes = Readers.UdtReader.readUdts engine basePath 
      schemas = Readers.SchemaReader.readSchemas basePath
      sequences = Readers.SimpleScriptReader.readScripts engine basePath "sequences" }
