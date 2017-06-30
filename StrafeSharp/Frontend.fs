/// Frontend is a module that converts keyboard state to the Datagram that could
/// be sent to the keyboard by Backend.
module StrafeSharp.Frontend

let Render (state : KeyboardState) : Datagram =
    { Packets = Array.zeroCreate 12 }
