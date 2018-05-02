module AbSyn
open System

type Exp =
  | Constant  of int
  | Var       of string
  | Plus      of Exp * Exp
  | Minus     of Exp * Exp
  | Times     of Exp * Exp
  | Divide    of Exp * Exp
  | Modulo    of Exp * Exp
  | Not       of Exp
  | Less      of Exp * Exp
  | GreatEq   of Exp * Exp
  | Equal     of Exp * Exp
  | NotEqual  of Exp * Exp

type Stat = 
  | Incr      of string * Exp
  | Decr      of string * Exp
  | Concat    of Stat * Stat
  | Call      of string
  | Uncall    of string
  | If        of Exp * Stat * Stat * Exp
  | Repeat    of Stat * Exp
  | Print     of string
  | Read      of string
  | Local     of string * Exp * Stat * Exp

type Def = 
  | Proc      of string * Stat

type Prog = Stat * (Def list)

