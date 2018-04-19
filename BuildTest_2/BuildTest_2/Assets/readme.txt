How to run & test samples:
http://www.cnblogs.com/answerwinner/p/4590981.html

Full documents is here:
http://www.cnblogs.com/answerwinner/p/4591144.html


How to run JavaScript Binding samples(in case online documents fail to open)

---------------------------------------------------------------------------------------

First of course, create an empty project and import JSBinding package.

In Build Settings dialog, switch platform to PC, Mac & Linux Standalone.


If you are on Windows, you have to copy DLL to unity install directory:
    1. for 32bit editor: copy Assets/Plugins/x86/mozjs-31.dll to UnityInstallDir/Editor folder
    2. for 64bit editor: copy Assets/Plugins/x86_64/mozjs-31.dll to UnityInstallDir/Editor folder
 
If you are running Windows exe, you have to copy(or move) mozjs-31.dll to the folder containing .exe.


Follow these steps:
    1. Click menu Assets | JavaScript | Gen Bindings
    2. Open scene JSBinding/Samples/2048/2048.unity.
    3. Click play button and enjoy it!

---------------------------------------------------------------------------------------