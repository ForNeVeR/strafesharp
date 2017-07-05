module StrafeSharp.Tests.FrontendTests

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
let ``Each packet has a length of 64`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState

    datagram.Packets
    |> Array.iter (fun a -> Assert.Equal(64, a.Length))

let private colorPacketHeader n =
    let index = byte (n / 4)
    [| 127uy; index; 60uy; 0uy |]

let resize = Utils.resizeTo 64

let splitValueTest baseIndex colorArray datagram =
    let expectedPacket0 = Array.concat [| colorPacketHeader 0; Array.take 60 colorArray |]
    let expectedPacket1 =
        Array.concat [| colorPacketHeader 1
                        colorArray |> Array.skip 60 |> Array.take 60 |]
    let expectedPacket2 =
        resize (Array.concat [| colorPacketHeader 2
                                Array.skip 120 colorArray |])
    Assert.Equal<byte>(expectedPacket0, datagram.Packets.[baseIndex])
    Assert.Equal<byte>(expectedPacket1, datagram.Packets.[baseIndex + 1])
    Assert.Equal<byte>(expectedPacket2, datagram.Packets.[baseIndex + 2])

[<Fact>]
let ``Color values are distributed between the corresponding packets`` () =
    let keyboardState =
        { RedValues = [| 1uy .. byte KeyboardState.KeyCount |]
          GreenValues = [| byte KeyboardState.KeyCount .. 1uy |]
          BlueValues = [| byte KeyboardState.KeyCount .. 1uy |] }
    let datagram = Frontend.Render keyboardState
    splitValueTest 0 keyboardState.RedValues datagram
    splitValueTest 4 keyboardState.GreenValues datagram
    splitValueTest 8 keyboardState.BlueValues datagram

[<Fact>]
let ``Packets 3, 7 and 11 have fixed content`` () =
    let keyboardState = KeyboardState.Empty
    let datagram = Frontend.Render keyboardState
    let genFixedPacket = function
        | 3 -> resize [| 7uy; 40uy; 1uy; 3uy; 1uy |]
        | 7 -> resize [| 7uy; 40uy; 2uy; 3uy; 1uy |]
        | 11 -> resize [| 7uy; 40uy; 3uy; 3uy; 2uy |]
        | _ -> failwith "Impossible"
    let packet3 = datagram.Packets.[3]
    let packet7 = datagram.Packets.[7]
    let packet11 = datagram.Packets.[11]

    Assert.Equal<byte>(genFixedPacket 3, packet3)
    Assert.Equal<byte>(genFixedPacket 7, packet7)
    Assert.Equal<byte>(genFixedPacket 11, packet11)
