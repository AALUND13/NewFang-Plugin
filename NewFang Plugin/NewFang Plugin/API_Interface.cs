using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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

        public static void updateServerStatus(string status, int players, int maxPlayers, float simulationSpeed, List<string> mods)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.QueryString.Add("apiKey", API_Key);

                try
                {
                    string url = $"{API_URL}/update_server_status";

                    // Serialize the mods array to JSON with proper escaping
                    string jsonPayload = $"{{\"status\":{status},\"players\":{players},\"maxPlayers\":{maxPlayers},\"simulationSpeed\":{simulationSpeed},\"mods\":{mods}}}";

                    string response = client.UploadString(url, jsonPayload);

                    Console.WriteLine("API Response: " + response);
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }

    }
}