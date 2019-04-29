namespace PainKiller.Adapters.PostgreSQL
open PainKiller.Abstractions.Contracts
open PainKiller.Abstractions.Models

type DatabaseReader() =
    interface IDatabaseReader with
        member this.GetFunctions connection =
            raise (System.NotImplementedException())

        member this.GetIndexes connection =
            raise (System.NotImplementedException())

        member this.GetProcedures connection =
            raise (System.NotImplementedException())

        member this.GetSchemas connection =
            raise (System.NotImplementedException())

        member this.GetSequences connection =
            raise (System.NotImplementedException())

        member this.GetUserDefinedTypes connection =
            raise (System.NotImplementedException())

        member this.GetViews connection =
            raise (System.NotImplementedException())

        member this.GetTables connection =
            List.empty

