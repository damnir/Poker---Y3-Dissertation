# Libraries and Dependencies
Following libraries and SDKs need to be manually added to the Unity Project in order to compile it:

    >     FirebaseAuth_version-7.1.0 - https://firebase.google.com/support/release-notes/unity
    >     FirebaseDatabase_version-7.1.0 - https://firebase.google.com/support/release-notes/unity
    >	  MLAPI Networking Library - version 0.1.0 - https://docs-multiplayer.unity3d.com/
    >     Newtonsoft JSON.Net- version 13.0.102 for Unity - https://github.com/jilleJr/Newtonsoft.Json-for-Unity
    
    System built using:
    >	  Unity Android SDK 28.0.3
    >	  Unity Version 2019.4.20f1

> Import a new package in Unity by going to Assets > Import Package > Custom Package.

# Build the Application
To build the server application, open the project and enable the `Server` script on the `NetworkManager` component then compile using the default Windows build settings and check the "Server Build" box. in Unity. 

To build the client application, open the project and enable the `Client` script on the `NetworkManager` component then compile the apk using the default Android settings.

> Compile by going to File > Build Settings > Build

# Executables
A pre-compiled executable version of the server application (for Windows) can be found in the source code under 'ServerBuild' folder.

> Note: the server application is configured to listen for connections on my personal server (Windows PC) on my public IP address that is port forwarded to the server; IP: xx Port: xx

A pre-compiled client-side .apk file for Android OS can be found in the source code under 'AndroidBuild' folder. Also configured to connect to the above IP address and port.
