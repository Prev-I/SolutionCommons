﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;


namespace Solution.Core.Utilities
{
    public static class CmdUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Executed a command inside an externla instance of CMD
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <returns>Return the exit code of executed command</returns>
        public static int ExecuteCommandCmd(string command)
        {
            try
            {
                // Find the full path to command exe file
                string commandFullPath = FileUtil.GetFullPath(command);

                // Log command and params as debug
                log.Debug(commandFullPath);

                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + commandFullPath);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();
                // Display the command output.
                log.Debug(result);

                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Executed a command registered to the sistem passing specified parameter
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="parameter">Parameter passed to the executed command</param>
        /// <returns>Return the exit code of executed command</returns>
        public static int ExecuteCommand(string command, string parameter)
        {
            try
            {
                // Find the full path to command exe file
                string commandFullPath = FileUtil.GetFullPath(command);

                // Log command and params as debug
                log.Debug(commandFullPath + " " + parameter );

                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(commandFullPath, parameter);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();
                // Display the command output.
                log.Debug(result);

                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw new Exception("Escalated exception", e);
            }
        }

    }
}
