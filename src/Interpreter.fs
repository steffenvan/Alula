module Interpreter

open System.Collections.Generic
open Microsoft.FSharp.Collections

open System
open AbSyn
open SymTab

exception MyError of string
type FunTable = Map<string, Stat>

let boolToInt = function
    | true  -> 1
    | false -> 0

let rec evalExp (e: Exp, tab : SymTab) = 
    match e with
      | Constant (var)  -> var
      | Var (id) -> 
            let res = SymTab.lookup id tab 
            res                                    
      | Plus(e1, e2)  ->
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            res1 + res2
      | Minus(e1, e2) -> 
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            res1 - res2
      | Times(e1, e2) -> 
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            res1 * res2
      | Divide(e1, e2) -> 
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            res1 / res2
      | Modulo(e1, e2) -> 
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            res1 % res2
      | Less (e1, e2) ->  
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            boolToInt (res1 < res2)
      | GreatEq (e1, e2) ->  
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            boolToInt (res1 >= res2)
      | Equal (e1, e2) ->  
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            boolToInt (res1 = res2)
      | NotEqual (e1, e2) ->  
            let res1 = evalExp (e1, tab)
            let res2 = evalExp (e2, tab)
            boolToInt (res1 <> res2)
      | Not (e) ->  
            let res1 = evalExp (e, tab)
            boolToInt (res1 = 0)

// Check that a variable x is not used in an expression
// in which x is updated. 
let rec notContains (var: string, e: Exp) =
    match e with
      | Var      (id)     -> id <> var
      | Constant (var)    -> true
      | Plus     (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Minus    (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Times    (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Divide   (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Modulo   (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Less     (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | GreatEq  (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Equal    (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | NotEqual (e1, e2) -> notContains (var, e1) && notContains(var, e2)
      | Not      (e)      -> notContains (var, e)

// Reverses a statement to its opposition. 
let rec revStat = function
    | Incr (var, exp)        -> Decr (var, exp)
    | Decr (var, exp)        -> Incr (var, exp)
    | Concat (stat1, stat2)  -> Concat (revStat stat2, revStat stat1)
    | Call (proc)            -> Uncall (proc)
    | Uncall (proc)          -> Call (proc)
    | If (e1, s1, s2, e2)    -> If (e2, revStat s1, revStat s2, e1)
    | Repeat (stat, exp)     -> Repeat (revStat stat, exp)
    | Print (var)            -> Read (var)
    | Read (var)             -> Print(var)
    | Local (var, e1, s, e2) -> Local (var, e2, revStat s, e1)

let rec evalStat (s: Stat, tab : SymTab, ftab : FunTable) = 
    match s with 
      | Incr (var, e1) -> 
            let res1 = SymTab.lookup var tab
            let res2 = evalExp (e1, tab)
            if notContains (var, e1) 
            then SymTab.bind var (res1+res2) tab
            else failwith "Variable cannot be used in expression"
      | Decr (var, e1) -> 
            let res1 = SymTab.lookup var tab
            let res2 = evalExp (e1, tab)
            if notContains (var, e1) 
            then SymTab.bind var (res1-res2) tab
            else failwith "Variable cannot be used in expression"
      | Concat (stat1, stat2) -> 
            evalStat (stat1, tab, ftab) 
            evalStat (stat2, tab, ftab)
      | Call (proc) -> 
            evalProc (proc, tab, ftab)
      | Uncall (proc) -> 
            uncallFun (proc, tab, ftab)
      | If (e1, s1, s2, e2) -> 
            let cond = evalExp (e1, tab) <> 0
            if cond
            then evalStat (s1, tab, ftab) 
            else evalStat (s2, tab, ftab) 
            let fi = evalExp (e2, tab) <> 0
            if fi <> cond 
            then failwith "fi is not equal to start condition"
            else ()
      | Repeat (s, e) -> 
            let cond = evalExp (e, tab) <> 0
            if not cond
            then failwith "Start condition is false"
            else evalStat(s, tab, ftab) 
                 while not (evalExp (e, tab) <> 0) do evalStat(s, tab, ftab) 
      | Print (var) -> 
            let res = SymTab.lookup var tab
            printfn "%i" res
            SymTab.bind var 0 tab
      | Read (var) ->
            let readVar = SymTab.lookup var tab
            if readVar <> 0 
            then failwith "Variable is not 0"
            else let newVar = Convert.ToInt32(Console.ReadLine())
                 SymTab.bind var newVar tab
      | Local (var, e1, s1, e2) -> 
            let res1 = evalExp (e1, tab)
            SymTab.bindLocal var res1 tab
            // printfn "%A" tab 
            evalStat(s1, tab, ftab)
            let res2 = evalExp (e2, tab)
            let localVar = SymTab.lookup var tab 
            if localVar <> res2
            then failwith "Exit expression is not equal to variable value"
            else SymTab.removeLocal var tab

// Function to evaluate procedures. 
// It only has access to the global symbol table.
and evalProc (procName: string, tab: SymTab, ftab : FunTable) = 
    let res = Map.find procName ftab
    let globalTab = SymTab.call tab
    evalStat (res, globalTab, ftab)

// Function to call a procedure backwards. 
// It only has access to the global symbol table.
and uncallFun (procName: string , tab: SymTab, ftab : FunTable) = 
    let res = Map.find procName ftab
    let globalTab = SymTab.call tab
    evalStat (revStat res, globalTab, ftab)

// Main is the call to the procedure
// defs contains all the statements and expressions to be executed. 
let evalProg (prog: Prog, reversed: bool) =
    let main, defs = prog     
    let symtab = SymTab.empty()
    let stringTup = defs |> List.map (fun (Proc (n, s)) -> (n, s))
    let ftab = Map.ofList(stringTup)
    if reversed  
    then evalStat (revStat main, symtab, ftab)
         printfn "%A" symtab
    else evalStat (main, symtab, ftab)
         printfn "%A" symtab
    for key in symtab.Global do
        if key.Value <> 0
        then failwith "Not all variables were set to 0"
        else ()
