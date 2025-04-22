using CommunityToolkit.Mvvm.ComponentModel;
using GWInstekPSUManager.ViewModels;

namespace GWInstekPSUManager.ViewModels;

public abstract class BaseViewModel : ObservableObject, IViewModel
{
    public virtual void OnViewShown() { }
    public virtual void OnViewHidden() { }
}
