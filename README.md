# FastFileManagement

FastFileManagement is a Powershell module written in C#, currently it only contains the "Get-ChildItemFast" cmdlet which is a faster but more limited version of Get-ChildItem (Only works on filesystems, doesn't have some of the switches that filters out certain attributes, warnings instead of error exceptions.)

Comparison:

```
Measure-Command -Expression {$null=Get-ChildItem -Path C:\ -Force -Recurse}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 30
Milliseconds      : 623
Ticks             : 306235114
TotalDays         : 0,000354438789351852
TotalHours        : 0,00850653094444444
TotalMinutes      : 0,510391856666667
TotalSeconds      : 30,6235114
TotalMilliseconds : 30623,5114

Measure-Command -Expression {$null=Get-ChildItemFast -Path C:\ -Recurse}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 11
Milliseconds      : 889
Ticks             : 118896812
TotalDays         : 0,000137612050925926
TotalHours        : 0,00330268922222222
TotalMinutes      : 0,198161353333333
TotalSeconds      : 11,8896812
TotalMilliseconds : 11889,6812
```

Other cmdlets I've thought about making for this module include Copy/Move-ItemFast, and maybe Remove-ItemFast, but it depends on how much speed there is to gain by writing my own versions of them.
