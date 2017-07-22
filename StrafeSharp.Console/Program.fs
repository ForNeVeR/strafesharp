open System

open StrafeSharp

[<EntryPoint>]
let main argv =
    use keyboard = Backend.InitializeKeyboard()
    let state = KeyboardState.Empty
    let datagram = Frontend.Render state
    keyboard.SendData datagram
    0
