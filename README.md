DarkChat 0.3
========

DarkChat is a [DarkMultiPlayer](https://github.com/godarklight/DarkMultiPlayer) plugin that connects your DMP Server with the chosen IRC channels in the chosen IRC network. You can think of DarkChat as this:

![Scheme](http://i.imgur.com/C3J56Ul.png)


DarkChat depends on Newtonsoft's JSON.NET and SirCmpwn's ChatSharp. Don't worry - you don't need to build ChatSharp. ChatSharp is included in DarkChat's releases.


## Installing

Installing DarkChat should be *easy peasy lemon squeezy* - meaning it should be super easy to install. You just drag all the files from the ZIP Archive and drop it inside the **Plugins** folder of your DarkMultiPlayer server.

* [Download DarkChat](https://github.com/RockyTV/DarkChat/releases)



## Running

You don't need to do anything else after you've installed DarkChat. If you want to customize the IRC bot, such as change its nickname, server, channels he will connect on, etc, you can open the configuration file that locates inside the DarkChat folder in the Plugins directory.


## Configuring

The DarkChat.cfg file looks like this:

```
{
  "Server": "irc.esper.net",
  "Nick": "DarkChat",
  "Ident": "DarkChat",
  "RealName": "DarkChat v0.2a",
  "NickServ": "",
  "Channels": []
}
```

### Server

The IRC server the bot will connect to.

### Nick

The bot's nick.

### Ident

Look up IRC Ident. It will change the bot's ident (normally the "username" of the bot that is displayed when he connects to a channel).

### RealName

The bot's realname.

### NickServ

Set up the NickServ password for the chosen nick.

### Channels

Separated by comma, example of multiple channels:

```
"Channels" : [
  "#channel1",
  "#channel2"
]
```
