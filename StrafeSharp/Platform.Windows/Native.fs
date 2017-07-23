module internal StrafeSharp.Platform.Windows.Native

open System
open System.Text
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

let private DIGCF_PRESENT : uint32 = 0x2u
let private DIGCF_DEVICEINTERFACE : uint32 = 0x10u

let private FILE_SHARE_READ = 0x00000001u
let private FILE_SHARE_WRITE = 0x00000002u

let private GENERIC_READ = 0x80000000u
let private GENERIC_WRITE = 0x40000000u

let private GUID_DEVINTERFACE_HID : Guid = Guid.ParseExact("{4D1E55B2-F16F-11CF-88CB-001111000030}", "B")

let private INVALID_HANDLE_VALUE = nativeint -1

let private OPEN_EXISTING = 3u

let private throwLastWin32Error () =
    let error = Marshal.GetLastWin32Error()
    failwith <| String.Format("Win32 error {0:X}", error)

#nowarn "9" // compiler warns on StructLayout; that's unnecessary

[<StructLayout(LayoutKind.Sequential)>]
[<Struct>]
type SP_DEVICE_INTERFACE_DATA =
    val mutable cbSize : uint32
    val mutable InterfaceClassGuid : Guid
    val mutable Flags : uint32
    val mutable Reserved : unativeint

[<StructLayout(LayoutKind.Sequential)>]
[<Struct>]
type SP_DEVINFO_DATA =
    val mutable cbSize : uint32
    val mutable ClassGuid : Guid
    val mutable DevInst : uint32
    val mutable Reserved : nativeint

module private Kernel32 =
    [<DllImport("Kernel32", CharSet = CharSet.Unicode)>]
    extern SafeFileHandle CreateFile(
        string lpFileName,
        uint32  dwDesiredAccess,
        uint32 dwShareMode,
        nativeint lpSecurityAttributes,
        uint32 dwCreationDisposition,
        uint32 dwFlagsAndAttributes,
        nativeint hTemplateFile)

    [<DllImport("Kernel32", CharSet = CharSet.Unicode)>]
    extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint32 nNumberOfBytesToWrite,
        uint32& lpNumberOfBytesWritten,
        nativeint lpOverlapped)

let createFile (path : string) : SafeFileHandle =
    Kernel32.CreateFile(path,
                        GENERIC_READ ||| GENERIC_WRITE,
                        FILE_SHARE_READ ||| FILE_SHARE_WRITE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0u,
                        IntPtr.Zero)

let writeFile (file : SafeFileHandle) (data : byte[]) : unit =
    let mutable writtenBytes = 0u
    let result = Kernel32.WriteFile(file,
                                    data,
                                    uint32 data.Length,
                                    &writtenBytes,
                                    IntPtr.Zero)
    if not result then throwLastWin32Error()

module private SetupAPI =
    type CONFIGRET = int
    let CR_SUCCESS = 0

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern CONFIGRET CM_Get_Device_ID(
        uint32 dnDevInst,
        StringBuilder Buffer,
        uint32 BufferLen,
        uint32 ulFlags)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern CONFIGRET CM_Get_Device_ID_Size(
        uint32& pulLen,
        uint32 dnDevInst,
        uint32 ulFlags)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern nativeint SetupDiGetClassDevs(
        Guid& ClassGuid,
        string Enumerator,
        nativeint hwndParent,
        uint32 Flags)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern bool SetupDiGetDeviceInterfaceDetail(
        nativeint DeviceInfoSet,
        SP_DEVICE_INTERFACE_DATA& DeviceInterfaceData,
        nativeint DeviceInterfaceDetailData,
        uint32 DeviceInterfaceDetailDataSize,
        uint32& RequiredSize,
        nativeint DeviceInfoData)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern bool SetupDiEnumDeviceInterfaces(
        nativeint DeviceInfoSet,
        SP_DEVINFO_DATA& DeviceInfoData,
        Guid& InterfaceClassGuid,
        uint32 MemberIndex,
        SP_DEVICE_INTERFACE_DATA& DeviceInterfaceData)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern bool SetupDiEnumDeviceInfo(
        nativeint DeviceInfoSet,
        uint32 MemberIndex,
        SP_DEVINFO_DATA& DeviceInfoData)

    [<DllImport("Setupapi", CharSet = CharSet.Unicode)>]
    extern bool SetupDiDestroyDeviceInfoList(nativeint DeviceInfoSet)

