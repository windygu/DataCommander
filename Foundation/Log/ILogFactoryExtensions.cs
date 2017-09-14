﻿using System;
using System.Diagnostics;
using System.Text;

namespace Foundation.Log
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogFactoryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ILog GetTypeLog(this ILogFactory applicationLog, Type type)
        {
#if CONTRACTS_FULL
            Contract.Requires<ArgumentNullException>(applicationLog != null);
            Contract.Requires<ArgumentNullException>(type != null);
#endif

            var name = type.FullName;

            var log = applicationLog.GetLog(name);
            var foundationLog = log as FoundationLog;
            if (foundationLog != null)
            {
                foundationLog.LoggedName = type.Name;
            }

            return log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <returns></returns>
        public static ILog GetCurrentTypeLog(this ILogFactory applicationLog)
        {
            var stackFrame = new StackFrame(1, false);
            var type = stackFrame.GetMethod().DeclaringType;
            return applicationLog.GetTypeLog(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static ILog GetCurrentTypeSectionLog(this ILogFactory applicationLog, string sectionName)
        {
            var stackFrame = new StackFrame(1, false);
            var type = stackFrame.GetMethod().DeclaringType;
            var name = $"{type.FullName}.{sectionName}";
            var log = applicationLog.GetLog(name);
            var foundationLog = log as FoundationLog;
            if (foundationLog != null)
            {
                foundationLog.LoggedName = $"{type.Name}.{sectionName}";
            }

            return log;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLog"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ILog GetCurrentMethodLog(this ILogFactory applicationLog, params object[] parameters)
        {
            var stackFrame = new StackFrame(1, false);
            var method = stackFrame.GetMethod();
            var type = method.DeclaringType;
            var name = $"{type.FullName}.{method.Name}";
            var log = applicationLog.GetLog(name);
            var foundationLog = log as FoundationLog;
            if (foundationLog != null)
            {
                foundationLog.LoggedName = $"{type.Name}.{method.Name}";
            }

            if (parameters.Length > 0)
            {
                var parameterInfos = method.GetParameters();
                var sb = new StringBuilder();
                sb.AppendFormat("Entering method {0}(", method.Name);
                var count = Math.Min(parameterInfos.Length, parameters.Length);

                for (var i = 0; i < count; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    sb.AppendFormat("\r\n{0} {1}", parameterInfo.ParameterType.Name, parameterInfo.Name);
                    if (i < parameters.Length)
                    {
                        sb.Append(" = ");
                        var parameterString = ParameterValueToString(parameters[i]);
                        sb.Append(parameterString);
                    }

                    if (i < count - 1)
                    {
                        sb.Append(',');
                    }
                }

                sb.Append(')');
                var message = sb.ToString();
                log.Trace(message);
            }

            return log;
        }

        private static string ParameterValueToString(object value)
        {
            string parameterString;
            if (value != null)
            {
                parameterString = value as string;

                if (parameterString != null)
                {
                    parameterString = "\"" + parameterString + '"';
                }
                else
                {
                    parameterString = value.ToString();
                }
            }
            else
            {
                parameterString = "null";
            }
            return parameterString;
        }
    }
}