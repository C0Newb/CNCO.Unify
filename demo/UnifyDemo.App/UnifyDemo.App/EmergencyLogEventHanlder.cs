using System;
using System.Diagnostics;
using CNCO.Unify.Logging;

namespace UnifyDemo.App;

internal class EmergencyLogEventHanlder : ILogger
{
  public void Alert(string message) { }

  public void Alert(string section, string message) { }

  public void Debug(string message) { }

  public void Debug(string section, string message) { }

  public void Emergency(string message) => Debugger.Break();

  public void Emergency(string section, string message) => Debugger.Break();

  public void Error(string message, Exception? exception = null) { }

  public void Error(string section, string message, Exception? exception = null) { }

  public void Info(string message) { }

  public void Info(string section, string message) { }

  public void Log(string message) { }

  public void Log(string section, string message) { }

  public void Log(LogLevel logLevel, string section, string message)
  {
    if (logLevel == LogLevel.Emergency)
    {
      Debugger.Break();
    }
  }

  public void Notice(string message) { }

  public void Notice(string section, string message) { }

  public void Verbose(string message) { }

  public void Verbose(string section, string message) { }

  public void Warning(string message) { }

  public void Warning(string section, string message) { }
}
