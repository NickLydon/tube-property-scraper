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