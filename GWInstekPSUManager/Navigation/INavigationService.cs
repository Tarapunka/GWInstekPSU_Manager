using GWInstekPSUManager.ViewModels;

namespace GWInstekPSUManager.Navigation;

public interface INavigationService
{
    void NavigateTo<T>() where T : IViewModel;
}