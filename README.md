# LoggingBridge

This repository contains the code to serve as example of creating custom Bridge plugin.

To use this LoggingBridge Plugin:

1) Clone the repo into Assets/External/Bridges/LoggingBridge inside of your Simulator Unity Project

2) Build the Bridge Plugin for use with the Simulator, navigate to the `Simulator -> Build` Unity Editor menu item. Select LoggingBridge bridge and build. Output bundle will be in AssetBundles/Bridges folder in root of Simulator Unity Project

3) Simulator will load, at runtime, all custom Bridge plugins from AssetBundles/Bridges folder


## Implementation details

LoggingBridge plugin has two source files implementing following C# classes:

1) `LoggingBrigeFactory` - describes name of plugin, and provides factory functions to create instance, subscriber and publisher. This is loaded by Simulator at runtime.

2) `LoggingBridgeInstance` - "sends" the message data. It is instantiated per every vehicle that uses bridge.

In this bridge all data is written to compressed text file in Unity data directory (`%USERPROFILE%/AppData/LocalLow/LG Silicon Valley Lab/LGSVL Simulator` folder on Windows).
File name used for output is assigned from connection string specified on Connect method - for example, `test` will create `test.json.gz` file.

This bridge example plugin serves as example how to start building bridge plugin. For more complete functionality refer to builtin ROS, ROS2 or CyberRT bridges in simulator.

### Copyright and License

Copyright (c) 2020 LG Electronics, Inc.

This software contains code licensed as described in LICENSE.
