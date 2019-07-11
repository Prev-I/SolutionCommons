using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to launch external command inside a CMD 
    /// </summary>
    public static class CmdUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Executed a command inside an external instance of CMD
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
                ProcessStartInfo procStartInfo =
                    new ProcessStartInfo("cmd", "/c " + commandFullPath)
                    {
                        // The following commands are needed to redirect the standard output.
                        // This means that it will be redirected to the Process.StandardOutput StreamReader.
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        // Do not create the black window.
                        CreateNoWindow = true
                    };
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();

                // Get the output into a string
                string error = proc.StandardError.ReadToEnd();
                string output = proc.StandardOutput.ReadToEnd();
                // Log the command output.
                if(!string.IsNullOrEmpty(error))
                    log.Error(error);
                if(!string.IsNullOrEmpty(output))
                    log.Debug(output);
                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Executed a command inside an external instance of CMD
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="output">Standard output of the executed command</param>
        /// <param name="error">Standard error of the executed command</param>
        /// <returns>Return the exit code of executed command</returns>
        public static int ExecuteCommandCmd(string command, out string output, out string error)
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
                ProcessStartInfo procStartInfo =
                    new ProcessStartInfo("cmd", "/c " + commandFullPath)
                    {
                        // The following commands are needed to redirect the standard output.
                        // This means that it will be redirected to the Process.StandardOutput StreamReader.
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        // Do not create the black window.
                        CreateNoWindow = true
                    };
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();

                // Get the output into a string
                error = proc.StandardError.ReadToEnd();
                output = proc.StandardOutput.ReadToEnd();
                // Log the command output.
                if (!string.IsNullOrEmpty(error))
                    log.Error(error);
                if (!string.IsNullOrEmpty(output))
                    log.Debug(output);
                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Executed a command registered to the system passing specified parameter
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
                log.Debug(commandFullPath + " " + parameter);

                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                ProcessStartInfo procStartInfo =
                    new ProcessStartInfo(commandFullPath, parameter)
                    {

                        // The following commands are needed to redirect the standard output.
                        // This means that it will be redirected to the Process.StandardOutput StreamReader.
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        // Do not create the black window.
                        CreateNoWindow = true
                    };
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();

                // Get the output into a string
                string error = proc.StandardError.ReadToEnd();
                string output = proc.StandardOutput.ReadToEnd();
                // Log the command output.
                if (!string.IsNullOrEmpty(error))
                    log.Error(error);
                if (!string.IsNullOrEmpty(output))
                    log.Debug(output);
                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Executed a command registered to the system passing specified parameter
        /// </summary>
        /// <param name="command">Command to be executed</param>
        /// <param name="parameter">Parameter passed to the executed command</param>
        /// <param name="output">Standard output of the executed command</param>
        /// <param name="error">Standard error of the executed command</param>
        /// <returns>Return the exit code of executed command</returns>
        public static int ExecuteCommand(string command, string parameter, out string output, out string error)
        {
            try
            {
                // Find the full path to command exe file
                string commandFullPath = FileUtil.GetFullPath(command);

                // Log command and params as debug
                log.Debug(commandFullPath + " " + parameter);

                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                ProcessStartInfo procStartInfo =
                    new ProcessStartInfo(commandFullPath, parameter)
                    {

                        // The following commands are needed to redirect the standard output.
                        // This means that it will be redirected to the Process.StandardOutput StreamReader.
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        // Do not create the black window.
                        CreateNoWindow = true
                    };
                // Now we create a process, assign its ProcessStartInfo and start it
                Process proc = new Process
                {
                    StartInfo = procStartInfo
                };
                proc.Start();

                // Get the output into a string
                error = proc.StandardError.ReadToEnd();
                output = proc.StandardOutput.ReadToEnd();
                // Log the command output.
                if (!string.IsNullOrEmpty(error))
                    log.Error(error);
                if (!string.IsNullOrEmpty(output))
                    log.Debug(output);
                //return command exitCode
                return proc.ExitCode;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

    }
}
