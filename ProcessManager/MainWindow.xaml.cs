using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public MainWindow()
        {
            InitializeComponent();
            Processes = new ObservableCollection<Process>();
            var currentProcess = Process.GetProcesses();
            foreach (var process in currentProcess)
                Processes.Add(process);
            
            ProcessTable.ItemsSource = Processes;
        }



        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Processes.Clear();
            var filteredProcessName = NameFilter.Text;
            var processes = ProcessHelper.FilterProcessByName(filteredProcessName);
            foreach (var process in processes)
                Processes.Add(process);
            
            ProcessTable.ItemsSource = Processes;
        }

        private void AddKillOrRestartProcessButton(object sender, SelectionChangedEventArgs e)
        {
            KillProcessButton.Visibility = Visibility.Visible;
            RestartProcessButton.Visibility = Visibility.Visible;
        }
    }
}
