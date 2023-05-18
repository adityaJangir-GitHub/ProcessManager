using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProcessManager.Helpers;

public static class ProcessHelper
{
    public static List<Process> FilterProcessByName(string processName)
    {
        var filteredProcess = new List<Process>();
        var allProcesses = Process.GetProcesses();
        foreach (var process in allProcesses)
            if (process.ProcessName.StartsWith(processName, StringComparison.OrdinalIgnoreCase))
                filteredProcess.Add(process);

        return filteredProcess;
    }
}
