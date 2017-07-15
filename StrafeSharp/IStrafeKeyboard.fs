namespace StrafeSharp

open System

type IStrafeKeyboard =
    inherit IDisposable
    abstract member SendData : Datagram -> unit
