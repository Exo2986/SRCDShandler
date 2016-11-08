# SRCDShandler
A fairly simple C# program to protect Source Dedicated Servers from crashes.

# SRCDShandler Commands
```
srcdspath path
```
Change the SRCDS.exe path that the handler uses.
```
srcdsargs args
```
Change the launch args to be used when launching SRCDS.
```
srcdsrestart
```
Restart SRCDS.

```
quit
```
Closes the program and closes SRCDS if it is running.

```
printvalue commandname
```
Prints the current value of the specified command if it has a value linked to it to the console. For example, typing `printvalue srcdsargs` would print the current launch arguments set by `srcdsargs`.
