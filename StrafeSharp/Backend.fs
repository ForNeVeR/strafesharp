/// Backend sends Datagram to the actual keyboard.
module StrafeSharp.Backend

open StrafeSharp.Platform

let InitializeKeyboard () : IStrafeKeyboard =

    upcast Windows.openKeyboard()
