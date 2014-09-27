namespace RightMoveStationKeyApp.Tests

open System
open NUnit.Framework
open FsUnit
open FSharp.Data
open RightMoveStationKeyApp

[<TestFixture>]
type Test() = 
    [<Test>]

    member x.ShouldGiveEmptyKeyCollectionWhenGivenNoStations() = 
        Seq.length (getStationKeys []) |> should equal 0