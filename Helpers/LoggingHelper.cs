// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using zxeltor.Types.Lib.Logging;

namespace zxeltor.Types.Lib.Helpers;

/// <summary>
///     A collection of static helpers used to manage log4net logging for the application.
/// </summary>
public class LoggingHelper
{
    #region Static Fields and Constants

    private static readonly ILog Log = LogManager.GetLogger(typeof(LoggingHelper));

    #endregion

    #region Public Members

    /// <summary>
    ///     Attempt to add our custom appender, so we can handle log4net messages via application event.
    /// </summary>
    /// <param name="appenderName">The name of the appender</param>
    /// <param name="appender">A reference to the appender</param>
    /// <returns>True if successful. False otherwise.</returns>
    public static bool TryAddingLoggingEventAppender(string appenderName, out LoggingEventAppender? appender)
    {
        appender = null;

        try
        {
            var h = (Hierarchy)LogManager.GetRepository();
            h.Root.Level = Level.All;

            appender = CreateLoggingEventAppender(appenderName);

            if (appender != null)
            {
                h.Root.AddAppender(appender);
                h.Configured = true;
                return true;
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to add DataGridCollectionAppender", e);
        }

        return false;
    }

    /// <summary>
    ///     Configure log4net by setting a log4net.config from the root application folder, if it's available.
    ///     <para>
    ///         If running in a development environment, this logic will look for another version of log4net.config named
    ///         log4net.Development.config to use while debugging. If the development version
    ///         isn't available, it will default to log4net.config, if it can.
    ///     </para>
    /// </summary>
    public static bool TryConfigureLog4NetLogging(out bool isUsingDevelopmentConfig)
    {
#if DEBUG
        var isDevelopment = true;
#else
        var isDevelopment = false;
#endif

        isUsingDevelopmentConfig = false;

        if (isDevelopment)
        {
            var devVersionOfSettingsFilePath = Path.Combine(AssemblyInfoHelper.GetMainApplicationRootFolder(),
                "Log4Net.Development.config");

            if (File.Exists(devVersionOfSettingsFilePath))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(devVersionOfSettingsFilePath));
                isUsingDevelopmentConfig = true;
                return true;
            }
        }

        if (!isUsingDevelopmentConfig)
        {
            var settingsFilePath = Path.Combine(AssemblyInfoHelper.GetMainApplicationRootFolder(), "Log4Net.config");

            if (File.Exists(settingsFilePath))
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(settingsFilePath));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Attempt to change the log4net file appender logging level programmatically.
    /// </summary>
    /// <param name="enableDebugLogging">True to enable debug logging. False otherwise.</param>
    /// <returns>True of successful. False otherwise.</returns>
    public static bool TrySettingLog4NetLogLevel(bool enableDebugLogging)
    {
        try
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            if (hierarchy == null) return false;

            var appenders = hierarchy.GetAppenders();
            if (appenders == null || appenders.Length == 0) return false;

            var appender = appenders.FirstOrDefault(app => app.Name.Equals("file"));
            if (appender == null) return false;

            var fileAppender = appender as FileAppender;
            if (fileAppender == null) return false;

            fileAppender.ClearFilters();

            if (enableDebugLogging)
                fileAppender.AddFilter(new LevelRangeFilter { LevelMin = Level.Debug, LevelMax = Level.Fatal });
            else
                fileAppender.AddFilter(new LevelRangeFilter { LevelMin = Level.Error, LevelMax = Level.Fatal });


            return true;
        }
        catch (Exception e)
        {
            Log.Error("Failed to set application log level", e);
        }

        return false;
    }

    #endregion

    #region Other Members

    /// <summary>
    ///     Attempt to create our custom appender.
    /// </summary>
    /// <param name="appenderName">A name for the appender</param>
    /// <returns>A handle to the appender</returns>
    private static LoggingEventAppender? CreateLoggingEventAppender(string appenderName)
    {
        try
        {
            var appender = new LoggingEventAppender(appenderName);
            var layout = new PatternLayout();
            layout.ConversionPattern = "% message %";
            layout.ActivateOptions();
            appender.Layout = layout;
            appender.ActivateOptions();
            return appender;
        }
        catch (Exception e)
        {
            Log.Error("Failed to create LoggingEventAppender", e);
        }

        return null;
    }

    #endregion
}