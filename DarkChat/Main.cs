using System;
using System.IO;
using ChatSharp;
using SettingsParser;
using ChatSharp.Events;
using DarkMultiPlayerServer;
using DarkMultiPlayerCommon;
using System.Collections.Generic;
using DarkMultiPlayerServer.Messages;
using MessageStream2;

namespace DarkChat
{
    public class Main : DMPPlugin
    {
        private static List<Command> CommandsList;

        private static ConfigParser<BotSettings> _settings;
        private static BotSettings settings;

        public static string pluginDir = "";

        // Bot configuration
        private static IrcClient _client;
        private static IrcUser _user;

        public Main()
        {
            // Setup our directories and files
            pluginDir = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins"), "DarkChat");
            if (!Directory.Exists(pluginDir))
            {
                Directory.CreateDirectory(pluginDir);
            }

            _settings = new ConfigParser<BotSettings>(new BotSettings(), Path.Combine(pluginDir, "config.cfg"));

            // Create and register our commands
            CommandsList = new List<Command>();
            CommandsList.Add(new Command("irc", "IRC Bot configuration commands", null));
            foreach (Command command in CommandsList)
            {
                CommandHandler.RegisterCommand(command.Name, command.Action, command.Description);
            }


            // Load our settings
            _settings.LoadSettings();
            settings = _settings.Settings;

            // Instantiate our bot
            SetupBot();
        }

        private void SetupBot()
        {
            DarkLog.Debug("Setting up bot...");
            _user = new IrcUser(settings.Nick, "darkchat", settings.Password, "http://github.com/RockyTV/DarkChat");
            _client = new IrcClient(settings.Server, _user, settings.UseSSL);

            _client.ChannelMessageRecieved += ChannelMessageReceived;
            _client.UserMessageRecieved += UserMessageReceived;
            _client.ConnectionComplete += ConnectionComplete;
            _client.NetworkError += NetworkError;
            _client.NickInUse += NickInUse;
            DarkLog.Debug("Done!");

            DarkLog.Debug("Connecting to " + settings.Server);
            _client.ConnectAsync();
            DarkLog.Debug("Done!");
        }

        private static void ChannelMessageReceived(object sender, PrivateMessageEventArgs args)
        {
            if (settings.StoreLog)
            {
                Log.LogChannel(args.PrivateMessage.Source, string.Format("<{0}> {1}", args.PrivateMessage.User.Nick, args.PrivateMessage.Message));
            }

            string message = args.PrivateMessage.Message;
            string user = args.PrivateMessage.User.Nick;
            string channel = args.PrivateMessage.Source;

            if (!settings.GlobalMessages)
            {
                message = string.Format("[{0}] <{1}>: {2}", DateTime.UtcNow.ToString("HH:mm:ss"), user, message);
                Chat.SendChatMessageToChannel(channel.Substring(1), message);
            }
            else
            {
                message = string.Format("[{0}] {1} <{2}>: {3}", DateTime.UtcNow.ToString("HH:mm:ss"), channel, user, message);
                Chat.SendChatMessageToAll(message);
            }
        }

        private static void UserMessageReceived(object sender, PrivateMessageEventArgs args)
        {
            if (settings.StoreLog)
            {
                Log.LogChannel(args.PrivateMessage.User.Nick, string.Format("<{0}> {1}", args.PrivateMessage.User.Nick, args.PrivateMessage.Message));
            }
        }

        private static void ConnectionComplete(object sender, EventArgs args)
        {
            try
            {
                foreach (string channel in _settings.Settings.Channels)
                {
                    _client.JoinChannel(channel);
                    Log.WriteLog("Joining '" + channel + "'");
                    DarkLog.Debug("Done!");
                }
            }
            catch (Exception e)
            {
                DarkLog.Error("Error while joining channel: " + e.ToString());
            }
        }

        private static void NetworkError(object sender, SocketErrorEventArgs args)
        {
            Log.WriteLog("Network error: " + args.SocketError.ToString());
        }

        private static void NickInUse(object sender, ErronousNickEventArgs args)
        {
            args.NewNick = settings.NickInUse;
        }

        public override void OnServerStart()
        {
            foreach (string channel in settings.Channels)
            {
                Log.LogChannel(channel, string.Format("**** BEGIN LOGGING AT {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")), false);
                Log.LogChannel(channel, "", false);
            }
        }

        public override void OnServerStop()
        {
            foreach (string channel in settings.Channels)
            {
                Log.LogChannel(channel, string.Format("**** ENDING LOGGING AT {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")), false);
                Log.LogChannel(channel, "", false);
            }
        }

        public override void OnMessageReceived(ClientObject client, ClientMessage messageData)
        {
            if (messageData.type == ClientMessageType.CHAT_MESSAGE)
            {
                using (MessageReader mr = new MessageReader(messageData.data))
                {
                    ChatMessageType type = (ChatMessageType)mr.Read<int>();
                    string fromPlayer = mr.Read<string>();

                    if (type == ChatMessageType.CHANNEL_MESSAGE)
                    {
                        string channel = "#" + mr.Read<string>();
                        string msg = mr.Read<string>();

                        string message = "";

                        if (settings.GlobalMessages)
                        {
                            if (channel == "")
                            {
                                channel = "Global";
                            }

                            message = string.Format("{0} said on {1}: {2}", fromPlayer, channel, msg);
                            foreach (IrcChannel _channel in _client.Channels)
                            {
                                _channel.SendMessage(message);
                            }
                        }
                        else
                        {
                            foreach (IrcChannel chan in _client.Channels)
                            {
                                if (chan.Name.ToLower() == channel)
                                {
                                    message = string.Format("{0} said: {1}", fromPlayer, msg);
                                    _client.Channels[channel].SendMessage(message);
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}