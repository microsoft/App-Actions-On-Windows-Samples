// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SampleAgentLauncher.Helpers;

/// <summary>
/// Result of an ODR command execution.
/// </summary>
public record OdrCommandResult(int ExitCode, string Output, string Error)
{
    /// <summary>
    /// Gets a value indicating whether the command executed successfully.
    /// </summary>
    public bool IsSuccess => ExitCode == 0;
}

/// <summary>
/// Helper class for executing ODR (On-Device Runtime) commands.
/// </summary>
public static class OdrCommandHelper
{
    private static string? _odrCommand;

    /// <summary>
    /// Gets the ODR command path from the Windows registry.
    /// </summary>
    /// <returns>The path to the ODR command executable.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the ODR command is not found in the registry.</exception>
    public static string GetOdrCommand()
    {
        if (string.IsNullOrEmpty(_odrCommand))
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Mcp", false);
            _odrCommand = key?.GetValue("Command") as string;
        }

        if (string.IsNullOrEmpty(_odrCommand))
        {
            throw new InvalidOperationException("ODR command not found in registry.");
        }

        return _odrCommand;
    }

    /// <summary>
    /// Runs an ODR command asynchronously with the specified arguments.
    /// </summary>
    /// <param name="command">The ODR command to execute (e.g., "agent-info list").</param>
    /// <param name="argument">Optional argument to pass to the command.</param>
    /// <returns>The result of the command execution.</returns>
    public static async Task<OdrCommandResult> RunCommandAsync(string command, string? argument = null)
    {
        string arguments = string.IsNullOrEmpty(argument) ? command : $"{command} \"{argument}\"";

        ProcessStartInfo startInfo = new()
        {
            FileName = GetOdrCommand(),
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = startInfo };
        process.Start();

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
        Task<string> errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll([outputTask, errorTask]);
        string output = outputTask.Result;
        string error = errorTask.Result;

        await process.WaitForExitAsync();

        return new OdrCommandResult(process.ExitCode, output, error);
    }

    /// <summary>
    /// Gets the full command string that would be executed.
    /// </summary>
    /// <param name="command">The ODR command.</param>
    /// <param name="argument">Optional argument.</param>
    /// <returns>The full command string.</returns>
    public static string GetFullCommandString(string command, string? argument = null)
    {
        string odrPath = GetOdrCommand();
        string arguments = string.IsNullOrEmpty(argument) ? command : $"{command} \"{argument}\"";
        return $"{odrPath} {arguments}";
    }
}
