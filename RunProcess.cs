using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuffer
{
  internal class RunProcess
  {
    private Process process;
    private StringBuilder output = new();
    private StringBuilder error = new();

    public (string Output, string Error, int ErrCode) RunProcessGrabOutput(string Executable, string Arguments, string WorkingDirectory)
    {
      int exitCode = -1;
      try
      {
        output.Clear();
        error.Clear();
        process = new Process();
        process.StartInfo.FileName = Executable;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.WorkingDirectory = WorkingDirectory;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.EnvironmentVariables.Add("PYTHONUNBUFFERED", "TRUE");

        if (!string.IsNullOrEmpty(Arguments))
          process.StartInfo.Arguments = Arguments;

        process.EnableRaisingEvents = true;
        process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputHandler);
        process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorHandler);
        process.Start(); 
        System.Diagnostics.Debug.WriteLine($"Started: {process.StartInfo.FileName} -> PID {process.Id}, MainModule={process.MainModule?.FileName}");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // You can set the priority only AFTER the you started the process.
        process.PriorityClass = ProcessPriorityClass.BelowNormal;
        process.WaitForExit();
        exitCode = process.ExitCode;
      }
      catch
      {
        // This is how we indicate that something went wrong.
        throw;
      }

      return (output.ToString(), error.ToString(), exitCode);
    }

    public Task<(string Output, string Error, int ErrCode)> RunProcessGrabOutputAsync(string Executable, string Arguments, string WorkingDirectory)
    {
      return Task.Run(() => RunProcessGrabOutput(Executable, Arguments, WorkingDirectory));
    }

    private void ProcessOutputHandler(object SendingProcess, DataReceivedEventArgs OutLine)
    {
      output.AppendLine(OutLine.Data);
    }

    private void ProcessErrorHandler(object SendingProcess, DataReceivedEventArgs OutLine)
    {
      error.AppendLine(OutLine.Data);
    }
  }
}
