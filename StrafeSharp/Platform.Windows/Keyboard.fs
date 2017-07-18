module internal StrafeSharp.Platform.Windows.Keyboard

open System
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

open StrafeSharp

let private devicePath = function
    | DeviceId(pid, vid, mi) -> String.Format("VID_{0:X4}_PID_{1:X4}_MI_{2:X2}", pid, vid, mi)

let private enumerateDeviceInterface deviceClassGuid =
    seq {
        let setHandle =
            Native.setupDiGetClassDevs
                Native.GUID_DEVINTERFACE_HID
                (Native.DIGCF_PRESENT ||| Native.DIGCF_DEVICEINTERFACE)
        try
            failwithf "TODO[F]: Call SetupDiEnumDeviceInfo"
        finally
            Native.setupDiDestroyDeviceInfoList setHandle
    }

let private matchingDevice deviceId deviceInfo =
    let devicePath = devicePath deviceId
    failwithf "TODO[F]: Call CM_Get_Device_ID on deviceInfo, compare result with devicePath"

let getDeviceInterfacePath deviceId =
    failwithf "TODO[F]: Call SetupDiGetDeviceInterfaceDetail; retrieve DevicePath"

let openDeviceFile path =
    failwithf "TODO[F]: Call CreateFile for path"

let private openDeviceHandle deviceId =
    let deviceInfo =
        enumerateDeviceInterface Native.GUID_DEVINTERFACE_HID
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
    Native.writeFile file data

type WindowsKeyboard(handle : SafeFileHandle) =
    interface IStrafeKeyboard with
        member __.Dispose() = handle.Dispose()
        member __.SendData(data : Datagram) =
            data.Packets
            |> Array.iter(writeFile handle)

let connect() : WindowsKeyboard =
    let handle = getCorsairStrafeHandle()
    new WindowsKeyboard(handle)
