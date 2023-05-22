using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ProcessManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Process> Processes { get; set; }
        private ICollectionView filterSource;
        private Timer ProcessInfoUpdateTimer;
        private int _selectedIndex;


        public MainWindow()
        {
            InitializeComponent();
            ClearFilter.Visibility = Visibility.Hidden;
            GetProcesses();
            LoadProcessesIntoGrid();

            SystemProcesserCount.Text = $"Logical Processor : {Environment.ProcessorCount}";
            OSVersion.Text = $"OS Version : {Environment.OSVersion}";

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
                : _ => _ is Process process && FilterProcess(process, filterText);
        }

        private bool FilterProcess(Process process, string filterText)
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
            var process = (Process)DataGrid.SelectedItem;
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
            var process = (Process)DataGrid.SelectedItem;

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
            Processes = Process.GetProcesses().ToList();
        }

        private void UpdateProcesses()
        {
            var processesSnapShot = new List<Process>();
            foreach (var process in Processes)
                processesSnapShot.Add(process);

            var UpdatedProcesses = Process.GetProcesses().ToList();

            var closedProcesses = Processes.Except(UpdatedProcesses).ToList();

            foreach (var closedProcess in closedProcesses)
                foreach (var process in processesSnapShot)
                    if (process.Id == closedProcess.Id)
                        Processes.Remove(process);

            Processes.AddRange(UpdatedProcesses.Except(Processes).ToList());

            DataGrid.SelectedIndex = _selectedIndex;
        }

        private void RowSelected(object sender, MouseButtonEventArgs e)
        {
            _selectedIndex = DataGrid.SelectedIndex;
        }

    }
    //public class ProcessInfo
    //{
    //    public Icon Icon { get; set; } = default;
    //    public string ProcessName { get; set; }
    //    public int Id { get; set; }
    //    public bool Responsive { get; set; }
    //    public long Memory { get; set; }
    //    public string CurrentState { get; set; }

    //    public ProcessInfo(Process process)
    //    {
    //        try
    //        {
    //        }
    //        catch { }

    //    }


    //}

}
