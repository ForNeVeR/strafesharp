namespace StrafeSharp

type KeyboardState =
    { RedValues : byte[]
      GreenValues : byte[]
      BlueValues : byte[] }
    with
        static member KeyCount = 144
        static member Empty =
            { RedValues = Array.create KeyboardState.KeyCount 7uy
              GreenValues = Array.create KeyboardState.KeyCount 7uy
              BlueValues = Array.create KeyboardState.KeyCount 7uy }
