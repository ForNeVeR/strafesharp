module internal StrafeSharp.Platform.Windows

open System
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

open StrafeSharp

module NativeMethods =
    [<DllImport("Kernel32")>]
    extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint32 nNumberOfBytesToWrite,
        uint32& lpNumberOfBytesWritten,
        nativeint lpOverlapped)

let private writeFile file data =
    let mutable writtenBytes = 0u
    let result = NativeMethods.WriteFile(file,
                                         data,
                                         uint32 data.Length,
                                         &writtenBytes,
                                         IntPtr.Zero)
    if not result then Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())

type WindowsKeyboard(handle : SafeFileHandle) =
    interface IStrafeKeyboard with
        member __.Dispose() = handle.Dispose()
        member __.SendData(data : Datagram) =
            data.Packets
            |> Array.iter(writeFile handle)

let openKeyboard() : WindowsKeyboard =
    failwithf "Not implemented yet"
