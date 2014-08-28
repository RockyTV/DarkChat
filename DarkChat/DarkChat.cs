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

    class Command
    {
        public string command;
        public Action<string> action;
        public string description;
    }

    public class DarkChat : DMPPlugin
    {
        private static string PLUGIN_VER = "0.3";

        private static string PLUGIN_DIR = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"), "DarkChat");
        private static string CONFIG_FILE = Path.Combine(PLUGIN_DIR, "DarkChat.cfg");

        private static Settings settings;

        private static List<string> listChannels = new List<string>();

        private static List<Command> listCommands = new List<Command>();

        private static IrcClient ircClient;
        private static IrcUser ircUser;

        public DarkChat()
        {
            RegisterCommands();

            loadConfig();

            DarkLog.Debug(String.Format("[DarkChat] initialized, version {0}", PLUGIN_VER));

        }

        public void RegisterCommands()
        {
            listCommands.Clear();
            listCommands.Add(new Command { command = "darkbot", action = DarkChat.DCCommand, description = "DarkChat commands." });

            foreach (Command command in listCommands)
            {
                CommandHandler.RegisterCommand(command.command, command.action, command.description);
            }
        }

        public static void checkDirectoryExists()
        {
            if (!Directory.Exists(PLUGIN_DIR))
                Directory.CreateDirectory(PLUGIN_DIR);
        }

        public static void DCCommand(string commandArgs)
        {
            string config = "";
            string func = "";
            string param = "";

            string[] parts = commandArgs.Split(' ');

            config = parts[0];
            if (parts.Length > 1)
            {
                func = parts[1];
                if (parts.Length < 4)
                    param = parts[2];
            }

            switch (config)
            {
                case "channels":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        string channels = String.Join(", ", listChannels);
                        DarkLog.Normal("[DarkChat] Channels: " + channels);
                    }
                    else if (func == "add")
                    {
                        listChannels.Add(param);
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Added '" + param + "' to the channel list.");
                    }
                    else if (func == "del")
                    {
                        if (listChannels.Contains(param))
                        {
                            listChannels.Remove(param);
                            saveConfig();
                            DarkLog.Normal("[DarkChat] Removed '" + param + "' from the channel list.");
                        }
                        else
                            DarkLog.Normal("[DarkChat] Error! There isn't a channel with that name on my config!");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                case "server":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        DarkLog.Normal("[DarkChat] Server: " + settings.Server);
                    }
                    else if (func == "set")
                    {
                        settings.Server = param;
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Successfully set '" + param + "' as server.");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                case "nick":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        DarkLog.Normal("[DarkChat] Nick: " + settings.Nick);
                    }
                    else if (func == "set")
                    {
                        settings.Nick = param;
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Successfully set '" + param + "' as nickname.");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                case "ident":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        DarkLog.Normal("[DarkChat] Ident: " + settings.Ident);
                    }
                    else if (func == "set")
                    {
                        settings.Ident = param;
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Successfully set '" + param + "' as ident.");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                case "realname":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        DarkLog.Normal("[DarkChat] Realname: " + settings.RealName);
                    }
                    else if (func == "set")
                    {
                        settings.RealName = param;
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Successfully set '" + param + "' as realname.");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                case "nickserv":
                    if (param == "")
                    {

                    }

                    if (func == "")
                    {
                        DarkLog.Normal("[DarkChat] NickServ password: " + settings.NickServ);
                    }
                    else if (func == "set")
                    {
                        settings.NickServ = param;
                        saveConfig();
                        DarkLog.Normal("[DarkChat] Successfully set '" + param + "' as NickServ password.");
                    }
                    else
                        DarkLog.Normal("[DarkChat] Invalid function. Type /darkbot help for more info.");
                    break;
                default:
                case "help":
                    DarkLog.Normal("[DarkChat] Available commands: channels [add|del] <param>, server [set] <param>, nick [set] <param>, ident [set] <param>, realname [set] <param>, nickserv [set] <param>, help");
                    break;
            }
        }

        public static void saveConfig()
        {
            checkDirectoryExists();

            using (StreamWriter sw = new StreamWriter(CONFIG_FILE))
            {
                Settings defaultSettings = new Settings
                {
                    Server = "irc.esper.net",
                    Nick = "DarkBot",
                    Ident = "DarkBot",
                    RealName = "DarkChat v" + PLUGIN_VER,
                    NickServ = "",
                    Channels = listChannels.ToArray()
                };

                string configJSON = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);

                sw.WriteLine(configJSON);
            }
        }

        public static void loadConfig()
        {
            listChannels.Clear();
            if (!File.Exists(CONFIG_FILE))
            {
                saveConfig();
            }
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(CONFIG_FILE));
            listChannels.AddRange(settings.Channels);
            
        }

        public void setupIRC()
        {
            if (ircClient == null)
            {
                ircUser = new IrcUser(settings.Nick, settings.Ident, settings.NickServ, settings.RealName);
                ircClient = new IrcClient(settings.Server, ircUser);
            }

            ircClient.ConnectAsync();
            ircClient.ConnectionComplete += (s, e) =>
            {
                DarkLog.Debug("[DarkChat] Connected to " + settings.Server);
                foreach (string channel in settings.Channels)
                {
                    DarkLog.Debug("[DarkChat] Joining " + channel);
                    ircClient.JoinChannel(channel);
                }
            };
            ircClient.ChannelMessageRecieved += (s, e) =>
            {
                string[] parts = e.PrivateMessage.Source.Split('#');
                ClientHandler.SendChatMessageToChannel(parts[1], String.Format("[{0}] <{1}> {2}", DateTime.Now.ToString("HH:mm:ss"), e.PrivateMessage.User.Nick, e.PrivateMessage.Message));
            };
            ircClient.NetworkError += (s, e) => DarkLog.Debug("[DarkChat] Connection Error: " + e.SocketError);
        }

        public void quitIRC()
        {
            if (ircClient != null)
                ircClient.Quit();
        }

        public override void OnServerStart()
        {
            setupIRC();
        }

        public override void OnServerStop()
        {
            quitIRC();
        }

        public override void OnMessageReceived(ClientObject client, ClientMessage message)
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
