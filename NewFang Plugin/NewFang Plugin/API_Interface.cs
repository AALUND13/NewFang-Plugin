using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using VRage.Utils;

namespace NewFang_Plugin
{
    internal class API_Interface
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static string API_URL = null;
        internal static string API_Key = null;

        public static void makeAPIRequest(string subFoler, string message, bool haveAPIKey = true)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    Log.Info($"Sending API Request : {API_URL}/{subFoler}?{(haveAPIKey ? $"apiKey={API_Key}&" : "")}{message}");
                    string data = webClient.DownloadString($"{API_URL}/{subFoler}?{(haveAPIKey ? $"apiKey={API_Key}&" : "")}{message}");
                    Log.Info($"API Send Successfully Respone : {data}");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the download or parsing of the XML
                    Log.Error($"Error making API request: {ex.Message}");
                    Log.Error($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        public static void sendDiscordMessage(long guildId, long channelId, string title, string description, string hexColor = "0x000000")
        {
            string encodedTitle = Uri.EscapeDataString(title);
            string encodedDescription = Uri.EscapeDataString(description);
            makeAPIRequest("send-embed-message", $"guildId={guildId}&channelId={channelId}&title={encodedTitle}&description={encodedDescription}&color={hexColor}");
        }

        public static void sendDiscordMessage(long guildId, long channelId, string title, string hexColor = "0x000000")
        {
            string encodedTitle = Uri.EscapeDataString(title);
            makeAPIRequest("send-embed-message", $"guildId={guildId}&channelId={channelId}&title={encodedTitle}&color={hexColor}");
        }

        public static void updateServerStatus(string status, int players, int maxPlayers, int simulationSpeed)
        {
            string encodedStatus = Uri.EscapeDataString(status);
            makeAPIRequest("update_server_status", $"status={encodedStatus}&players={players}&maxPlayers={maxPlayers}&simulationSpeed={simulationSpeed}");
        }

    }
}