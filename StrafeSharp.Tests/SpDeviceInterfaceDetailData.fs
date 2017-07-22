module StrafeSharp.Tests.SpDeviceInterfaceDetailData

open System
open System.Runtime.InteropServices

open Xunit

open StrafeSharp.Platform.Windows

[<Fact>]
let ``determineHeaderSize returns 6 on 32-bit architecture`` () =
    Assert.Equal(6, SpDeviceInterfaceDetailData.CrossPlatform.determineHeaderSize 4)

[<Fact>]
let ``determineHeaderSize returns 8 on 64-bit architecture`` () =
    Assert.Equal(8, SpDeviceInterfaceDetailData.CrossPlatform.determineHeaderSize 8)

[<Fact>]
let ``allocate throws an exception for buffer = -1`` () =
    Assert.Throws<Exception>(Action (fun () -> ignore <| SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 -1))

[<Fact>]
let ``allocate throws an exception for buffer = 5`` () =
    Assert.Throws<Exception>(Action (fun () -> ignore <| SpDeviceInterfaceDetailData.CrossPlatform.allocate 4 5))

[<Fact>]
let ``allocate works for 64-bit architecture and buffer = 8`` () =
    use res = SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 8
    ()

[<Fact>]
let ``allocate works for 64-bit architecture and buffer = 9`` () =
    use res = SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 9
    ()

[<Fact>]
let ``allocate result can be freed`` () =
    use res = SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 100
    ()

[<Fact>]
let ``allocate writes the header value`` () =
    let getHeader ptr = Marshal.ReadInt32(ptr, 0)
    use struct1 = SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 100
    use struct2 = SpDeviceInterfaceDetailData.CrossPlatform.allocate 4 100
    Assert.Equal(8, getHeader struct1.Pointer)
    Assert.Equal(6, getHeader struct2.Pointer)

[<Fact>]
let ``getStringContent reads the content by offset 4`` () =
    use buffer = SpDeviceInterfaceDetailData.CrossPlatform.allocate 8 16
    let message = "test"

    message + "\u0000"
    |> Seq.iteri (fun i c -> Marshal.WriteInt16(buffer.Pointer, 4 + i * 2, int16 c))

    Assert.Equal(message, SpDeviceInterfaceDetailData.getStringContent buffer)
