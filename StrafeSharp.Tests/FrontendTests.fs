module Tests

open System
open Xunit

open StrafeSharp

[<Fact>]
let ``Render returns a Datagram`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState
    Assert.NotNull datagram
