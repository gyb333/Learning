using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learn.OSS
{
    public class SettingsFactory
    {

        private static ISettings _settings = new Settings();


        public static void InitializeSettingsFactory(ISettings settings)
        {
            _settings = settings;
        }

        public static ISettings GetSettings()
        {
            return _settings;
        }
    }
}
