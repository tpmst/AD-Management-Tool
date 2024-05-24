using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTool.Contracts.Services
{
    public interface IPassword_Generator
    {
        string GeneratePassword();

        string GenerateSecurePass(int passwordLength2);
    }
}
