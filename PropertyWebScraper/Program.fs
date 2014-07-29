// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSharp.Collections.ParallelSeq
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

let getStationKeys stationsOfInterest =
    let windowed size sequence =
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

    stationsOfInterest
    |> List.ofSeq
    |> List.map(fun station ->             
        let typeaheadurl = "http://www.rightmove.co.uk/typeAhead/uknostreet/"

        let stationkey (station, response) =
            let response = JsonProv.Parse(response)
            let stations = 
                response.TypeAheadLocations 
                |> Seq.filter (fun r -> r.LocationIdentifier.ToLower().Contains("station"))
            if Seq.isEmpty stations then None else Some({ station = station; key = (Seq.head stations).LocationIdentifier.Replace("^", "%5E"); })
                

        let twoCharNameSplit = 
            station.name 
            |> windowed 2 
            |> Seq.map System.String.Concat 
            |> Seq.map(fun s -> s.ToUpper()) 
            |> List.ofSeq
        let lookAheadUrls = 
            let rec loop state url =
                seq {
                    match state with
                    | [] -> failwith "supply some text"
                    | h::[] -> 
                        let newUrl = url + h + "/"
                        yield newUrl                             
                    | h::t -> 
                        let newUrl = url + h + "/"
                        yield newUrl 
                        yield! loop t newUrl
                }
            loop twoCharNameSplit typeaheadurl

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

[<EntryPoint>]
let main argv =     

    let info = Provider.GetSample()
    
    let allLines = 
        info.Stations.Stations
        |> Array.map(fun s -> s.ServingLines.ServingLines)
        |> Array.collect id
        |> Set.ofArray

    let stationsOfInterest = [ "Farringdon"; "Liverpool Street"; ]
    let linesOfInterest = [ "Hammersmith & City"; "Metropolitan"; "Circle"; ]

    let stations lines =
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

    
    File.WriteAllText(
        "stationkeys.json",    
        Newtonsoft.Json.JsonConvert.SerializeObject(
            getStationKeys (stations linesOfInterest)
            |> List.filter Option.isSome
            |> List.map Option.get
        )
    )
    
//    let request = Http.Request("http://www.rightmove.co.uk/property-to-rent/find.html?searchType=RENT&locationIdentifier=STATION%5E3431&insId=4&radius=0.5&displayPropertyType=&minBedrooms=2&maxBedrooms=&minPrice=&maxPrice=2000&maxDaysSinceAdded=&retirement=&sortByPriceDescending=&_includeLetAgreed=on&primaryDisplayPropertyType=&secondaryDisplayPropertyType=&oldDisplayPropertyType=&oldPrimaryDisplayPropertyType=&letType=&letFurnishType=&houseFlatShare=false")
    
    



    printfn "%A" argv
    0 // return an integer exit code
