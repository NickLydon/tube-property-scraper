module RightMoveStationKeyApp

open System
open FSharp.Data
open CommonTypes

type JsonProv = JsonProvider<"http://www.rightmove.co.uk/typeAhead/uknostreet/AL/DE/RN/EY/">

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
        |> Seq.last
        |> stationkey
    )
