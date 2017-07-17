module internal StrafeSharp.Platform.Windows

open System
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

open StrafeSharp

module NativeConstants =
    let GUID_DEVINTERFACE_HID = Guid.ParseExact("{4D1E55B2-F16F-11CF-88CB-001111000030}", "B")

module NativeMethods =
    [<DllImport("Kernel32")>]
    extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint32 nNumberOfBytesToWrite,
        uint32& lpNumberOfBytesWritten,
        nativeint lpOverlapped)

type private DeviceId = DeviceId of vid : uint32 * pid : uint32 * mi : uint32
let private devicePath = function
    | DeviceId(pid, vid, mi) -> String.Format("VID_{0:X4}_PID_{1:X4}_MI_{2:X2}", pid, vid, mi)

let private enumerateDeviceInterface deviceClassGuid =
    failwithf "TODO[F]: Call SetupDiGetClassDevs"
    // TODO[F]: Don't forget to call SetupDiDestroyDeviceInfoList!

let private matchingDevice deviceId deviceInfo =
    let devicePath = devicePath deviceId
    failwithf "TODO[F]: Call CM_Get_Device_ID on deviceInfo, compare result with devicePath"

let getDeviceInterfacePath deviceId =
    failwithf "TODO[F]: Call SetupDiGetDeviceInterfaceDetail; retrieve DevicePath"

let openDeviceFile path =
    failwithf "TODO[F]: Call CreateFile for path"

let private openDeviceHandle deviceId =
    let deviceInfo =
        enumerateDeviceInterface NativeConstants.GUID_DEVINTERFACE_HID
        |> Seq.filter(matchingDevice deviceId)
        |> Seq.tryHead
    match deviceInfo with
    | Some info ->
        let interfacePath = getDeviceInterfacePath info
        openDeviceFile interfacePath
        // TODO[F]: No idea if we actually need to call HidD_SetFeature
    | None -> failwithf "Cannot find device with path %s" (devicePath deviceId)

let private getCorsairStrafeHandle() =
    let vendorId = 0x1b1cu
    let productId = 0x1b20u
    let interfaceId = 0x3u
    let deviceId = DeviceId(vendorId, productId, interfaceId)
    openDeviceHandle deviceId

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
    let handle = getCorsairStrafeHandle()
    new WindowsKeyboard(handle)
