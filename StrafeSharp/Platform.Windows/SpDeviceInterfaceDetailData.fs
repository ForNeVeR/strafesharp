/// Declares routines for working with platform-dependent SP_DEVICE_INTERFACE_DETAIL_DATA type.
module internal StrafeSharp.Platform.Windows.SpDeviceInterfaceDetailData

open System
open System.Runtime.InteropServices

open FSharp.NativeInterop

type MemoryBuffer(pointer : nativeint, size : int) =
    member __.Pointer : nativeint = pointer
    member __.Size : int = size
    interface IDisposable with
        member __.Dispose() = Marshal.FreeHGlobal pointer

#nowarn "9" // compiler warns on StructLayout and NativeInterop module

module CrossPlatform =

    // We need Pack = 1 for x32 code, but don't need it for x64 code due to fancy internals of this
    // structure. See https://stackoverflow.com/questions/10728644/properly-declare-sp-device-interface-detail-data-for-pinvoke
    // for details.
    [<StructLayout(LayoutKind.Sequential, Pack = 1)>]
    [<Struct>]
    type private SP_DEVICE_INTERFACE_DETAIL_DATA32 =
        val mutable cbSize : uint32

        // It's declared as TCHAR DevicePath[ANYSIZE_ARRAY] in the original code, but it has no use
        // anyway, so just declare it as char to provide proper padding:
        val mutable DevicePath : char

    [<StructLayout(LayoutKind.Sequential)>]
    [<Struct>]
    type private SP_DEVICE_INTERFACE_DETAIL_DATA64 =
        val mutable cbSize : uint32

        // See comment above.
        val mutable DevicePath : char

    let determineHeaderSize : int -> int = function
        | 4 -> sizeof<SP_DEVICE_INTERFACE_DETAIL_DATA32>
        | 8 -> sizeof<SP_DEVICE_INTERFACE_DETAIL_DATA64>
        | other -> failwithf "Cannot determine SP_DEVICE_INTERFACE_DETAIL_DATA static header size for architecture with IntPtr.Size = %d" other

    let allocate (intPtrSize : int) (totalBufferSize : int) : MemoryBuffer =
        let headerSize = determineHeaderSize intPtrSize
        if totalBufferSize < headerSize
        then failwithf "bufferSize (%d) should be greater than SP_DEVICE_INTERFACE_DETAIL_DATA header size (%d)" totalBufferSize headerSize

        let pointer = Marshal.AllocHGlobal(totalBufferSize)
        Marshal.WriteInt32(pointer, 0, headerSize)
        new MemoryBuffer(pointer, totalBufferSize)

let allocate : int -> MemoryBuffer = CrossPlatform.allocate IntPtr.Size

let getStringContent (buffer : MemoryBuffer) : string =
    let charPtr = NativePtr.add (NativePtr.ofNativeInt<char> buffer.Pointer) (sizeof<uint32> / sizeof<char>)
    String(charPtr)
