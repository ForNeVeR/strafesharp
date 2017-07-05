/// Frontend is a module that converts keyboard state to the Datagram that could
/// be sent to the keyboard by Backend.
module StrafeSharp.Frontend

open System

let packetSize = 64

let private colorPacket number values =
    Array.concat <| seq {
        yield [| 127uy; byte number; 60uy; 0uy |]
        yield [| for b in 60 * number .. (60 * (number + 1)) - 1 do
                     yield (if b < Array.length values
                         then Array.get values b
                         else 0uy) |]
     }

let resize = Utils.resizeTo packetSize

let private staticPacket = function
    | 3 -> resize [| 7uy; 40uy; 1uy; 3uy; 1uy; |]
    | 7 -> resize [| 7uy; 40uy; 2uy; 3uy; 1uy; |]
    | 11 -> resize [| 7uy; 40uy; 3uy; 3uy; 2uy; |]
    | o -> failwithf "Don't know how to generate static packet number %d" o

let private generatePackage state = function
    | x when x >= 0 && x <= 2 -> colorPacket x state.RedValues
    | x when x = 3 || x = 7 || x = 11 -> staticPacket x
    | x when x >= 4 && x <= 6 -> colorPacket (x - 4) state.GreenValues
    | x when x >= 8 && x <= 10 -> colorPacket (x - 8) state.BlueValues
    | o -> failwithf "Don't know how to generate packet number %d" o

let Render (state : KeyboardState) : Datagram =
    { Packets = [| for index in 0..11 do yield generatePackage state index |] }
