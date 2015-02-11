using System;
using System.IO;
using System.Text;
using DarkMultiPlayerServer;

namespace DarkChat
{
    public class Log
    {
        private static string LogDir = Path.Combine(Main.pluginDir, "logs");

        public static void WriteLog(string message)
        {
            string output = string.Format("[IRC] {0}", message);
            DarkLog.Normal(output);
        }

        public static void LogChannel(string channel, string message, bool date = true)
        {
            string logFile = Path.Combine(LogDir, string.Format("{0}.log", channel));

            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }

            string output;
            try
            {
                if (date)
                {
                    output = string.Format("[{0}] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), message);
                }
                else
                {
                    output = string.Format("{0}", message);
                }
                File.AppendAllText(logFile, string.Format("{0}{1}", output, Environment.NewLine));
            }
            catch (Exception e)
            {
                DarkLog.Error("Error while writing to channel log file!");
                DarkLog.Error(e.ToString());
            }
        }
    }
}
