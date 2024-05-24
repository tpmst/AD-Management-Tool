using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADTool.Core.Models;
using ADTool.Core.Services;


namespace ADTool.Contracts.Services
{
    public interface IUpdateService
    {

        Task<bool> UpdateApplication(int latestVersion, int currentVersion);

    }
}
