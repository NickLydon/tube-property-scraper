module RightMoveStationKeyApp.Integration.Tests

open System
open NUnit.Framework
open FsUnit
open FSharp.Data
open System.IO
open XYZ.RightMoveStationKeyApp

[<Test>]
let ``should give rightmove keys for all stations on circle line``() = 
    let filename = "output.json"
    run filename [| "Circle" |]
    let contents = File.ReadAllText filename
    contents |> should equal """[{"station":{"name":"Bow Road","serving":["Circle","District","Hammersmith & City"],"point":{"Item1":-0.024823746874528683,"Item2":51.526800254826064000}},"key":"STATION%5E1166"},{"station":{"name":"Embankment","serving":["Bakerloo","Circle","District","Northern"],"point":{"Item1":-0.122360215814178660,"Item2":51.507241796748396000}},"key":"STATION%5E3251"},{"station":{"name":"Euston Square","serving":["Circle","Hammersmith & City","Metropolitan"],"point":{"Item1":-0.135836140694320340,"Item2":51.525561006016105000}},"key":"STATION%5E3311"},{"station":{"name":"Farringdon","serving":["Circle","Hammersmith & City","Metropolitan"],"point":{"Item1":-0.105065254236880330,"Item2":51.520444754930790000}},"key":"STATION%5E3431"},{"station":{"name":"Tower Hill","serving":["Circle","District"],"point":{"Item1":-0.076353695174605170,"Item2":51.510065953884120000}},"key":"STATION%5E9272"}]"""

[<Test>]
let ``should give rightmove keys for all stations on every line when no line args provided``() = 
    let filename = "output.json"
    run filename [| |]
    let contents = File.ReadAllText filename
    contents |> should equal """[{"station":{"name":"Euston","serving":["Northern","Victoria"],"point":{"Item1":-0.133289718799315530,"Item2":51.528596260899460000}},"key":"STATION%5E3311"},{"station":{"name":"Oxford Circus","serving":["Bakerloo","Central","Victoria"],"point":{"Item1":-0.141768983668966370,"Item2":51.515123799037090000}},"key":"STATION%5E6953"},{"station":{"name":"Pimlico","serving":["Victoria"],"point":{"Item1":-0.133748665287791170,"Item2":51.489193760110520000}},"key":"STATION%5E7220"},{"station":{"name":"Tottenham Hale","serving":["Victoria"],"point":{"Item1":-0.060281116367504950,"Item2":51.588045301521500000}},"key":"STATION%5E9272"}]"""