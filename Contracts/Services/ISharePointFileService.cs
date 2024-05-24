using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTool.Contracts.Services
{
    public interface ISharePointFileService
    {
        Task<string> GetFileBody();

        Task UploadConfig(byte[] fileBytes);

        Task<bool> UpdateCreateSharedMailbox(string contetnt);

        Task<bool> UpdateCreateTeamsgroup(string contetnt);

        Task<bool> UpdateLogFile(string contetnt, string username);

        Task<bool> UpdateNewUserDetails(string contetnt);

        Task<bool> UpdateDeleteUserDetails(string contetnt);

    }
}
