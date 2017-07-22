module internal StrafeSharp.Platform.Windows.Keyboard

open System
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

open StrafeSharp

let private devicePath = function
    | DeviceId(pid, vid, mi) -> String.Format("VID_{0:X4}_PID_{1:X4}_MI_{2:X2}", pid, vid, mi)

let private matchingDevice deviceId deviceInfo =
    let expectedDevicePath = devicePath deviceId
    let actualDevicePath = Native.cmGetDeviceId deviceInfo
    expectedDevicePath = actualDevicePath

let private getDeviceInterfacePath deviceInfoSet devInfoData =
    let interfaceData = Native.setupDiEnumDeviceInterfaces deviceInfoSet devInfoData
    let size = Native.sizeFromSetupDiGetDeviceInformationDetail deviceInfoSet interfaceData
    use memory = SpDeviceInterfaceDetailData.allocate (int size)
    Native.pathFromSetupDiGetDeviceInformationDetail deviceInfoSet interfaceData

let openDeviceFile path =
    failwithf "TODO[F]: Call CreateFile for path"

let private openDeviceHandle deviceId =
    let deviceFilter = matchingDevice deviceId
    let mutable foundDevice = None

    let deviceInfoSet =
        Native.setupDiGetClassDevs
            Native.GUID_DEVINTERFACE_HID
            (Native.DIGCF_PRESENT ||| Native.DIGCF_DEVICEINTERFACE)
    try
        let mutable counter = 0u
        let mutable next = true
        while next do
            match Native.setupDiEnumDeviceInfo deviceInfoSet counter with
            | Some deviceData ->
                counter <- counter + 1u
                if deviceFilter deviceData
                then
                    next <- false
                    let interfacePath = getDeviceInterfacePath deviceInfoSet deviceData
                    foundDevice <- openDeviceFile interfacePath
                    // TODO[F]: No idea if we actually need to call HidD_SetFeature
            | None -> next <- false
    finally
        Native.setupDiDestroyDeviceInfoList deviceInfoSet

    match foundDevice with
    | Some device -> device
    | None -> failwithf "Cannot find device with path %s" (devicePath deviceId)

let private getCorsairStrafeHandle() =
    let vendorId = 0x1b1cu
    let productId = 0x1b20u
    let interfaceId = 0x3u
    let deviceId = DeviceId(vendorId, productId, interfaceId)
    openDeviceHandle deviceId

type WindowsKeyboard(handle : SafeFileHandle) =
    interface IStrafeKeyboard with
        member __.Dispose() = handle.Dispose()
        member __.SendData(data : Datagram) =
            data.Packets
            |> Array.iter(Native.writeFile handle)

let connect() : WindowsKeyboard =
    let handle = getCorsairStrafeHandle()
    new WindowsKeyboard(handle)
