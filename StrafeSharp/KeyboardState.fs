namespace StrafeSharp

type KeyboardState =
    { RedValues : byte[]
      GreenValues : byte[]
      BlueValues : byte[] }
    with
        static member KeyCount = 144
        static member Empty =
            { RedValues = Array.zeroCreate KeyboardState.KeyCount
              GreenValues = Array.zeroCreate KeyboardState.KeyCount
              BlueValues = Array.zeroCreate KeyboardState.KeyCount }
