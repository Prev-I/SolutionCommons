using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

using Solution.Core.Utilities;
using Solution.Tools.Utilities;


namespace Solution.Cli
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog emailLog = LogManager.GetLogger("EmailLogger");


        static void Main(string[] args)
        {
            if (FileUtil.CheckAlreadyRunning())
            {
                log.Warn("Programma già avviato!");
                return;
            }

            try
            {
                //DO SOMETHING!!!

            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                emailLog.Error(e.Message, e);
            }
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }

    }
}
