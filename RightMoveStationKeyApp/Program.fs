// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSharp.Data
open System
open System.IO
open CommonTypes
open Newtonsoft.Json
open XYZ.RightMoveStationKeyApp


[<EntryPoint>]
let main argv = 
    run argv.[0] argv.[0..]

    0 // return an integer exit code