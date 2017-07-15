module StrafeSharp.Utils

open System

let resizeTo n array =
    let mutable a = array
    Array.Resize(&a, n)
    a
