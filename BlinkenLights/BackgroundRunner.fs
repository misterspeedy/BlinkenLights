module ThrottlingAgent

/// Message type used by the agent - contains queueing 
/// of work items and notification of completion 
type internal ThrottlingAgentMessage = 
  | Completed
  | Work of Async<unit>
    
/// Represents an agent that runs operations in concurrently. When the number
/// of concurrent operations exceeds 'limit', they are queued and processed later
type BackgroundRunner() = 
  let agent = MailboxProcessor.Start(fun agent -> 

    /// Represents a state when the agent is blocked
    let rec waiting () = 
      // Use 'Scan' to wait for completion of some work
      agent.Scan(function
        | Completed -> Some(working())
        | _ -> None)

    /// Represents a state when the agent is working
    and working() = async { 
      // Receive any message 
      let! msg = agent.Receive()
      match msg with 
      | Completed -> 
          return! working()
      | Work work ->
          // Start the work item & continue in blocked/working state
          async { try do! work 
                  finally agent.Post(Completed) }
          |> Async.Start
          return! waiting () }

    // Start in working state with zero running work items
    working())      

  /// Queue the specified asynchronous workflow for processing
  member x.DoWork(work) = agent.Post(Work work)