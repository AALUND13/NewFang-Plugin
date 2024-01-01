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

        private string _API_URL = "Unknow";
        private string _API_Key = "Unknow";

        public string API_URL { get => _API_URL; set => SetValue(ref _API_URL, value); }
        public string API_Key { get => _API_Key; set => SetValue(ref _API_Key, value); }
    }
}
