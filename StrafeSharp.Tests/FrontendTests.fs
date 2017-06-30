module Tests

open System
open Xunit

open StrafeSharp

[<Fact>]
let ``Render returns a nonempty Datagram`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState
    Assert.NotEmpty datagram.Packets

[<Fact>]
let ``Datagram have 12 packets`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState
    Assert.Equal(12, datagram.Packets.Length)

[<Fact>]
let ``Each packet has a length of 60`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState

    datagram.Packets
    |> Array.iter (fun a -> Assert.Equal(60, a.Length))

[<Fact>]
let ``Packets 3, 7 and 11 have fixed content`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState
    let resize a = Array.Resize(a, 60); a.Value
    let genFixedPacket = function
        | 3 -> ref [| 7uy; 40uy; 1uy; 3uy; 1uy |] |> resize
        | 7 -> ref [| 7uy; 40uy; 2uy; 3uy; 1uy |] |> resize
        | 11 -> ref [| 7uy; 40uy; 3uy; 3uy; 2uy |] |> resize
        | _ -> failwith "Impossible"
    let packet3 = datagram.Packets.[3]
    let packet7 = datagram.Packets.[7]
    let packet11 = datagram.Packets.[11]

    Assert.Equal<byte>(genFixedPacket 3, packet3)
    Assert.Equal<byte>(genFixedPacket 7, packet7)
    Assert.Equal<byte>(genFixedPacket 11, packet11)
