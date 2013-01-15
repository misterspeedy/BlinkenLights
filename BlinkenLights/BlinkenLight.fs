namespace BlinkenLights

open Blink1Lib
open System
open System.Drawing
open BackgroundRunner

/// Extensions to blink(1)'s Blink1() class.
type BlinkenLight() = 
    inherit Blink1()
    let mutable RGB = 0uy, 0uy, 0uy
    let bgr = new BackgroundRunner()

    member private bl.doSetRGB(R : byte, G : byte, B : byte) = 
        RGB <- R, G, B
        base.setRGB(R |> int, G |> int, B |> int)

    /// Sets color as an RGB color, noting the setting so that is is readable later.
    member bl.SetRGB(R : byte, G : byte, B : byte) =
        bl.doSetRGB(R, G, B)

    /// Overrides inherited setRGB to ensure that we always keep a copy of the RGB setting.
    member bl.setRGB = bl.SetRGB

    /// Sets the color as a System.Drawing.Color, noting the setting so that is is readable later.
    member bl.SetColor(color : Color) =
        bl.SetRGB(color.R, color.G, color.B)

    /// Fades to the specified RGB color over the specified period.
    member bl.FadeToRGB(R : byte, G : byte, B : byte, ms : int) =
        RGB <- R, G, B
        base.fadeToRGB(ms, R |> int, G |> int, B |> int)
        
    /// Fades to the specified System.Drawing.Color over the specified period.
    member bl.FadeToColor(color : System.Drawing.Color, ms : int) =
        bl.FadeToRGB(color.R, color.G, color.B, ms)

    /// Overrides inherited setRGB to ensure that we always keep a copy of the RGB setting (also we have a
    /// different argument order here from the base class).
    member bl.fadeToRGB = bl.FadeToRGB

    /// Cycles between the specified colors, keeping each color on for the specified duration.
    /// Lengths of colors and duration arrays can differ; values are used successively from
    /// each array.  Stop cycling by calling Cancel().
    member bl.Cycle(colors : array<Color>, mss : array<int>) =
        if colors.Length = 0 then
            raise (new ArgumentException("Colors array cannot be empty"))
        if mss.Length = 0 then
            raise (new ArgumentException("Mss array cannot be empty"))
        bgr.DoWork(async {
                            let colorIndex = ref 0
                            let msIndex = ref 0
                            while true do
                                let color = colors.[colorIndex.Value]
                                let ms = mss.[msIndex.Value]
                                bl.SetColor(color)
                                do! Async.Sleep(ms)
                                colorIndex := (colorIndex.Value + 1) % colors.Length
                                msIndex := (msIndex.Value + 1) % mss.Length
                        })

    /// Changes the RGB color for the specified period, then reverts to the previous color.
    /// (Does not block the calling thread.)
    member bl.Flash(R : byte, G : byte, B : byte, ms : int) =
        bl.FlashCount(1, R, G, B, ms, 0)

    /// Changes the System.Drawing.Color for the specified period, then reverts to the previous color.
    /// (Does not block the calling thread.)
    member bl.Flash(color : Color, ms : int) =
        bl.Flash(color.R, color.G, color.B, ms)

    /// Alternates between the specified RGB color and the previously set color,
    /// keeping each color on for the specified lengths of time, and repeating until
    /// the condition function returns true.  (Does not block the calling thread.)
    member bl.FlashUntil(condition : unit -> bool, R : byte, G : byte, B : byte, onMs : int, offMs : int) =
        bgr.DoWork(async {
                            while not (condition()) do
                                let prevRGB = RGB
                                bl.doSetRGB(R, G, B)
                                do! Async.Sleep(onMs)
                                bl.doSetRGB(prevRGB)
                                do! Async.Sleep(offMs)
                        })

    /// Alternates between the specified System.Drawing.Color color and the previously set color,
    /// keeping each color on for the specified lengths of time, and repeating until
    /// the condition function returns true.  (Does not block the calling thread.)
    member bl.FlashUntil(condition : unit -> bool, color : Color, onMs : int, offMs : int) =
        bl.FlashUntil(condition, color.R, color.G, color.B, onMs, offMs)

    /// Alternates between the specified RGB color and the previously set color,
    /// keeping each color on for the specified lengths of time, and repeating for 
    /// the specified number of iterations.  (Does not block the calling thread.)
    member bl.FlashCount(count : int, R : byte, G : byte, B : byte, onMs : int, offMs : int) =
        let countDone() =
            let counter = ref 0
            fun () -> counter := counter.Value + 1
                      counter.Value > count
        bl.FlashUntil(countDone(), R, G, B, onMs, offMs)

    /// Alternates between the specified System.Drawing.Color and the previously set color,
    /// keeping each color on for the specified lengths of time, and repeating for 
    /// the specified number of iterations.  (Does not block the calling thread.)
    member bl.FlashCount(count : int, color : Color, onMs : int, offMs : int) =
        bl.FlashCount(count, color.R, color.G, color.B, onMs, offMs)

    /// Cancels any currently runnng Flash... operation.
    member bl.CancelFlashing() =
        bgr.Cancel()