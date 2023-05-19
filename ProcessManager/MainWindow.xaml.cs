using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProcessManager.Helpers;

namespace ProcessManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProcessInfo> Processes { get; set; }
        private ICollectionView filterSource;
        private Timer ProcessInfoUpdateTimer;
        private ObservableCollection<ProcessInfo> UpdatedProcesses;


        public MainWindow()
        {
            InitializeComponent();
            ClearFilter.Visibility = Visibility.Hidden;
            Processes = new ObservableCollection<ProcessInfo>();
            GetProcesses();
            LoadProcessesIntoGrid();

            ProcessInfoUpdateTimer = new Timer();
            ProcessInfoUpdateTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            ProcessInfoUpdateTimer.Elapsed += ProcessInfoUpdateTimerElapsed;

            ProcessInfoUpdateTimer.Start();
        }

        private void ProcessInfoUpdateTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateProcesses();
            }));
        }

        private void FilterAction(string filterText)
        {
            filterSource.Filter = string.IsNullOrEmpty(filterText)
                ? (Predicate<object>)null
                : _ => _ is ProcessInfo process && FilterProcess(process, filterText);
        }

        private bool FilterProcess(ProcessInfo process, string filterText)
        {
            return ContainsIgnoreCase(process.ProcessName, filterText);
        }

        private void Filter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Filter.Text))
            {
                ClearFilter.Visibility = Visibility.Visible;
            }
        }

        private void OnFilterKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            FilterAction(Filter.Text);
        }

        private void ClearFilter_OnClick(object sender, RoutedEventArgs e)
        {
            Filter.Text = "";
            ClearFilter.Visibility = Visibility.Hidden;
            FilterAction(string.Empty);
        }

        private bool ContainsIgnoreCase(string source, string value)
        {
            if ((source == null) || (value == null))
            {
                return false;
            }

            return source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private void OnFilterButtonClick(object sender, RoutedEventArgs e)
        {
            FilterAction(Filter.Text);
        }

        private void OnKillProcessButtonClick(object sender, RoutedEventArgs e)
        {
            var process = (ProcessInfo)DataGrid.SelectedItem;
            if (process == null)
            {
                MessageBox.Show("Please select a process to kill.", "Error", MessageBoxButton.OK);
                return;
            }
            var message = $"Are you sure you want to stop {process.ProcessName}?";
            var dialogResult = MessageBox.Show(message, "Error", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                Processes.Remove(process);
                Process.GetProcessById(process.Id).Kill();
                LoadProcessesIntoGrid();
            }
        }

        private void OnRestartProcessButtonClick(object sender, RoutedEventArgs e)
        {
            var process = (ProcessInfo)DataGrid.SelectedItem;

            if (process == null)
            {
                MessageBox.Show("Please select a process to restart.", "Error", MessageBoxButton.OK);
                return;
            }
            var message = $"Are you sure you want to restart {process.ProcessName}?";
            var dialogResult = MessageBox.Show(message, "Error", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                Process.Start(process.ProcessName);
                Process.GetProcessById(process.Id).Kill();
            }
        }

        private void LoadProcessesIntoGrid()
        {
            DataGrid.ItemsSource = Processes;
            filterSource = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
        }

        private void GetProcesses()
        {
            var count = 1;
            var currentProcesses = Process.GetProcesses();
            if (Filter.Text != null)
            {
                foreach (var process in currentProcesses)
                    if (ContainsIgnoreCase(process.ProcessName, Filter.Text))
                        Processes.Add(new ProcessInfo()
                        {
                            SNo = count++,
                            ProcessName = process.ProcessName,
                            Id = process.Id,
                            Responsive = process.Responding,
                            Memory = process.PrivateMemorySize64 / 1000000,
                            CurrentState = process.MainWindowTitle

                        });
            }
            else
            {
                foreach (var process in currentProcesses)
                    Processes.Add(new ProcessInfo()
                    {
                        ProcessName = process.ProcessName,
                        Id = process.Id,
                        Responsive = process.Responding,
                        Memory = process.PrivateMemorySize64 / 1000000,
                        CurrentState = process.MainWindowTitle

                    });

            }

        }

        private void UpdateProcesses()
        {
            var processesSnapShot = new ObservableCollection<ProcessInfo>();
            foreach (var process in Processes)
                processesSnapShot.Add(process);

            UpdatedProcesses = new ObservableCollection<ProcessInfo>();
            var count = 1;
            foreach (var process in Process.GetProcesses())
                UpdatedProcesses.Add(new ProcessInfo()
                {
                    SNo = count++,
                    ProcessName = process.ProcessName,
                    Id = process.Id,
                    Responsive = process.Responding,
                    Memory = process.PrivateMemorySize64 / 1000000,
                    CurrentState = process.MainWindowTitle

                });
            var stoppedProcesse = Processes.Except(UpdatedProcesses).ToList();
            foreach (var stpprocess in stoppedProcesse)
                foreach(var process in processesSnapShot)
                    if(process.Id == stpprocess.Id)
                        Processes.Remove(process);

            var newPreocesses = UpdatedProcesses.Except(Processes).ToList();
            foreach(var newprocess  in newPreocesses)
                    Processes.Add(newprocess);
        }
        //private ObservableCollection<ProcessInfo> ConvertProcessArrayToCollection(Process[] processes)
        //{
        //    var newProcess = new ObservableCollection<ProcessInfo>();
        //    foreach (var process in processes)
        //        newProcess.Add(new ProcessInfo()
        //        {
        //            ProcessName = process.ProcessName,
        //            Id = process.Id,
        //            Responsive = process.Responding,
        //            Memory = process.PrivateMemorySize64 / 1000000,
        //            CurrentState = process.MainWindowTitle

        //        });
        //    return newProcess;
        //}

    }
    public class ProcessInfo
    {
        public int SNo { get; set; }
        public string ProcessName { get; set; }
        public int Id { get; set; }
        public bool Responsive { get; set; }
        public long Memory { get; set; }
        public string CurrentState { get; set; }

    }

}
