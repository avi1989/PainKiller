namespace System.Data
open System.Data
open System.Runtime.CompilerServices

[<Extension>]
type DataReaderExtensions() =
    static member GetNullableItem (reader:IDataReader) (colName: string) (getValue: int -> 'A) =
        let ordinal = reader.GetOrdinal(colName)
        match reader.IsDBNull(ordinal) with
        | true -> None
        | false -> Some (getValue ordinal)

    [<Extension>]
    static member GetString(reader: IDataReader, colName: string) =
        reader.GetString(reader.GetOrdinal(colName))

    [<Extension>]
    static member GetChar(reader: IDataReader, colName: string) =
        reader.GetChar(reader.GetOrdinal(colName))

    [<Extension>]
    static member GetValue(reader: IDataReader, colName: string) = 
        reader.GetValue(reader.GetOrdinal(colName))

    [<Extension>]
    static member GetInt32(reader: IDataReader, colName: string) =
        reader.GetInt32(reader.GetOrdinal(colName))

    [<Extension>]
    static member GetNullableString(reader: IDataReader, colName: string) =
        DataReaderExtensions.GetNullableItem reader colName reader.GetString

    [<Extension>]
    static member GetNullableInt32 (reader: IDataReader, colName: string) =
        DataReaderExtensions.GetNullableItem reader colName reader.GetInt32
