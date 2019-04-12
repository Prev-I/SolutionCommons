using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;


namespace Solution.Core.Utilities
{
    public static class BinaryUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Converts the given unsigned short to big endian. 
        /// If the host machine is big endian no convertion is being performed.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static ushort ToBigEndian_Reverse(ushort u)
        {
            byte[] data = BitConverter.GetBytes(u);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            return BitConverter.ToUInt16(data, 0);
        }

        /// <summary>
        /// Converts the given unsigned short to big endian. 
        /// If the host machine is big endian no convertion is being performed.
        /// <summary>
        /// <param name="source">The unsigned short to convert.</param>
        /// <returns>The converted unsigned short.</returns>
        public static ushort ToBigEndian(ushort source)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (ushort)(source >> 8 | source << 8);
            }
            return source;
        }

        /// <summary>
        /// Converts the given unsigned int to big endian. 
        /// If the host machine is big endian no convertion is being performed.
        /// <summary>
        /// <param name="source">The unsigned int to convert.</param>
        /// <returns>The converted unsigned int.</returns>
        public static uint ToBigEndian(uint source)
        {
            if (BitConverter.IsLittleEndian)
            {
                return (uint)(source >> 24) |
                             ((source << 8) & 0x00FF0000) |
                             ((source >> 8) & 0x0000FF00) |
                              (source << 24);
            }
            return source;
        }
    }
}
