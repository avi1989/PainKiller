namespace PainKiller.ConsoleApp.Contracts

open PainKiller.ConsoleApp.Models

type IDatabaseRetriever =
    abstract member GetDatabase: string -> Database

type IDatabasePersister =
    abstract member PersistDatabase: string -> Database -> unit