using System;
using SettingsParser;
using System.ComponentModel;
using System.Collections.Generic;

namespace DarkChat
{
    public class BotSettings
    {
        [Description("The IRC server the bot should connect to.")]
        public string Server = "irc.esper.net";
        [Description("Use SSL instead to connect to the IRC server.")]
        public bool UseSSL = false;
        [Description("The nick the bot should use.")]
        public string Nick = "DarkBot";
        [Description("The nick the bot should use if the main nickname is in use.")]
        public string NickInUse = "DarkBot_";
        [Description("NickServ password for the specified nick.")]
        public string Password = "";
        [Description("A list of channels the bot should connect to.")]
        public List<string> Channels = new List<string>();

        [Description("Specify if the global ingame chat should be sent to the IRC channel. If set to false, only messages sent to one of the bot's IRC channels will be sent to the respective IRC channel.")]
        public bool GlobalMessages = true;
        [Description("Specify whether to store IRC messages onto a separate log file.")]
        public bool StoreLog = true;
    }
}
