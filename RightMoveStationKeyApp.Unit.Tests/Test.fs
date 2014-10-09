module RightMoveStationKeyApp.Unit.Tests

open System
open NUnit.Framework
open XYZ.RightMoveStationKeyApp
open FsUnit
open FSharp.Data
open CommonTypes

let emptyHttpResponse = { 
            Body = HttpResponseBody.Text("hello"); 
            Cookies = Map.empty; 
            HttpResponse.Headers = Map.empty; 
            HttpResponse.StatusCode = 200;
            ResponseUrl = "";
        }

[<Test>]
let ``should return set when given no station lines``() =
    getStationKeys (fun url -> emptyHttpResponse) [] 
    |> Seq.length
    |> should equal 0

[<Test>]
let ``should keep first response doesn't throw exception``() =
    let iters = ref -1
    let givenStation = 
        { 
            name = "name"
            point = (0m,0m)
            serving = [| "any" |]  } 

    let expected = Some { 
        rmstationkey.key = "station_name0"
        station = givenStation }

    let actual = 
        getStationKeys (fun url -> 
            iters := !iters + 1
            match !iters with 
            | a -> { emptyHttpResponse with Body = HttpResponseBody.Text(sprintf """{ "typeAheadLocations": [ { "locationIdentifier": "station_name%i" } ] }""" !iters) } 
            | b when b <= 0 -> failwith "what what") [ givenStation ]
        |> Seq.head

    actual |> should equal expected