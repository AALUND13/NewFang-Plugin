using System;
using System.Collections.Generic;
using Torch;

namespace NewFang_Plugin
{
    public class NewFang_PluginConfig : ViewModel
    {

        //private string _StringProperty = "root";
        //private int _IntProperty = 0;
        //private bool _BoolProperty = true;

        //public string StringProperty { get => _StringProperty; set => SetValue(ref _StringProperty, value); }
        //public int IntProperty { get => _IntProperty; set => SetValue(ref _IntProperty, value); }
        //public bool BoolProperty { get => _BoolProperty; set => SetValue(ref _BoolProperty, value); }

        private string _WebHooksUrl = "https://discord.com/api/webhooks/1151322918249300068/kTklVuvyBPGwOCrFcayof5A4FJ7qypGG0STmekVgLHDc7GP_pnxWV6OLAQvUnDcaTXGd";

        public string WebHooksUrl { get => _WebHooksUrl; set => SetValue(ref _WebHooksUrl, value); }
    }
}
