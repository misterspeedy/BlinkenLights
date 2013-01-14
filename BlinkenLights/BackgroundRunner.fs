module BackgroundRunner

open System.Threading

/// Message type used by the agent - contains queueing 
/// of work items and notification of completion.
type internal ThrottlingAgentMessage = 
  | Completed
  | Work of Async<unit>
    
/// Represents an agent that runs operations in the background, one at a time.
type BackgroundRunner() = 
  let mutable cts = new CancellationTokenSource()

  let agent = MailboxProcessor.Start(fun agent -> 

    let rec waiting () = 
      agent.Scan(function
        | Completed -> Some(working())
        | _ -> None)

    and working() = async { 
      let! msg = agent.Receive()
      match msg with 
      | Completed -> 
          return! working()
      | Work work ->
          let asyn = async { try do! work 
                             finally agent.Post(Completed) }
          Async.Start(asyn, cts.Token)
          return! waiting () }

    working())      

  /// Queues the specified asynchronous workflow for processing.
  member x.DoWork(work) = agent.Post(Work work)

  /// Cancel the current operation
  member x.Cancel() = 
    cts.Cancel()
    // Respawn the cancellation source as it has been 'used up':
    cts <- new CancellationTokenSource() 