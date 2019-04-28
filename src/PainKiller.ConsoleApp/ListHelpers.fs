module PainKiller.ConsoleApp.ListHelpers

let getFirstMatchingResult fn =
    List.tryHead << List.filter fn

let pairLists fn l1 l2 =
    l1 |> List.map (fun x -> 
                        let matchingElement = l2 |> getFirstMatchingResult (fn x)
                        (x, matchingElement))

let removeUnpairedItems a =
    a |> List.filter (fun (x, (y: 'b option)) -> y.IsSome)
      |> List.map (fun (x, y) -> (x, y.Value))

let pairListsWithoutUnpairedItems fn l1 l2 = pairLists fn l1 l2 |> removeUnpairedItems