using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public ObservableCollection<Process> Processes { get; set; }
        private ICollectionView filterSource;
        public MainWindow()
        {
            InitializeComponent();
            ClearFilter.Visibility = Visibility.Hidden;
            LoadProcessesIntoGrid();
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
        private void OnFilterButtonClick(object sender, TextChangedEventArgs e)
        {
            //Processes.Clear();
            //var filteredProcessName = Filter.Text;
            //var processes = ProcessHelper.FilterProcessByName(filteredProcessName);
            //foreach (var process in processes)
            //    Processes.Add(process);

            //DataGrid.BindingGroup;
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
                process.Kill();
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
                process.Kill();
            }
        }
        private void LoadProcessesIntoGrid()
        {
            Processes = new ObservableCollection<Process>();
            var currentProcess = Process.GetProcesses();
            foreach (var process in currentProcess)
                Processes.Add(process);

            DataGrid.ItemsSource = Processes;
            filterSource = CollectionViewSource.GetDefaultView(DataGrid.ItemsSource);
        }
    }
}
