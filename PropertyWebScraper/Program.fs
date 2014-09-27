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
open CommonTypes

type TflProvider = XmlProvider<"stations-facilities.xml", false, true>
type StationKeysProv = JsonProvider<"stationkeys.json">

    
[<EntryPoint>]
let main argv =     

    let info = StationKeysProv.GetSamples()
    
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
