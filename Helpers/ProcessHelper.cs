// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Diagnostics;
using log4net;

namespace zxeltor.Types.Lib.Helpers;

public static class ProcessHelper
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessHelper));

    #region Public Members
    /// <summary>
    ///     Get the current process.
    /// </summary>
    /// <returns>A handle to the current process.</returns>
    public static Process GetCurrentProcess()
    {
        return Process.GetCurrentProcess();
    }

    /// <summary>
    ///     Used to determine the number of processes running for the provided name.
    /// </summary>
    /// <param name="processName">The name of the process.</param>
    /// <returns>The number of process instances for the provided process name</returns>
    public static int RunningProcessInstanceCount(string processName)
    {
        var allRunningProcesses = Process.GetProcesses().ToList();
        var count = allRunningProcesses.Where(proc => proc.ProcessName.Contains(processName)).ToList().Count;
        allRunningProcesses.ForEach(proc => proc.Dispose());

        return count;
    }

    #endregion
}