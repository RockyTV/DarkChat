using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkMultiPlayerServer;
using DarkMultiPlayerCommon;
using System.Reflection;
using MessageStream;
using System.IO;
using ChatSharp;
using Newtonsoft.Json;

namespace DarkChat
{
    class Settings
    {
        public string Server { get; set; }

        public string Nick { get; set; }

        public string Ident { get; set; }

        public string RealName { get; set; }

        public string NickServ { get; set; }

        public string[] Channels { get; set; }
    }

    [DMPPlugin]
    public class DarkChat
    {
        private static string PLUGIN_VER = "0.1";

        private static string PLUGIN_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DarkChat");
        private static string CONFIG_FILE = Path.Combine(PLUGIN_DIR, "DarkChat.cfg");

        private static Settings settings;
        

        private static IrcClient ircClient;
        private static IrcUser ircUser;

        public DarkChat()
        {
            loadConfig();
            DarkLog.Debug(String.Format("[DarkChat] initialized version {0}", PLUGIN_VER));
            ircUser = new IrcUser(settings.Nick, settings.Ident, settings.NickServ, settings.RealName);
            ircClient = new IrcClient(settings.Server, ircUser);
        }

        public void checkDirectoryExists()
        {
            if (!Directory.Exists(PLUGIN_DIR))
                Directory.CreateDirectory(PLUGIN_DIR);
        }

        public void saveConfig()
        {
            checkDirectoryExists();

            using (StreamWriter sw = new StreamWriter(CONFIG_FILE))
            {
                Settings defaultSettings = new Settings
                {
                    Server = "irc.esper.net",
                    Nick = "DarkChat",
                    Ident = "DarkChat",
                    RealName = "DarkChat v" + PLUGIN_VER,
                    NickServ = "",
                    Channels = new String[] {"dmp"}
                };

                string configJSON = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);

                sw.WriteLine(configJSON);
            }
        }

        public void loadConfig()
        {
            if (!File.Exists(CONFIG_FILE))
            {
                saveConfig();
            }
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(CONFIG_FILE));
            
        }

        public void Update()
        {
            
        }

        public void OnServerStart()
        {
            ircClient.ConnectAsync();
            ircClient.ConnectionComplete += (s, e) =>
            {
                DarkLog.Debug("[DarkChat] connected.");
                foreach (string channel in settings.Channels)
                {
                    DarkLog.Debug("[DarkChat] joining #" + channel);
                    ircClient.JoinChannel("#" + channel);
                }
            };
            ircClient.ChannelMessageRecieved += (s, e) =>
                {
                    string[] parts = e.PrivateMessage.Source.Split('#');
                    ClientHandler.SendChatMessageToChannel(parts[1], String.Format("[{0}] <{1}> {2}", DateTime.Now.ToString("HH:mm:ss"), e.PrivateMessage.User.Nick, e.PrivateMessage.Message));
                };
            ircClient.NetworkError += (s, e) => DarkLog.Debug("[DarkChat] Connection Error: " + e.SocketError);
        }

        public void OnServerStop()
        {
            ircClient.Quit();
        }

        public void OnMessageReceived(ClientObject client, ClientMessage message)
        {
            if (message.type == ClientMessageType.CHAT_MESSAGE)
            {
                using (MessageReader mr = new MessageReader(message.data, false))
                {
                    ChatMessageType messageType = (ChatMessageType)mr.Read<int>();
                    string fromPlayer = mr.Read<string>();
                    if (messageType == ChatMessageType.CHANNEL_MESSAGE)
                    {
                        string channel = "#" + mr.Read<string>().ToLower();
                        string umessage = mr.Read<string>();
                        foreach (var ircChannel in ircClient.Channels)
                        {
                            if (ircChannel.Name.ToLower() == channel)
                            {
                                ircChannel.SendMessage(String.Format("{0} -> {1}", fromPlayer, umessage));
                            }
                        }
                    }
                }
            }
        }
    }
}
