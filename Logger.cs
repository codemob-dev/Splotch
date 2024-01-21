using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splotch;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Reflection;
using HarmonyLib;

public static class Logger
{
    public static void InitLogger()
    {
        // Create a new process
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        //process.StartInfo.CreateNoWindow = true;
        process.StartInfo.CreateNoWindow = false;  // Set this to true if you want to hide the window

        // Start the process
        process.Start();
        // Wait for a moment to ensure the new console window has time to open
        System.Threading.Thread.Sleep(1000);

        // Create a new console window and attach it to the process
        AttachConsole((uint) process.Id);

        // Redirect standard output to the new console window
        StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
        sw.AutoFlush = true;
        Console.SetOut(sw);

        // Now you can write to the new console window
        Logger.Log("Logging initialized.");

        // Close the process when done
        //process.WaitForExit();
    }

    public static void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(message);
        UnityEngine.Debug.Log(message);
    }

    public static void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        UnityEngine.Debug.LogWarning(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        UnityEngine.Debug.LogError(message);
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // Import the AttachConsole function from kernel32.dll
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AttachConsole(uint dwProcessId);
}
