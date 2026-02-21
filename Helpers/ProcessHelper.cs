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
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Execute a console application with the provided parameters.
    /// </summary>
    /// <param name="executablePath">The executable to run.</param>
    /// <param name="useShellExecute">
    ///     Optional: Indicates whether to use the operating system shell to start the process.
    ///     Defaults to false.
    /// </param>
    /// <param name="workingDirectory">Optional: Working directory.</param>
    /// <param name="executableArgs">Optional: Command line arguments for the executable.</param>
    /// <param name="consoleOutputPath">Optional: Executable command line output file.</param>
    /// <param name="cancellationToken">Optional: Cancellation token.</param>
    /// <returns>A handle to the asynchronous operation.</returns>
    public static async Task ExecuteConsoleApplication(string executablePath, bool? useShellExecute,
        string? workingDirectory,
        string? executableArgs, string? consoleOutputPath, CancellationToken? cancellationToken)
    {
        var consoleOutput = new LinkedList<string>();
        var isRedirectOutputToFile = false;

        using (var proc = new Process())
        {
            proc.StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = useShellExecute ?? false
            };

            if (!string.IsNullOrWhiteSpace(executableArgs)) proc.StartInfo.Arguments = executableArgs;
            if (!string.IsNullOrWhiteSpace(workingDirectory)) proc.StartInfo.WorkingDirectory = workingDirectory;
            if (!string.IsNullOrWhiteSpace(consoleOutputPath)) isRedirectOutputToFile = true;

            try
            {
                if (isRedirectOutputToFile)
                {
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;

                    /*
                     * Collect the console application standard output, so we can catch any miscellaneous messages and/or warnings.
                     */
                    proc.OutputDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                            consoleOutput.AddLast($"{DateTime.UtcNow:G} [STD]: {args.Data}");
                    };

                    /*
                     * Collect the console application error output, so we can catch any miscellaneous error messages.
                     */
                    proc.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrEmpty(args.Data))
                            consoleOutput.AddLast($"{DateTime.UtcNow:G} [ERR]: {args.Data}");
                    };

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                }

                proc.Start();

                //if (!string.IsNullOrWhiteSpace(consoleOutputPath))
                //{
                //    proc.BeginOutputReadLine();
                //    proc.BeginErrorReadLine();
                //}

                if (cancellationToken.HasValue)
                    await proc.WaitForExitAsync(cancellationToken.Value);
                else
                    await proc.WaitForExitAsync();

                if (!proc.HasExited)
                    proc.Kill();
            }
            catch
            {
                if (!proc.HasExited)
                    proc.Kill();

                throw;
            }
            finally
            {
                if (isRedirectOutputToFile)
                    WriteConsoleOutputToFile(consoleOutputPath, consoleOutput);
            }
        }
    }

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

    #region Other Members

    /// <summary>
    ///     Dump the console application output to console_output.txt.
    /// </summary>
    /// <param name="consoleOutputPath">The console output file path.</param>
    /// <param name="output">A string array of output results.</param>
    /// <exception cref="Exception">Failed to write file to disk.</exception>
    private static void WriteConsoleOutputToFile(string consoleOutputPath, LinkedList<string> output)
    {
        try
        {
            using (var sw = new StreamWriter(consoleOutputPath))
            {
                if (output == null || output.Count == 0)
                    sw.WriteLine(
                        $"{DateTime.UtcNow:G} [WARN]: No results were returned from the console app while the validator was running.");
                else
                    output.ToList().ForEach(line => sw.WriteLine(line));

                sw.Flush();
                sw.Close();
            }
        }
        catch (Exception e)
        {
            throw new Exception("Failed to write console output file.", e);
        }
    }

    #endregion
}