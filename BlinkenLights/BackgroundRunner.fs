module ThrottlingAgent

/// Message type used by the agent - contains queueing 
/// of work items and notification of completion.
type internal ThrottlingAgentMessage = 
  | Completed
  | Work of Async<unit>
    
/// Represents an agent that runs operations in the background, one at a time.
type BackgroundRunner() = 
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
          async { try do! work 
                  finally agent.Post(Completed) }
          |> Async.Start
          return! waiting () }

    working())      

  /// Queue the specified asynchronous workflow for processing
  member x.DoWork(work) = agent.Post(Work work)