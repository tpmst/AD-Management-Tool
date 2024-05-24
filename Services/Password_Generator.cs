using ADTool.Contracts.Services;
using ADTool.Views;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTool.Services
{
    public class Password_Generator : IPassword_Generator
    {
        private static string AppConfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppCon.json");

        private static readonly Random random = new Random();
        private string chars;
        private int passwordlength;
        public string GeneratePassword()
        {
            AppConfigManager.TryLoadPasswordConfig(AppConfPath, out passwordlength, out chars);
            if(passwordlength < 12)
            {
                passwordlength = 12;
            }
            return new string(Enumerable.Repeat(chars, passwordlength)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string GenerateSecurePass(int passwordLength2)
        {
            AppConfigManager.TryLoadPasswordConfig(AppConfPath, out passwordlength, out chars);
            return new string(Enumerable.Repeat(chars, passwordLength2)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
