using Splotch.Loader;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


public static class Logger
{
    /// <summary>
    /// It gets the function that called the function thats happening rn.
    /// Basically if A() calls B() then B() calls this, then this will tell you
    /// what class A() is from
    /// </summary>
    /// <returns>The class that called your function</returns>
    private static string GetCallingClass()
    {
        // Basically its meant for errors but it also logs every function that is called!
        StackTrace stackTrace = new StackTrace();
        if (stackTrace.FrameCount >= 3)
        {
            Type callingClass = stackTrace.GetFrame(2).GetMethod().DeclaringType;
            //Console.WriteLine($"Calling class: {callingClass?.Name}");
            return callingClass?.Name;
        }
        else
        {
            // This should never happen, but if it does here it is.
            return "n/a";
            //Console.WriteLine("Unable to determine calling class.");
        }
    }


    /// <summary>
    /// Initializes the console.
    /// 
    /// CAN ONLY BE CALLED BY SPLOTCH
    /// </summary>
    internal static void InitLogger()
    {
        VersionChecker.RunVersionChecker();

        // Create a new process
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = !Splotch.Config.LoadedSplotchConfig.consoleEnabled && !VersionChecker.updateNeeded;  // Set this to true if you want to hide the window (Might be how we disable the window)

        // Start the process
        process.Start();

        // It breaks if this isn't here
        Thread.Sleep(1000);

        // Connects the console window to bopl
        AttachConsole((uint)process.Id);

        // Sets the output to the console window thing
        StreamWriter sw = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(sw);

        // Set the title of the window
        Console.Title = $"Splotch Log version {VersionChecker.currentVersionString}";

        Application.logMessageReceived += HandleUnityLogs;

        if (VersionChecker.updateNeeded)
        {
            Console.WriteLine($"Update {VersionChecker.targetVersionString} needed! The current installed version is {VersionChecker.currentVersionString} Press [P] to open the download page or any other key to continue");

            ConsoleKeyInfo key = Console.ReadKey();
            if (key.KeyChar == 'p')
            {
                Process.Start("https://github.com/commandblox/Splotch/releases");
            }

            if (!Splotch.Config.LoadedSplotchConfig.consoleEnabled)
                process.Close();
        }

        Log("Log test");
        Warning("Warn test");
        Error("Error test");
        Debug("Debug test");


        Log("Logging initialized.");
        Log("Starting Mod logging!");
    }
    private static string PrevMSG = "";
    private static void HandleUnityLogs(string condition, string stackTrace, LogType type)
    {
        if (condition == PrevMSG)
        {
            return;
        }
        switch (type)
        {
            case LogType.Error:
                Error(condition, true);
                Error(stackTrace, true);
                break;
            case LogType.Warning:
                Warning(condition, true);
                break;
            case LogType.Log:
                Log(condition, true);
                break;
            case LogType.Exception:
                Error(condition, true);
                Error(stackTrace, true);
                break;
            case LogType.Assert:
                Error(condition, true);
                Error(stackTrace, true);
                break;
        }
    }


    /// <summary>
    /// Logs into console and output_log.txt
    /// </summary>
    public static void Log(string message, bool doublestack = false)
    {
        string formattedString;
        if (doublestack)
        {

            formattedString = $"[INFO    : Unity] {message}";
        }
        else
        {
            formattedString = $"[INFO    : {GetCallingClass()}] {message}";
        }
        //string formattedString = $"[INFO    : {GetCallingClass()}] {message}";

        PrevMSG = formattedString;

        Console.ForegroundColor = ConsoleColor.Gray;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.Log(formattedString);
    }

    /// <summary>
    /// Logs a warning into console and logs to output_log.txt
    /// </summary>
    public static void Warning(string message, bool doublestack = false)
    {
        string formattedString;
        if (doublestack)
        {

            formattedString = $"[WARNING : Unity] {message}";
        }
        else
        {
            formattedString = $"[WARNING : {GetCallingClass()}] {message}";
        }

        PrevMSG = formattedString;

        Console.ForegroundColor = ConsoleColor.Yellow;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.LogWarning(formattedString);

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    /// <summary>
    /// Logs an error into console and logs to output_log.txt
    /// </summary>
    public static void Error(string message, bool doublestack = false)
    {
        string formattedString;
        if (doublestack)
        {

            formattedString = $"[ERROR   : Unity] {message}";
        }
        else
        {
            formattedString = $"[ERROR   : {GetCallingClass()}] {message}";
        }
        //string formattedString = $"[ERROR   : {GetCallingClass()}] {message}";

        PrevMSG = formattedString;

        Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.LogError(formattedString);

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    /// <summary>
    /// Logs an debug message into console and logs to output_log.txt.
    /// 
    /// <c> VERBOSE LOGGING NEEDS TO BE ENABLED </c>
    /// </summary>
    public static void Debug(string message, bool doublestack = false)
    {
        if (!Splotch.Config.LoadedSplotchConfig.verboseLoggingEnabled)
        {
            return;
        }

        string formattedString;
        if (doublestack)
        {

            formattedString = $"[DEBUG   : Unity] {message}";
        }
        else
        {
            formattedString = $"[DEBUG   : {GetCallingClass()}] {message}";
        }

        PrevMSG = formattedString;

        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine(formattedString);
        UnityEngine.Debug.LogError(formattedString);

        Console.ForegroundColor = ConsoleColor.Gray;
    }

    // AttachConsole lets me "hook" the logger into the thing
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(uint dwProcessId);
}
