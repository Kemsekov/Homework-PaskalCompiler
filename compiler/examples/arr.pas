var
   variable2 : float;
   threedim, another: array[1..10, 2..10, 1..4] of array[1..2] of integer;
   // invalid_arr : array[1..3] of char;
   variable3 : string;
   // variable2 : char;
   // another : integer;
   // invalid_type : abc;
begin
   variable2 := -2+23.0*3-1.0/(52*13-4) + (23=9)>variable2-True;
   variable2 := variable2+3;
   variable3 := variable3+3;
end;