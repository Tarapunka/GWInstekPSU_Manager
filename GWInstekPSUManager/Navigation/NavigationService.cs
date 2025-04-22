using GWInstekPSUManager.ViewModels;
using System.Windows.Controls;

namespace GWInstekPSUManager.Navigation;

public class NavigationService : INavigationService
{
    private readonly Func<Type, UserControl> _viewFactory;

    public NavigationService(Func<Type, UserControl> viewFactory)
    {
        _viewFactory = viewFactory;
    }

    public void NavigateTo<T>() where T : IViewModel
    {
        var viewModelType = typeof(T);
        var viewType = Type.GetType(viewModelType.FullName.Replace("ViewModels", "Views").Replace("ViewModel", "View"));

        if (viewType == null)
            throw new InvalidOperationException($"View for ViewModel {viewModelType.Name} not found.");

        var view = _viewFactory(viewType);
        var contentControl = App.Current.MainWindow.FindName("MainContent") as ContentControl;

        if (contentControl != null)
            contentControl.Content = view;
    }
}