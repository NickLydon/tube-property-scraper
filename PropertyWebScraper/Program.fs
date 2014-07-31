// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSharp.Data
open System.Xml.Linq
open System
open System.Net
open System.Net.Http
open System.Web.Http
open System.IO
open System.Text
open Newtonsoft.Json

type Provider = XmlProvider<"stations-facilities.xml", false, true>
type JsonProv = JsonProvider<"http://www.rightmove.co.uk/typeAhead/uknostreet/AL/DE/RN/EY/">
type KeysProv = JsonProvider<"stationkeys.json">
type station = { name: string; serving: string array; point: (decimal * decimal); }
type rmstationkey = { station: station; key: string;}   

module Seq =
    //Unlike Seq.windowed, this will not overlap elements, i.e. whereas Seq.windowed would return
    //[1;2];[2;3] given input of [1;2;3;] and size of 2, this will give
    //[1;2];[3] for the same input
    let windowedExclusive size sequence =
        let rec loop sequence =
            seq {
                if size > 0 && Seq.empty <> sequence then
                    if (Seq.length sequence <= size) 
                    then yield sequence 
                    else                        
                        yield (sequence |> Seq.take size)
                        yield! loop (sequence |> Seq.skip size)
            }
        loop sequence
    

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
    let info = Provider.GetSample()

    lines
    |> Seq.map(fun line ->
        info.Stations.Stations
        |> Array.filter(fun s -> s.ServingLines.ServingLines |> Array.exists(fun a -> a = line))
        |> Array.map(fun s -> 
            let split = s.Placemark.Point.Coordinates.Split([| ',' |])
            { name = s.Name; 
                serving = s.ServingLines.ServingLines; 
                point = (System.Decimal.Parse(split.[0]),  System.Decimal.Parse(split.[1])) })            
    )
    |> Seq.collect id
    |> Set.ofSeq

let linesOfInterest = [ "Hammersmith & City"; "Metropolitan"; "Circle"; ]

[<EntryPoint>]
let main argv =     

    let info = KeysProv.GetSamples()
    
    info
    |> Seq.map(fun s ->
        async {
            let! response = Http.AsyncRequest(sprintf "http://www.rightmove.co.uk/property-to-rent/find.html?searchType=RENT&locationIdentifier=%s&insId=4&radius=0.5&displayPropertyType=&minBedrooms=2&maxBedrooms=&minPrice=&maxPrice=2000&maxDaysSinceAdded=&retirement=&sortByPriceDescending=&_includeLetAgreed=on&primaryDisplayPropertyType=&secondaryDisplayPropertyType=&oldDisplayPropertyType=&oldPrimaryDisplayPropertyType=&letType=&letFurnishType=&houseFlatShare=false" s.Key)
        
            let body =            
                match response.Body with
                | FSharp.Data.HttpResponseBody.Text t -> (s.Station.Name, t)
                | _ -> failwith ""

            return body
        }
    )
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.iter (fun (station, resp) -> File.WriteAllText(sprintf "%s.html" station, resp))
            


    printfn "%A" argv
    0 // return an integer exit code
