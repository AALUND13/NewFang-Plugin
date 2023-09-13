using System;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;

namespace NewFang_Plugin
{
    [Category("NewFang_Plugin")]
    public class NewFang_PluginCommands : CommandModule
    {

        public NewFang_Plugin Plugin => (NewFang_Plugin)Context.Plugin;

        [Command("Restart", "This Command Will Restart The Server")]
        [Permission(MyPromoteLevel.Moderator)]
        public void Restart(int timeInMin = 5)
        {
            if (Plugin.isRestarting) return;

            Context.Torch.CurrentSession?.Managers?.GetManager<IChatManagerServer>()?.SendMessageAsSelf($"Restarting in {Math.Max(timeInMin, 0)} minutes");

            Plugin.timeToRestart = Math.Max(timeInMin, 0);
            Plugin.pluginIsUpToDate = false; //This will stop the auto updater
            Plugin.StartRestartTimer();
        }
    }
}
