

var a :Int,
    b: Real,
    c : String;

begin
    fine_int_c := 234
    overflow_int_c:=-102039

    fine_float_c:= 0.02
    wrong_float_c:= 0.02.23

    c:='Good stringc'
    c:='unclosed string c
    c:= another unclosed string c'

    //add forbidden symbols
    @#$

    a.CallFunction();
    }
    *)
    {
        multiline
        comment
    }

    (* Short comment! *)




    for I:=1 to 10 do a:=a*0.5;

    //check that comments are skippable in the middle of code
    if {skip this part} (* also this part *) a then 
        b:=a+b
    endif

end
{
    Multiline unclosed comment
(* unclosed short comment!

