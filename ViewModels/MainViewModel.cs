using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MemoryTool.Models;
using MemoryTool.Services;

namespace MemoryTool.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MemoryMonitor _monitor;
        private MemoryInfo _systemMemory = null!;
        private bool _isRefreshing;

        public MemoryInfo SystemMemory
        {
            get => _systemMemory;
            set { _systemMemory = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProcessMemoryInfo> Processes { get; }

        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _monitor = new MemoryMonitor();
            SystemMemory = new MemoryInfo();
            Processes = new ObservableCollection<ProcessMemoryInfo>();
            RefreshCommand = new RelayCommand(Refresh);
        }

        public void Refresh()
        {
            IsRefreshing = true;

            try
            {
                SystemMemory = _monitor.GetSystemMemoryInfo();

                Processes.Clear();
                var processes = _monitor.GetProcessList();
                processes.Sort((a, b) => b.WorkingSet.CompareTo(a.WorkingSet));

                foreach (var process in processes)
                {
                    Processes.Add(process);
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
