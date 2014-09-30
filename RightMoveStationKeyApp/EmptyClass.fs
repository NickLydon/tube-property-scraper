namespace XYZ

open FSharp.Data
open System
open System.IO
open CommonTypes
open Newtonsoft.Json


type TflProvider = XmlProvider<"stations-facilities.xml", false, true>
type JsonProv = JsonProvider<"http://www.rightmove.co.uk/typeAhead/uknostreet/AL/DE/RN/EY/">

module RightMoveStationKeyApp =
    let getStationKeys stationsOfInterest =
        
        stationsOfInterest
        |> Seq.map(fun station ->             
            let typeaheadurl = "http://www.rightmove.co.uk/typeAhead/uknostreet/"

            let stationkey (station, response) =
                let response = JsonProv.Parse(response)
                let stations = 
                    response.TypeAheadLocations 
                    |> Seq.filter (fun r -> r.LocationIdentifier.ToLower().Contains("station"))
                if Seq.isEmpty stations then None else Some({ station = station; key = (Seq.head stations).LocationIdentifier.Replace("^", "%5E"); })
                    
            let lookAheadUrls = 
                let twoCharNameSplit = 
                    station.name 
                    |> Seq.windowedExclusive 2 
                    |> Seq.map System.String.Concat 
                    |> Seq.map(fun s -> s.ToUpper()) 
                    |> List.ofSeq
                twoCharNameSplit 
                |> Seq.fold(fun (urls: string list, previous) current -> 
                    let newUrl = previous + current + "/" 
                    (newUrl::urls, newUrl)) ([], typeaheadurl)
                |> fst
            
            let responses =
                lookAheadUrls
                |> Seq.map(fun url ->
                    try
                        Some(Http.Request url)
                    with
                    | _ -> None
                )
                |> Seq.takeWhile Option.isSome
                |> Seq.map(fun r -> 
                    let body = r.Value.Body

                    match body with
                    | FSharp.Data.HttpResponseBody.Text a -> (station, a)
                    | _ -> failwith "shouldn't receive binary" 
                )
            if Seq.length responses = 0 then None else Some(Seq.last responses |> stationkey)
        )
        |> Seq.filter Option.isSome
        |> Seq.map Option.get

    let writeStationKeysToFile stations fileOutput =         
        File.WriteAllText(
            fileOutput,    
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
            |> Array.filter(fun s -> 
                s.ServingLines 
                |> Array.exists(fun a -> a = line)
            )
            |> Array.map(fun s -> 
                let split = s.Placemark.Point.Coordinates.Split([| ',' |])
                { name = s.Name; 
                    serving = s.ServingLines; 
                    point = (System.Decimal.Parse(split.[0]),  System.Decimal.Parse(split.[1])) })            
        )
        |> Seq.collect id
        |> Set.ofSeq

    let run fileOutput stationNames = 
        let stationsOfInterest = 
            let names = 
                match stationNames with
                | [| |] -> 
                    TflProvider.GetSample().Stations
                    |> Array.map (fun s -> s.Name)
                | _ -> stationNames
            names |> stations
        
        writeStationKeysToFile stationsOfInterest fileOutput