using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ADTool.Services;

namespace ADTool.Contracts.Services
{
    public interface ILogService
    {
        Task createLog(string Action, string inputData);

    }
}
