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

# SRCDShandler Launch Arguments

```
-srcdspath "path"
```
Makes the program use the specified path for the current session. This will not work unless there are quotes around the path.

```
-srcdsargs "args"
```
Makes the program use the specified launch arguments when launching SRCDS.exe for the current session. This will not work unless there are quotes around the arguments.

## Usage Example

  A good practice when using SRCDShandler is to create a shortcut to the SRCDShandler executable and supply the `-srcdspath "path"` and/or `-srcdsargs "args"` arguments at the end of the target of the shortcut (You can edit the target by right clicking a shortcut and clicking Properties at the bottom). Example:
```
"C:\Users\Me\Documents\SRCDShandler\SRCDShandler.exe" -srcdspath \"C:\Users\Me\Documents\MySourceDedicatedServer\srcds.exe\" -srcdsargs \"-game garrysmod +map gm_construct +gamemode sandbox +maxplayers 16\"
```
  The first part in quotes is the target path for the shortcut. After that, you can put in arguments to be supplied to the program when it is run. By supplying `-srcdspath` and/or `-srcdsargs` you can use SRCDShandler much more effectively and much more easily.
