# FastFileManagement

Get-ChildItem is a general purpose cmdlet that works with non filesystem providers like the registry.

Get-ChildItemFast is a faster alternative made specifically for grabbing the same information that Get-ChildItem does for the filesystem.

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

For now it lacks some of the features that Get-ChildItem has like only showing files with specific attributes, or filtering out hidden files
but the returned data should be identical.
