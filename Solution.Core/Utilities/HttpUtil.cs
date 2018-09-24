using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using log4net;

/// <summary>
/// Utils to handle some quirks of http mimetypes or special characters 
/// loosely based on http://remy.supertext.ch/2012/08/clean-filenames/
/// </summary>
/// 
namespace Solution.Core.Utilities
{
    public static class HttpUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region mimetype

        public static string GetMIMEType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (extension.Length > 0 &&
                MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
            {
                return MIMETypesDictionary[extension.Remove(0, 1)];
            }
            return "unknown/unknown";
        }

        private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
        {
            {"ai", "application/postscript"},
            {"aif", "audio/x-aiff"},
            {"aifc", "audio/x-aiff"},
            {"aiff", "audio/x-aiff"},
            {"asc", "text/plain"},
            {"atom", "application/atom+xml"},
            {"au", "audio/basic"},
            {"avi", "video/x-msvideo"},
            {"bcpio", "application/x-bcpio"},
            {"bin", "application/octet-stream"},
            {"bmp", "image/bmp"},
            {"cdf", "application/x-netcdf"},
            {"cgm", "image/cgm"},
            {"class", "application/octet-stream"},
            {"cpio", "application/x-cpio"},
            {"cpt", "application/mac-compactpro"},
            {"csh", "application/x-csh"},
            {"css", "text/css"},
            {"dcr", "application/x-director"},
            {"dif", "video/x-dv"},
            {"dir", "application/x-director"},
            {"djv", "image/vnd.djvu"},
            {"djvu", "image/vnd.djvu"},
            {"dll", "application/octet-stream"},
            {"dmg", "application/octet-stream"},
            {"dms", "application/octet-stream"},
            {"doc", "application/msword"},
            {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
            {"docm","application/vnd.ms-word.document.macroEnabled.12"},
            {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
            {"dtd", "application/xml-dtd"},
            {"dv", "video/x-dv"},
            {"dvi", "application/x-dvi"},
            {"dxr", "application/x-director"},
            {"eps", "application/postscript"},
            {"etx", "text/x-setext"},
            {"exe", "application/octet-stream"},
            {"ez", "application/andrew-inset"},
            {"gif", "image/gif"},
            {"gram", "application/srgs"},
            {"grxml", "application/srgs+xml"},
            {"gtar", "application/x-gtar"},
            {"hdf", "application/x-hdf"},
            {"hqx", "application/mac-binhex40"},
            {"htm", "text/html"},
            {"html", "text/html"},
            {"ice", "x-conference/x-cooltalk"},
            {"ico", "image/x-icon"},
            {"ics", "text/calendar"},
            {"ief", "image/ief"},
            {"ifb", "text/calendar"},
            {"iges", "model/iges"},
            {"igs", "model/iges"},
            {"jnlp", "application/x-java-jnlp-file"},
            {"jp2", "image/jp2"},
            {"jpe", "image/jpeg"},
            {"jpeg", "image/jpeg"},
            {"jpg", "image/jpeg"},
            {"js", "application/x-javascript"},
            {"kar", "audio/midi"},
            {"latex", "application/x-latex"},
            {"lha", "application/octet-stream"},
            {"lzh", "application/octet-stream"},
            {"m3u", "audio/x-mpegurl"},
            {"m4a", "audio/mp4a-latm"},
            {"m4b", "audio/mp4a-latm"},
            {"m4p", "audio/mp4a-latm"},
            {"m4u", "video/vnd.mpegurl"},
            {"m4v", "video/x-m4v"},
            {"mac", "image/x-macpaint"},
            {"man", "application/x-troff-man"},
            {"mathml", "application/mathml+xml"},
            {"me", "application/x-troff-me"},
            {"mesh", "model/mesh"},
            {"mid", "audio/midi"},
            {"midi", "audio/midi"},
            {"mif", "application/vnd.mif"},
            {"mov", "video/quicktime"},
            {"movie", "video/x-sgi-movie"},
            {"mp2", "audio/mpeg"},
            {"mp3", "audio/mpeg"},
            {"mp4", "video/mp4"},
            {"mpe", "video/mpeg"},
            {"mpeg", "video/mpeg"},
            {"mpg", "video/mpeg"},
            {"mpga", "audio/mpeg"},
            {"ms", "application/x-troff-ms"},
            {"msh", "model/mesh"},
            {"mxu", "video/vnd.mpegurl"},
            {"nc", "application/x-netcdf"},
            {"oda", "application/oda"},
            {"ogg", "application/ogg"},
            {"pbm", "image/x-portable-bitmap"},
            {"pct", "image/pict"},
            {"pdb", "chemical/x-pdb"},
            {"pdf", "application/pdf"},
            {"pgm", "image/x-portable-graymap"},
            {"pgn", "application/x-chess-pgn"},
            {"pic", "image/pict"},
            {"pict", "image/pict"},
            {"png", "image/png"},
            {"pnm", "image/x-portable-anymap"},
            {"pnt", "image/x-macpaint"},
            {"pntg", "image/x-macpaint"},
            {"ppm", "image/x-portable-pixmap"},
            {"ppt", "application/vnd.ms-powerpoint"},
            {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
            {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
            {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
            {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
            {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
            {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
            {"ps", "application/postscript"},
            {"qt", "video/quicktime"},
            {"qti", "image/x-quicktime"},
            {"qtif", "image/x-quicktime"},
            {"ra", "audio/x-pn-realaudio"},
            {"ram", "audio/x-pn-realaudio"},
            {"ras", "image/x-cmu-raster"},
            {"rdf", "application/rdf+xml"},
            {"rgb", "image/x-rgb"},
            {"rm", "application/vnd.rn-realmedia"},
            {"roff", "application/x-troff"},
            {"rtf", "text/rtf"},
            {"rtx", "text/richtext"},
            {"sgm", "text/sgml"},
            {"sgml", "text/sgml"},
            {"sh", "application/x-sh"},
            {"shar", "application/x-shar"},
            {"silo", "model/mesh"},
            {"sit", "application/x-stuffit"},
            {"skd", "application/x-koan"},
            {"skm", "application/x-koan"},
            {"skp", "application/x-koan"},
            {"skt", "application/x-koan"},
            {"smi", "application/smil"},
            {"smil", "application/smil"},
            {"snd", "audio/basic"},
            {"so", "application/octet-stream"},
            {"spl", "application/x-futuresplash"},
            {"src", "application/x-wais-source"},
            {"sv4cpio", "application/x-sv4cpio"},
            {"sv4crc", "application/x-sv4crc"},
            {"svg", "image/svg+xml"},
            {"swf", "application/x-shockwave-flash"},
            {"t", "application/x-troff"},
            {"tar", "application/x-tar"},
            {"tcl", "application/x-tcl"},
            {"tex", "application/x-tex"},
            {"texi", "application/x-texinfo"},
            {"texinfo", "application/x-texinfo"},
            {"tif", "image/tiff"},
            {"tiff", "image/tiff"},
            {"tr", "application/x-troff"},
            {"tsv", "text/tab-separated-values"},
            {"txt", "text/plain"},
            {"ustar", "application/x-ustar"},
            {"vcd", "application/x-cdlink"},
            {"vrml", "model/vrml"},
            {"vxml", "application/voicexml+xml"},
            {"wav", "audio/x-wav"},
            {"wbmp", "image/vnd.wap.wbmp"},
            {"wbmxl", "application/vnd.wap.wbxml"},
            {"wml", "text/vnd.wap.wml"},
            {"wmlc", "application/vnd.wap.wmlc"},
            {"wmls", "text/vnd.wap.wmlscript"},
            {"wmlsc", "application/vnd.wap.wmlscriptc"},
            {"wrl", "model/vrml"},
            {"xbm", "image/x-xbitmap"},
            {"xht", "application/xhtml+xml"},
            {"xhtml", "application/xhtml+xml"},
            {"xls", "application/vnd.ms-excel"},
            {"xml", "application/xml"},
            {"xpm", "image/x-xpixmap"},
            {"xsl", "application/xml"},
            {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
            {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
            {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
            {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
            {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
            {"xslt", "application/xslt+xml"},
            {"xul", "application/vnd.mozilla.xul+xml"},
            {"xwd", "image/x-xwindowdump"},
            {"xyz", "chemical/x-xyz"},
            {"zip", "application/zip"}
        };

        #endregion


        #region Downloads

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return Convert.ToString(Math.Sign(byteCount) * num) + suf[place];
        }

        //http://stackoverflow.com/questions/3885964/regex-to-replace-invalid-characters
        public static string RemoveNonWordChars(string source)
        {
            return RemoveNonWordChars(source, "");
        }

        //http://stackoverflow.com/questions/3885964/regex-to-replace-invalid-characters
        public static string RemoveNonWordChars(string source, string replacement)
        {
            //\W is any non-word character (not [^a-zA-Z0-9_]).
            Regex regex = new Regex(@"[^a-zA-Z0-9_-]+");
            return regex.Replace(source, replacement);
        }

        public static string CleanFileName(string filename)
        {
            string fileEnding = null;
            int index = filename.LastIndexOf(".", StringComparison.InvariantCulture);

            //removes the file ending.
            if (index != -1)
            {
                fileEnding = filename.Substring(index + 1);
                filename = filename.Substring(0, index);
                //remove based on the CharacterReplacements list
                for (int i = 0; i < CharacterReplacements.GetLength(0); i++)
                {
                    fileEnding = fileEnding.Replace(CharacterReplacements[i, 0], CharacterReplacements[i, 1]);
                }
                //remove everything that is left
                fileEnding = "." + RemoveNonWordChars(fileEnding);
            }
            //remove based on the CharacterReplacements list
            for (int i = 0; i < CharacterReplacements.GetLength(0); i++)
            {
                filename = filename.Replace(CharacterReplacements[i, 0], CharacterReplacements[i, 1]);
            }
            //remove everything that is left
            filename = RemoveNonWordChars(filename);
            return filename + fileEnding;
        }

        private static string[,] CharacterReplacements = {
        { " ", "_"},
        { "&", "-"},
        { "?", "-"},
        { "!", "-"},
        { "%", "-"},
        { "+", "-"},
        { "#", "-"},
        { ":", "-"},
        { ";", "-"},
        { ".", "-"},

        { "¢", "c" },   //cent
        { "£", "P" },   //Pound
        { "€", "E" },   //Euro
        { "¥", "Y" },   //Yen
        { "°", "d" },   //degree
        { "¼", "1-4" }, //fraction one-quarter
        { "½", "1-2" }, //fraction half    
        { "¾", "1-3" }, //fraction three-quarters}
        { "@", "AT)"}, //at                                                  
        { "Œ", "OE" },  //OE ligature, French (in ISO-8859-15)        
        { "œ", "oe" },  //OE ligature, French (in ISO-8859-15)        
 
        {"Å","A" },  //ring
        {"Æ","AE"},  //diphthong
        {"Ç","C" },  //cedilla
        {"È","E" },  //grave accent
        {"É","E" },  //acute accent
        {"Ê","E" },  //circumflex accent
        {"Ë","E" },  //umlaut mark
        {"Ì","I" },  //grave accent
        {"Í","I" },  //acute accent
        {"Î","I" },  //circumflex accent
        {"Ï","I" },  //umlaut mark
        {"Ð","Eth"}, //Icelandic
        {"Ñ","N" },  //tilde
        {"Ò","O" },  //grave accent
        {"Ó","O" },  //acute accent
        {"Ô","O" },  //circumflex accent
        {"Õ","O" },  //tilde
        {"Ö","O" },  //umlaut mark
        {"Ø","O" },  //slash
        {"Ù","U" },  //grave accent
        {"Ú","U" },  //acute accent
        {"Û","U" },  //circumflex accent
        {"Ü","U" },  //umlaut mark
        {"Ý","Y" },  //acute accent
        {"Þ","eth"}, //Icelandic - http://en.wikipedia.org/wiki/Thorn_(letter)
        {"ß","ss"},  //German
 
        {"à","a" },  //grave accent
        {"á","a" },  //acute accent
        {"â","a" },  //circumflex accent
        {"ã","a" },  //tilde
        {"ä","ae"},  //umlaut mark
        {"å","a" },  //ring
        {"æ","ae"},  //diphthong
        {"ç","c" },  //cedilla
        {"è","e" },  //grave accent
        {"é","e" },  //acute accent
        {"ê","e" },  //circumflex accent
        {"ë","e" },  //umlaut mark
        {"ì","i" },  //grave accent
        {"í","i" },  //acute accent
        {"î","i" },  //circumflex accent
        {"ï","i" },  //umlaut mark
        {"ð","eth"}, //Icelandic
        {"ñ","n" },  //tilde
        {"ò","o" },  //grave accent
        {"ó","o" },  //acute accent
        {"ô","o" },  //circumflex accent
        {"õ","o" },  //tilde
        {"ö","oe"},  //umlaut mark
        {"ø","o" },  //slash
        {"ù","u" },  //grave accent
        {"ú","u" },  //acute accent
        {"û","u" },  //circumflex accent
        {"ü","ue"},  //umlaut mark
        {"ý","y" },  //acute accent
        {"þ","eth"}, //Icelandic - http://en.wikipedia.org/wiki/Thorn_(letter)
        {"ÿ","y" },  //umlaut mark
        };

        #endregion


        #region Base64

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool IsBase64String(string base64EncodedData)
        {
            base64EncodedData = base64EncodedData.Trim();
            return (base64EncodedData.Length % 4 == 0) && Regex.IsMatch(base64EncodedData, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        #endregion

    }
}
