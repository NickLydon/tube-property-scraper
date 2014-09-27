module Seq
    //Unlike Seq.windowed, this will not overlap elements, i.e. whereas Seq.windowed would return
    //[1;2];[2;3] given input of [1;2;3;] and size of 2, this will give
    //[1;2];[3] for the same input
    let windowedExclusive size sequence =
        let rec loop sequence =
            seq {
                if size > 0 && Seq.empty <> sequence then
                    if (Seq.length sequence <= size) 
                    then yield sequence 
                    else                        
                        yield (sequence |> Seq.take size)
                        yield! loop (sequence |> Seq.skip size)
            }
        loop sequence