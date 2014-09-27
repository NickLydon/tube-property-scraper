// Learn more about F# at http://fsharp.net
open RightMoveStationKeyApp
open FSharp.Data
open System
open System.IO
open CommonTypes
open Newtonsoft.Json

type TflProvider = XmlProvider<"stations-facilities.xml", false, true>
type JsonProv = JsonProvider<"http://www.rightmove.co.uk/typeAhead/uknostreet/AL/DE/RN/EY/">


let writeStationKeysToFile stations =         
    File.WriteAllText(
        "stationkeys.json",    
        Newtonsoft.Json.JsonConvert.SerializeObject(
            getStationKeys stations
            |> Seq.filter Option.isSome
            |> Seq.map Option.get
        )
    )


let stations lines =
    let info = TflProvider.GetSample()

    lines
    |> Seq.map(fun line ->
        info.Stations
        |> Array.filter(fun s -> s.ServingLines |> Array.exists(fun a -> a = line))
        |> Array.map(fun s -> 
            let split = s.Placemark.Point.Coordinates.Split([| ',' |])
            { name = s.Name; 
                serving = s.ServingLines; 
                point = (System.Decimal.Parse(split.[0]),  System.Decimal.Parse(split.[1])) })            
    )
    |> Seq.collect id
    |> Set.ofSeq


[<EntryPoint>]
let main argv = 
    let stationsOfInterest = 
        let names = 
            match argv with
            | [| |] -> 
                TflProvider.GetSample().Stations
                |> Array.map (fun s -> s.Name)
            | _ -> argv

        names |> stations
    
    writeStationKeysToFile stationsOfInterest

    0 // return an integer exit code