using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkMultiPlayerServer;
using DarkMultiPlayerCommon;
using MessageStream;
using ChatSharp;

namespace DarkChat
{
    [DMPPlugin]
    public class DarkChat
    {
        private static string PLUGIN_VER = "0.1";
        
        // IRC CONFIG
        private static string SERVER = "irc.esper.net";
        private static string NICK = "DarkChatBot";
        private static string IDENT = "DarkChatBot";
        private static string REALNAME = "DarkChatBot";
        private static string CHANNEL = "DMP";

        private static IrcClient ircClient;
        private static IrcUser ircUser;

        public DarkChat()
        {
            DarkLog.Debug(String.Format("[DarkChat] initialized version {0}", PLUGIN_VER));
            DarkLog.Debug("[DarkChat] creating IRCUser");
            ircUser = new IrcUser(NICK, IDENT, "", REALNAME);
            ircClient = new IrcClient(SERVER, ircUser);
            DarkLog.Debug("[DarkChat] connecting to " + SERVER);
            ircClient.NetworkError += (s, e) => DarkLog.Debug("[DarkChat] Connection Error: " + e.SocketError);
            ircClient.ConnectionComplete += (s, e) => ircClient.JoinChannel(CHANNEL);
        }

        public void Update()
        {
            
            ircClient.ChannelMessageRecieved += (s, e) =>
            {
                ClientHandler.SendChatMessageToChannel(CHANNEL, String.Format("[{0}] <{1}> {2}", DateTime.Now.ToString("HH:mm:ss"), e.PrivateMessage.User.Nick, e.PrivateMessage.Message));
            };
        }

        public void OnClientConnect(ClientObject client)
        {

        }

        public void OnClientAuthenticated(ClientObject client)
        {

        }

        public void OnClientDisconnect(ClientObject client)
        {

        }

        public void OnMessageReceived(ClientHandler client, ClientMessage message)
        {
            if (message.type == ClientMessageType.CHAT_MESSAGE)
            {
                using (MessageReader mr = new MessageReader(message.data, false))
                {
                    ChatMessageType messageType = (ChatMessageType)mr.Read<int>();
                    string fromPlayer = mr.Read<string>();
                    if (messageType == ChatMessageType.CHANNEL_MESSAGE)
                    {
                        string channel = mr.Read<string>();
                        string umessage = mr.Read<string>();
                        if (channel == CHANNEL)
                            ircClient.Channels[0].SendMessage(String.Format("{0} -> {1}", fromPlayer, umessage));
                    }
                }
            }
        }

        public void OnRawMessageReceived(ClientHandler client, byte[] message)
        {

        }
    }
}