let cmGetDeviceId (device : SP_DEVINFO_DATA) : string =
    let devInst = device.DevInst
    let mutable idLength = 0u
    let lenResult = SetupAPI.CM_Get_Device_ID_Size(&idLength, devInst, 0u)
    if lenResult <> SetupAPI.CR_SUCCESS
    then failwithf "CM_Get_Device_ID_Size error: %d" lenResult

    let buffer = StringBuilder(int idLength + 1) // 1 for terminating zero
    let idResult = SetupAPI.CM_Get_Device_ID(devInst, buffer, uint32 buffer.Capacity, 0u)
    if idResult <> SetupAPI.CR_SUCCESS
    then failwithf "CM_Get_Device_ID error: %d" idResult

    buffer.ToString()

let sizeFromSetupDiGetDeviceInterfaceDetail (deviceInfoSet : nativeint)
                                            (interfaceData : SP_DEVICE_INTERFACE_DATA) : uint32 =
    let mutable deviceInterfaceData = interfaceData
    let mutable requiredSize = 0u
    SetupAPI.SetupDiGetDeviceInterfaceDetail(deviceInfoSet,
                                             &deviceInterfaceData,
                                             IntPtr.Zero,
                                             0u,
                                             &requiredSize,
                                             IntPtr.Zero)
    |> ignore
    requiredSize

let pathFromSetupDiGetDeviceInterfaceDetail (deviceInfoSet : nativeint)
                                            (interfaceData : SP_DEVICE_INTERFACE_DATA)
                                            (buffer : SpDeviceInterfaceDetailData.MemoryBuffer)
                                            : string =
    let mutable deviceInterfaceData = interfaceData
    let mutable requiredSize = 0u
    let pointer = buffer.Pointer
    let result = SetupAPI.SetupDiGetDeviceInterfaceDetail(deviceInfoSet,
                                                          &deviceInterfaceData,
                                                          pointer,
                                                          uint32 buffer.Size,
                                                          &requiredSize,
                                                          IntPtr.Zero)
    if not result then throwLastWin32Error()

    SpDeviceInterfaceDetailData.getStringContent buffer

let setupDiGetClassDevs () : nativeint =
    let mutable classGuid = GUID_DEVINTERFACE_HID
    let flags = DIGCF_PRESENT ||| DIGCF_DEVICEINTERFACE
    let result = SetupAPI.SetupDiGetClassDevs(&classGuid, null, IntPtr.Zero, flags)
    if result = INVALID_HANDLE_VALUE then throwLastWin32Error()
    result

let setupDiEnumDeviceInterfaces (deviceInfoSet : nativeint)
                                (deviceInfoData : SP_DEVINFO_DATA) : SP_DEVICE_INTERFACE_DATA =
    let mutable mutableDeviceInfoData = deviceInfoData
    let mutable interfaceClassGuid = GUID_DEVINTERFACE_HID
    let mutable interfaceData =
        SP_DEVICE_INTERFACE_DATA(cbSize = uint32 sizeof<SP_DEVICE_INTERFACE_DATA>)
    let result = SetupAPI.SetupDiEnumDeviceInterfaces(deviceInfoSet,
                                                      &mutableDeviceInfoData,
                                                      &interfaceClassGuid,
                                                      0u,
                                                      &interfaceData)
    if not result then throwLastWin32Error()
    interfaceData

let setupDiDestroyDeviceInfoList (deviceInfoSet : nativeint) : unit =
    let result = SetupAPI.SetupDiDestroyDeviceInfoList deviceInfoSet
    if not result then throwLastWin32Error()

let setupDiEnumDeviceInfo (deviceInfoSet : nativeint)
                          (memberIndex : uint32) : SP_DEVINFO_DATA option =
    let mutable data = SP_DEVINFO_DATA(cbSize = uint32 sizeof<SP_DEVINFO_DATA>)
    if SetupAPI.SetupDiEnumDeviceInfo(deviceInfoSet, memberIndex, &data)
    then Some data
    else None
