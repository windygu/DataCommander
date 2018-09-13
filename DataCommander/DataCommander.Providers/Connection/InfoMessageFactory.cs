﻿using Foundation;
using Foundation.Core;

namespace DataCommander.Providers.Connection
{
    public static class InfoMessageFactory
    {
        public static InfoMessage Create(InfoMessageSeverity severity, string header, string message)
        {
            var creationTime = LocalTime.Default.Now;
            return new InfoMessage(creationTime, severity, header, message);
        }
    }
}