// Copyright (c) 2024, Todd Taylor (https://github.com/zxeltor)
// All rights reserved.
// 
// This source code is licensed under the Apache-2.0-style license found in the
// LICENSE file in the root directory of this source tree.

using log4net.Appender;
using log4net.Core;
using log4net.Filter;

namespace zxeltor.Types.Lib.Logging;

public class LoggingEventAppender : AppenderSkeleton
{
    #region Constructors

    public LoggingEventAppender(string appenderName)
    {
        this.Name = appenderName;
        this.Threshold = Level.Debug;
        this.AddFilter(new LevelRangeFilter() { LevelMin = Level.Debug, LevelMax = Level.Fatal });
    }

    #endregion

    #region Public Members

    public event EventHandler<LoggingEvent>? LoggingEvent;

    #endregion

    #region Other Members

    #region Overrides of AppenderSkeleton

    /// <inheritdoc />
    protected override void Append(LoggingEvent loggingEvent)
    {
        if (this.LoggingEvent != null) this.LoggingEvent.Invoke(this, loggingEvent);
    }

    #endregion

    #endregion
}