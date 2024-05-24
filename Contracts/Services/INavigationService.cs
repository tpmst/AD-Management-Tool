using System.Windows.Controls;

namespace ADTool.Contracts.Services;

public interface INavigationService
{
    event EventHandler<string> Navigated;

    bool CanGoBack { get; }

    void Initialize(Frame shellFrame);

    Task<bool> NavigateTo(string pageKey, object parameter = null, bool clearNavigation = false);

    void GoBack();

    void UnsubscribeNavigation();

    List<string> GetOpendPage();

    void CleanNavigation();
}
