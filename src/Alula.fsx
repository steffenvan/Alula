open System.Text
open System.IO
open Microsoft.FSharp.Text.Lexing
open Microsoft.FSharp.Text.Parsing
open Microsoft.FSharp.Collections
open AbSyn
open Interpreter
open SymTab

exception FileProblem of string
let usage =
    [ "   bin/alula FILENAME.ala\n"
    ; "   this interprets the file and prints the results.\n"
    ]
// Print error message to the standard error channel.
let errorMessage (message : string) : Unit =
    printfn "%s\n" message

let bad () : Unit =
    errorMessage "Unknown command-line arguments. The correct usage is:"
    errorMessage (usage |> List.fold (+) "")

// Function that lexes and parses a program. 
let parseString (s : string) =
    Parser.Prog Lexer.Token
    <| LexBuffer<_>.FromBytes (Encoding.UTF8.GetBytes s)

// Read text from a file given as parameter with an added extension
let parseFile (filename : string) =
  let txt = 
    try  
      let inStream = File.OpenText (filename + ".ala")
      let txt = inStream.ReadToEnd()
      inStream.Close()
      txt
    with
      | ex -> ""
  if txt = "" 
  then failwith "Invalid file name or empty file"
  else parseString txt

let sanitiseFilename (filename : string) : string =
    if filename.EndsWith ".ala"
    then filename.Substring(0, (String.length filename)-4)
    else filename
    

// This also checks if a program is waiting for input from user. 
// E.g. if it is executed forwards and contains a read,
// or backwards and contains a print. 
let interpret (filename : string) =
    printfn "Choose which direction the program should be executed (forwards or backwards):"
    let direction = System.Console.ReadLine().ToLower()
    let program = parseFile filename    

    if direction = "backwards" && program.ToString().Contains("Print")
    then printfn "The program is waiting for input, enter value: "
         Interpreter.evalProg (program, true)

    elif direction = "forwards" && program.ToString().Contains("Read")
    then printfn "The program is waiting for input, enter value: "
         Interpreter.evalProg (program, false)

    elif direction = "backwards"
    then Interpreter.evalProg (program, true)

    elif direction = "forwards"
    then Interpreter.evalProg (program, false)

    else printfn "Invalid direction"

[<EntryPoint>]
let main (prog: string[]) =
  try
    match prog with
      | [|file|] -> interpret (sanitiseFilename file)
      | _ -> bad ()
    0
  with
    | Interpreter.MyError (message) ->
        errorMessage ("Interpreter error" + message)
        System.Environment.Exit 1
        1
    | FileProblem filename ->
        errorMessage ("There was a problem with the file: " + filename)
        System.Environment.Exit 1
        1

