using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Xml;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Session;
using VRage.Scripting;

namespace NewFang_Plugin
{
    public class NewFang_Plugin : TorchPluginBase, IWpfPlugin
    {

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly string CONFIG_FILE_NAME = "NewFang_PluginConfig.cfg";

        private NewFang_PluginControl _control;
        public UserControl GetControl() => _control ?? (_control = new NewFang_PluginControl(this));

        private Persistent<NewFang_PluginConfig> _config;
        public NewFang_PluginConfig Config => _config?.Data;

        private string webHooksUrl = "https://discord.com/api/webhooks/1151322918249300068/kTklVuvyBPGwOCrFcayof5A4FJ7qypGG0STmekVgLHDc7GP_pnxWV6OLAQvUnDcaTXGd";

        public Timer restartTimer;
        public int timeToRestart = 5;
        public bool pluginIsUpToDate = true;

        public bool isRestarting = false;

        public Timer timer;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            SetupConfig();
            StartTimer();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();
        }

        private void StartTimer()
        {
            timer = new Timer(60000 * 5); // 60 * 5 second interval
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        public string GetVersionFromManifest(string manifestUrl)
        {
            string version = string.Empty;

            using (var webClient = new WebClient())
            {
                try
                {
                    string xmlString = webClient.DownloadString(manifestUrl);

                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xmlString);

                    XmlNode versionNode = xmlDocument.SelectSingleNode("/PluginManifest/Version");
                    if (versionNode != null)
                    {
                        version = versionNode.InnerText;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the download or parsing of the XML
                    Log.Error(ex.Message);
                }
            }

            return version;
        }

        public static bool downloadLatestVersionPlugin(string path, string fileName)
        {
            string url = "https://raw.githubusercontent.com/NewFang/NewFang-Plugin/master/Build/NewFang%20Plugin.zip";
            string savePath = Path.Combine(path, fileName);

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, savePath);
                    Log.Info("ZIP file downloaded successfully.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to download the ZIP file: {ex.Message}");
                return false;
            }
        }

        public void StartRestartTimer()
        {
            restartTimer = new Timer(60000); // 60 second interval
            restartTimer.Elapsed += RestartTimerElapsed;
            restartTimer.Start();
            isRestarting = true;
        }

        private void RestartTimerElapsed(object sender, ElapsedEventArgs e)
        {
            timeToRestart--;
            if (timeToRestart < 1)
            {
                Torch.Restart();
            }
            Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf($"Restarting in {timeToRestart} minutes");
            Log.Info($"Restarting in {timeToRestart} minutes");
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (pluginIsUpToDate)
            {
                Log.Info("Making github request...");

                string originalPath = StoragePath;
                string trimmedPath = originalPath.TrimEnd("Instance".ToCharArray());
                string path = Path.Combine(trimmedPath, "Plugins");

                string manifestUrl = "https://raw.githubusercontent.com/NewFang/NewFang-Plugin/master/NewFang%20Plugin/manifest.xml";
                string versionFromGithub = GetVersionFromManifest(manifestUrl);

                if (versionFromGithub != string.Empty)
                {
                    Log.Info("Successfully got the version from Github. Comparing version");
                    if (Version != versionFromGithub)
                    {
                        Log.Info($"Plugin is not up to date | version from Github: {versionFromGithub}, plugin Version: {Version}");
                        Log.Info("Updateing plugin");

                        Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf("'NewFang Plugin' Is Out Of Date! Updating plugin...");
                        if (downloadLatestVersionPlugin(path, "NewFang_Plugin.zip"))
                        {
                            Log.Info("Successfully downloaded latest version of 'NewFang Plugin'");
                            Log.Info("Restarting in 5 minutes");
                            Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf("Successfully downloaded latest version of 'NewFang Plugin'");
                            Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf("Restarting in 5 minutes");
                            StartRestartTimer();
                            pluginIsUpToDate = false;
                        }
                        else
                        {
                            Log.Info("Falled to download latest version of 'NewFang Plugin'");
                            Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf("Falled to download latest version of 'NewFang Plugin'");
                        }
                    }
                    else
                    {
                        Log.Info($"Plugin is up to date | version from Github: {versionFromGithub}, plugin Version: {Version}");
                    }
                }
                else
                {
                    Log.Error("Github request falled");
                }
            }
        }

        private void sendMessageToDiscord(string message, string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                var payload = $"{{\"content\": \"{message}\"}}";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Timeout = 5000; // Set a timeout if needed

                // Convert the payload to bytes
                byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

                // Set the content length to the length of the payload
                request.ContentLength = payloadBytes.Length;

                // Write the payload to the request stream
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(payloadBytes, 0, payloadBytes.Length);
                    requestStream.Close();
                }

                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    Log.Info("Message sent successfully.");
                }
                else
                {
                    Log.Info($"Failed to send message. Status code: {response.StatusCode}");
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        Log.Info(reader.ReadToEnd());
                    }
                }

                response.Close();
            }
            catch (Exception ex)
            {
                Log.Info($"An error occurred: {ex.Message}");
            }
        }



        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            switch (state)
            {

                case TorchSessionState.Loaded:
                    Log.Info("Session Loaded!");
                    sendMessageToDiscord("Server Online", webHooksUrl);
                    break;

                case TorchSessionState.Unloading:
                    Log.Info("Session Unloading!");
                    sendMessageToDiscord("Server Offline", webHooksUrl);
                    break;
            }
        }

        private void SetupConfig()
        {

            var configFile = Path.Combine(StoragePath, CONFIG_FILE_NAME);

            try
            {

                _config = Persistent<NewFang_PluginConfig>.Load(configFile);

            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data == null)
            {

                Log.Info("Create Default Config, because none was found!");

                _config = new Persistent<NewFang_PluginConfig>(configFile, new NewFang_PluginConfig());
                _config.Save();
            }
        }

        public void Save()
        {
            try
            {
                _config.Save();
                Log.Info("Configuration Saved.");
            }
            catch (IOException e)
            {
                Log.Warn(e, "Configuration failed to save");
            }
        }
    }
}
