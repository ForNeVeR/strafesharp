module internal StrafeSharp.Platform.Windows.Native

open System
open System.Runtime.InteropServices

open Microsoft.Win32.SafeHandles

let GUID_DEVINTERFACE_HID : Guid = Guid.ParseExact("{4D1E55B2-F16F-11CF-88CB-001111000030}", "B")

let DIGCF_PRESENT : uint32 = 0x2u
let DIGCF_DEVICEINTERFACE : uint32 = 0x10u

let private INVALID_HANDLE_VALUE = nativeint -1

let private throwLastWin32Error () = Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())

#nowarn "9" // compiler warns on StructLayout; that's unnecessary

[<StructLayout(LayoutKind.Sequential)>]
[<Struct>]
type SP_DEVINFO_DATA =
    val mutable cbSize : uint32
    val mutable ClassGuid : Guid
    val mutable DevInst : uint32
    val mutable Reserved : nativeint

module private SetupAPI =
    [<DllImport("SetupAPI")>]
    extern nativeint SetupDiGetClassDevs(
        Guid& ClassGuid,
        string Enumerator,
        nativeint hwndParent,
        uint32 Flags)

    [<DllImport("SetupAPI")>]
    extern bool SetupDiEnumDeviceInfo(
        nativeint DeviceInfoSet,
        uint32 MemberIndex,
        SP_DEVINFO_DATA& DeviceInfoData)

    [<DllImport("SetupAPI")>]
    extern bool SetupDiDestroyDeviceInfoList(nativeint DeviceInfoSet)

let setupDiGetClassDevs (classGuid : Guid) (flags : uint32) : nativeint =
    let mutable classGuidMutable = classGuid
    let result = SetupAPI.SetupDiGetClassDevs(&classGuidMutable, null, IntPtr.Zero, flags)
    if result = INVALID_HANDLE_VALUE then throwLastWin32Error()
    result

let setupDiDestroyDeviceInfoList (deviceInfoSet : nativeint) : unit =
    let result = SetupAPI.SetupDiDestroyDeviceInfoList deviceInfoSet
    if not result then throwLastWin32Error()

let setupDiEnumDeviceInfo (deviceInfoSet : nativeint)
                          (memberIndex : uint32) : SP_DEVINFO_DATA option =
    let mutable data = SP_DEVINFO_DATA(cbSize = uint32 sizeof<SP_DEVINFO_DATA>)
    if SetupAPI.SetupDiEnumDeviceInfo(deviceInfoSet, memberIndex, &data)
    then Some data
    else None

module private Kernel32 =
    [<DllImport("Kernel32")>]
    extern bool WriteFile(
        SafeFileHandle hFile,
        byte[] lpBuffer,
        uint32 nNumberOfBytesToWrite,
        uint32& lpNumberOfBytesWritten,
        nativeint lpOverlapped)

let writeFile (file : SafeFileHandle) (data : byte[]) : unit =
    let mutable writtenBytes = 0u
    let result = Kernel32.WriteFile(file,
                                    data,
                                    uint32 data.Length,
                                    &writtenBytes,
                                    IntPtr.Zero)
    if not result then throwLastWin32Error()
