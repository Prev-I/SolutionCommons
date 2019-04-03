using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Solution.Core.Extensions
{
    public static class StringExt
    {

        /// <summary>
        /// Tronca una stringa a limit caratteri
        /// </summary>
        public static string Truncate(this string str, int limit)
        {
            if (string.IsNullOrEmpty(str) || str.Length <= limit)
                return str;

            return str.Substring(0, limit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string SliceEnd(this string source, int len)
        {
            if (source.IsValid())
            {
                if (source.Length > len)
                {
                    int index = source.Length - len;
                    return source.Substring(index, len);
                }
            }

            return source;
        }

        /// <summary>
        /// Esegue la Join di un char array dalla pasizione start per la lunghezza length
        /// </summary>
        public static string JoinToString(this char[] c, int start, int length)
        {
            StringBuilder b = new StringBuilder();

            if (start >= c.Length)
                return null;

            int stop = start + length;
            if (stop > c.Length)
                stop = c.Length;

            for (int x = start; x < stop; x++)
                b.Append(c[x]);

            return b.ToString();
        }

        /// <summary>
        /// Crea una stringa ripetendo length volte "c"
        /// </summary>
        public static string CharString(this char c, int length)
        {
            if (length <= 0)
                return "";

            StringBuilder b = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                b.Append(c);

            return b.ToString();
        }

        /// <summary>
        /// Rimuove il padding a sinistra di una stringa
        /// </summary>
        public static string UnPadLeft(this string str, char padChar)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            StringBuilder b = new StringBuilder();

            char[] chrarr = str.ToCharArray();
            //scorro tutti i caratteri della stringa, da SX a DX
            for (int i = 0; i < chrarr.Length; i++)
            {
                //se il carattere è diverso dal carattere di PAD
                if (chrarr[i] != padChar)
                {
                    //crea un join e lo ritorna
                    return chrarr.JoinToString(i, chrarr.Length - i);
                }
            }

            return "";
        }

        /// <summary>
        /// Rimuove il padding a destra di una stringa
        /// </summary>
        public static string UnPadRight(this string str, char padChar)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            StringBuilder b = new StringBuilder();

            char[] chrarr = str.ToCharArray();

            //scorro tutti i caratteri della stringa, da SX a DX
            for (int i = chrarr.Length - 1; i >= 0; i--)
            {
                //se il carattere è diverso dal carattere di PAD
                if (chrarr[i] != padChar)
                {
                    //crea un join e lo ritorna
                    return chrarr.JoinToString(0, i);
                }
            }

            return "";
        }


        /// <summary>
        /// Aggiunge padding a sinistra fino a length: 1 -> 0001. Opzionalmente spezza la stringa se piu lunga.
        /// </summary>
        public static string PadLeft(this string str, char padChar, int length, bool truncate = false)
        {
            if (string.IsNullOrEmpty(str))
                return padChar.CharString(length);

            if (str.Length > length && truncate == true)
                return str.Truncate(length);

            if (str.Length < length)
                return padChar.CharString(length - str.Length) + str;

            return str;
        }

        /// <summary>
        /// Aggiunge padding a destra fino a length: 1 -> 1000. Opzionalmente spezza la stringa se piu lunga.
        /// </summary>
        public static string PadRight(this string str, char padChar, int length, bool truncate = false)
        {
            if (string.IsNullOrEmpty(str))
                return padChar.CharString(length);

            if (str.Length > length && truncate == true)
                return str.Truncate(length);

            if (str.Length < length)
                return str + padChar.CharString(length - str.Length);

            return str;
        }

        /// <summary>
        /// Verifica che l'ora "source" sia compresa fra gli orari "hourFrom" e "hourTo" (Converte in int).
        /// </summary>
        public static bool HourIsBetween(this string source, string hourFrom, string hourTo)
        {
            return int.Parse(source) >= int.Parse(hourFrom) && int.Parse(source) <= int.Parse(hourTo);
        }
    }
}
