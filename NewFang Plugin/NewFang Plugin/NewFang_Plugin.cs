using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            SetupConfig();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");

            Save();
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
