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
    private static string getCallingClass()
    {
        StackTrace stackTrace = new StackTrace();
        if (stackTrace.FrameCount >= 3)
        {
            Type callingClass = stackTrace.GetFrame(2).GetMethod().DeclaringType;
            //Console.WriteLine($"Calling class: {callingClass?.Name}");
            return callingClass?.Name;
        }
        else
        {
            return "n/a";
            //Console.WriteLine("Unable to determine calling class.");
        }
        return null;
    }
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


        Logger.Log("Log test");
        Logger.Warning("Warn test");
        Logger.Error("Error test");

        // Now you can write to the new console window
        Logger.Log("Logging initialized.");


        // Close the process when done
        //process.WaitForExit();
    }

    public static void Log(string message)
    {
        string formattedString = $"[INFO    : {getCallingClass()}] {message}";

        Console.ForegroundColor = ConsoleColor.Gray;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.Log(formattedString);
    }

    public static void Warning(string message)
    {
        string formattedString = $"[WARNING : {getCallingClass()}] {message}";

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.LogWarning(formattedString);

        Console.ForegroundColor = ConsoleColor.Gray;
    }
    public static void Error(string message)
    {
        string formattedString = $"[ERROR   : {getCallingClass()}] {message}";

        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.LogError(formattedString);

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // Import the AttachConsole function from kernel32.dll
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AttachConsole(uint dwProcessId);
}
