using ADTool.ViewModels;

namespace ADTool.Contracts.Services;

public interface IUserDataService
{
    event EventHandler<UserViewModel> UserDataUpdated;

    void Initialize();

    UserViewModel GetUser();
}
