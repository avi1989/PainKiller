namespace PainKiller.ConsoleApp.Contracts

open PainKiller.Abstractions.Models

type IDatabaseRetriever =
    abstract member GetDatabase: string -> Database

type IDatabasePersister =
    abstract member PersistDatabase: string -> Database -> Database -> unit