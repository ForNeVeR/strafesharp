namespace StrafeSharp.Platform.Windows

type internal DeviceId = DeviceId of vid : uint32 * pid : uint32 * mi : uint32
    with static member CorsairStrafe = DeviceId(0x1b1cu, 0x1b20u, 0x3u)
