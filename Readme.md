# Pixa.Soundbridge

[![Status](https://img.shields.io/badge/status-active-success.svg)]()
[![GitHub Issues](https://img.shields.io/github/issues/pixafera/soundbridge.svg)](https://github.com/pixafera/soundbridge/issues)
[![GitHub Pull Requests](https://img.shields.io/github/issues-pr/pixafera/soundbridge.svg)](https://github.com/pixafera/soundbridge/pulls)

## Table of Contents

+ [About](#about)
+ [Getting Started](#getting_started)
+ [Installing](#installing)
+ [Usage](#usage)

## About <a name="about"></a>

This is a library for interacting with [Roku Soundbridge](https://en.wikipedia.org/wiki/SoundBridge), a network music
player that Roku made before they were known for set-top boxes.  Soundbridges can be controlled over the network with
a text based protocol called RCP.  This library implements an RCP client for the .net platform.

## Getting Started <a name="getting_started"></a>

You'll need a .net project targetting [a .net implementation implementing .net standard 2.0 or later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version)
and a Roku Soundbridge, either that you've owned for the past 15 years or bought on ebay for the nostalgia.


## Installing <a name="installing"></a>

### Nuget Package

Pixa.Soundbridge is available as a nuget package on [nuget.org](https://nuget.org/packages/Pixa.Soundbridge).  You can
install it with one of the following commands:

#### Package Manager

From the Package Manager Console in Visual Studio with the project you wish to reference this library selected type:

```
Install-Package Pixa.Soundbridge
```

#### .net CLI

With the .net core SDK installed and at a command prompt at the directory where the project file you wish to reference
this library is located type:

```
dotnet add package Pixa.Soundbridge
```

#### PackageReference

Edit the project file you wish to reference this library and add the following to any `ItemGroup` section:

```xml
<PackageReference Include="Pixa.Soundbridge"/>
```

## Usage <a name="usage"></a>

### Connect to a Soundbridge

```csharp
using Pixa.Soundbridge;

...

var soundbridgeIPAddress = "192.168.123.456";
var sb = SoundbridgeFactory.CreateFromTcp(soundbridgeIPAddress);
```

### Connect the Soundbridge to a server

```csharp
// Get a list of the servers the soundbridge can see
var servers = sb.GetServers();

// Connect to a specific server
var serverName = "My Media Server";
var server = servers["My Media Server"];
```

### Play some music

```csharp
// Open a container

var c1 = server.Container.GetChildContainers()["Audio"];
c1.Enter();
var c2 = c1.GetChildContainers()["Random Music"];
c2.Enter();
c2.GetSongs().Play();
```
