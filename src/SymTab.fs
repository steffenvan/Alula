module SymTab

open System
open System.Collections.Generic

type SymTab = 
    {   Global: Dictionary<string, int>;
        Local: Dictionary<string, int>  }
let empty () = { Global = new Dictionary<string, int>();
                 Local  = new Dictionary<string, int>() }

let rec lookup var (tab : SymTab) = 
    if tab.Local.ContainsKey(var) 
    then tab.Local.[var]
    elif tab.Global.ContainsKey(var)
    then tab.Global.[var]
    else 0
 
let bind var value (tab : SymTab) = 
    if tab.Local.ContainsKey(var)
    then tab.Local.[var] <- value
    else tab.Global.[var] <- value

let bindLocal var value (tab:  SymTab) = 
    tab.Local.[var] <- value

let removeLocal var (tab: SymTab) = 
    if tab.Local.Remove(var) 
    then ()
    else failwith "Key was not removed. Did not exists or something bad happened."
let call (tab: SymTab) = 
    let newSym = { Global = tab.Global; 
                   Local  = new Dictionary<string, int>() }
    newSym