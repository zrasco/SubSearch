using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SubSearchUI.Services.Concrete
{
    class MainWindowLogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public MainWindowLogSink(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }
        public void Emit(LogEvent logEvent)
        {
            // TODO: I'm certain there's a much better way to get the source context, but it's all I've been able to figure out so far
            // I'll tackle this again when I have time to comb the serilog documentation

            // Get the source context of the log
            string sourceContext = logEvent.Properties["SourceContext"].ToString().Replace("\"", "");

            if (logEvent.Exception == null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action (() => {
                    (Application.Current.MainWindow as MainWindow).AddLogEntry(DateTimeOffset.Now, sourceContext, logEvent.RenderMessage(_formatProvider), (LogLevel)logEvent.Level);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    (Application.Current.MainWindow as MainWindow).AddLogEntry(logEvent.Exception, DateTimeOffset.Now, sourceContext, logEvent.RenderMessage(_formatProvider), (LogLevel)logEvent.Level);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private LogLevel GetMSLogLevelFromSerilog(LogEventLevel serilogLevel)
        {

            LogLevel retval;

            switch (serilogLevel)
            {
                case LogEventLevel.Debug:
                    retval = LogLevel.Debug;
                    break;
                case LogEventLevel.Error:
                    retval = LogLevel.Error;
                    break;
                case LogEventLevel.Information:
                    retval = LogLevel.Information;
                    break;
                case LogEventLevel.Warning:
                    retval = LogLevel.Warning;
                    break;
                case LogEventLevel.Verbose:
                    retval = LogLevel.Trace;
                    break;
                case LogEventLevel.Fatal:
                default:
                    retval = LogLevel.Critical;
                    break;
            }

            return retval;
        }
    }
    public static class MySinkExtensions
    {
        public static LoggerConfiguration MainWindowLogSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new MainWindowLogSink(formatProvider));
        }
    }
}
