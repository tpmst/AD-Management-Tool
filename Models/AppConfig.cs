using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTool.Models
{
    public class AppConfig
    {
        public string ConfigurationsFolder { get; set; }

        public string AppPropertiesFileName { get; set; }

        public string PrivacyStatement { get; set; }

        public string UserFileName { get; set; }

        public string IdentityClientId { get; set; }
    }
}
