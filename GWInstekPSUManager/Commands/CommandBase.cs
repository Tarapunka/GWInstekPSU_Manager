﻿using System.Windows.Input;

namespace GWInstekPSUManager.Commands;

public abstract class CommandBase : ICommand
{
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public abstract bool CanExecute(object parameter);
    public abstract void Execute(object parameter);
}
