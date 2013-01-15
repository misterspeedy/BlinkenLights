open System
open BlinkenLights
open System.Drawing
open System.Diagnostics

let wait() =    
    Console.WriteLine("(press a key)")
    Console.ReadKey(false) |> ignore

[<EntryPoint>]
let main argv = 
    let blinkenLight = BlinkenLight()
    blinkenLight.``open``() |> ignore
    
    Console.WriteLine("Setting 255, 50, 50")
    blinkenLight.SetRGB(255uy, 50uy, 50uy)
    wait()

    Console.WriteLine("Setting PeachPuff")
    blinkenLight.SetColor(Color.PeachPuff)
    wait()

    Console.WriteLine("Fading to Crimson over 5 seconds")
    blinkenLight.FadeToColor(Color.Crimson, 5000)
    wait()

    Console.WriteLine("Flashing through colors of the rainbow, one per sec, finishing on black.")
    blinkenLight.SetColor(Color.Black)
    [Color.Red; Color.Orange; Color.Yellow; Color.Green; Color.Blue; Color.Indigo; Color.Violet]
    |> List.iter (fun col -> blinkenLight.Flash(col, 1000))
    Console.WriteLine("(Main thread has continued.)")
    wait()

    Console.WriteLine("Flashing orange/black for five seconds, or until you hit a key.")
    blinkenLight.SetColor(Color.Black)

    let haveElapsed ms =
        let start = DateTime.Now
        fun () -> let current = DateTime.Now
                  (current - start).TotalMilliseconds > ms

    blinkenLight.FlashUntil((haveElapsed 5000.), Color.Orange, 700, 400) 
    Console.WriteLine("(Main thread has continued.)")
    wait()
    blinkenLight.CancelFlashing()

    Console.WriteLine("Flashing rapidly red/blue 50 times, or until you hit a key.")
    blinkenLight.SetColor(Color.Red)
    blinkenLight.FlashCount(50, Color.Blue, 100, 100) 
    Console.WriteLine("(Main thread has continued.)")
    wait()
    blinkenLight.CancelFlashing()

    Console.WriteLine("Acting as a metronome (3/4 time)")
    blinkenLight.Cycle([|Color.GreenYellow
                         Color.Black
                         Color.Green
                         Color.Black
                         Color.Green
                         Color.Black|],
                       [|250|])
    wait()
    blinkenLight.CancelFlashing()

    Console.WriteLine("Acting as a metronome (4/4 time)")
    blinkenLight.Cycle([|Color.GreenYellow
                         Color.Black
                         Color.DarkGoldenrod
                         Color.Black
                         Color.Green
                         Color.Black
                         Color.DarkGoldenrod
                         Color.Black|],
                       [|250|])
    wait()
    blinkenLight.CancelFlashing()

    blinkenLight.SetColor(Color.Black)  

    let cpuCounter = new PerformanceCounter(CategoryName="Processor", CounterName="% Processor Time", InstanceName = "_Total")
    let diskCounter = new PerformanceCounter(CategoryName="PhysicalDisk", CounterName="Avg. Disk Queue Length", InstanceName = "_Total")
    
    Console.WriteLine("Showing CPU loading and disk access (close the console window to finish).")
    while true do
        let cpu, disk = cpuCounter.NextValue(), cpuCounter.NextValue()
        let scaledCpu = ((min (cpu * 2.55f) 255.f)) |> byte
        blinkenLight.SetRGB(0uy, scaledCpu, 0uy)
        if disk > 0.f then
            blinkenLight.Flash(Color.Red, 10)
        Threading.Thread.Sleep(500)

    0 
