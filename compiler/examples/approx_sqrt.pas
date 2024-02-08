// A program to find approximate value of square root of a real number in Pascal
var
  x, y, e: real;
begin
  // Read the input number
  write('Enter a positive number: ');
  readln(x);
  // Check if the input is valid
  if x < 0 then
    writeln('Invalid input')
  else
  begin
    // Initialize y as half of x
    y := x / 2;
    // Initialize e as a small positive number
    e := 0.00001;
    // Repeat until the difference between y and x/y is less than e
    while abs(y - x / y) > e do
    begin
      (*Update y using the average of y and x/y*)
      y := (y + x / y) / 2;
    end;
    {Print the approximate square root of x}
    writeln('The square root of ', x, a' is ', y);
  end;
end.