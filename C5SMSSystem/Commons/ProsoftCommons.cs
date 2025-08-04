using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Security.Cryptography;
using System.IO;
using System.Globalization;

namespace C5SMSSystem
{
    public static class ProsoftCommons
    {
        #region Members
        /// <summary>
        /// name of log file
        /// </summary>
        public static string m_logFileName;
        /// <summary>
        /// Requested tower ID length
        /// </summary>
        public static int RequestedTowerIDLength = 10;
        #endregion

        public static T GetVisualParent<T>(object childObject) where T : Visual
        {
            DependencyObject child = childObject as DependencyObject;
            while ((child != null) && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        public static void ErrorLogInTxtFile(string message)
        {
            System.IO.StreamWriter sw = null;

            try
            {
                string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";
                //string sPathName = @"E:\";
                string sPathName = @"C5SMSSystemErrorLog_";

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                sw = new System.IO.StreamWriter(sPathName + sErrorTime + ".txt", true);

                sw.WriteLine(sLogFormat + message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }
        }

        public static void LogModemResponseInTxtFile(string message)
        {
            System.IO.StreamWriter sw = null;

            try
            {
                string sLogFormat = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " ==> ";
                //string sPathName = @"E:\";
                string sPathName = @"C5SMSSystemModemResponse_";

                string sYear = DateTime.Now.Year.ToString();
                string sMonth = DateTime.Now.Month.ToString();
                string sDay = DateTime.Now.Day.ToString();

                string sErrorTime = sDay + "-" + sMonth + "-" + sYear;

                sw = new System.IO.StreamWriter(sPathName + sErrorTime + ".txt", true);

                sw.WriteLine(sLogFormat + message);
                sw.Flush();

            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Creates new log file specified by FileName
        /// or overwrites existing file.
        /// </summary>
        /// <param name="FileName">name of log file</param>
        public static void CreateLogFile(string FileName)
        {
            m_logFileName = FileName;
            // create only if not extists
            if (!System.IO.File.Exists(FileName))
            {
                System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(FileName, System.Text.Encoding.UTF8);
                xmlWriter.WriteStartDocument(true);
                xmlWriter.WriteDocType("errorlog", null, "errorlog.dtd", null);
                xmlWriter.WriteElementString("errorlog", "");
                xmlWriter.Close();
            }
        }

        /// <summary>
        /// Saves error message to log file.
        /// </summary>
        /// <param name="e" >Exception to write down into XML file</param>
        public static void SaveExeptionToLog(System.Exception e)
        {
            string strOriginal = null;

            System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(m_logFileName);
            xmlReader.WhitespaceHandling = System.Xml.WhitespaceHandling.None;
            //Moves the reader to the root element.
            // skip <?xml version="1.0" ...
            xmlReader.MoveToContent();
            strOriginal = xmlReader.ReadInnerXml();
            xmlReader.Close();
            // write new document
            System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(m_logFileName, System.Text.Encoding.UTF8);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteDocType("errorlog", null, "errorlog.dtd", null);
            xmlWriter.WriteStartElement("errorlog");
            // write original content
            xmlWriter.WriteRaw(strOriginal);
            //write down new exception
            WriteException(e, xmlWriter);
            xmlWriter.WriteEndElement();
            xmlWriter.Close();
        }

        /// <summary>
        /// Writes exception as XML 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="writer"></param>
        private static void WriteException(Exception e, System.Xml.XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("exception");
            xmlWriter.WriteElementString("time",
                System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            // write the description of exeption
            xmlWriter.WriteElementString("description",
                e.Message);

            // method where exception was thrown
            if (e.TargetSite != null)
                xmlWriter.WriteElementString("method",
                    e.TargetSite.ToString());
            // help link
            if (e.HelpLink != null)
                xmlWriter.WriteElementString("helplink",
                    e.HelpLink);
            // call stack trace
            xmlWriter.WriteStartElement("trace");
            xmlWriter.WriteString(e.StackTrace);
            xmlWriter.WriteEndElement();
            if (e.InnerException != null)
            {
                // recursively writes inner exceptions
                WriteException(e.InnerException, xmlWriter);
            }
        }

        public static string Decrypt(string decValue)
        {
            //Decrypt
            string decryptString = string.Empty;
            char decChar;
            foreach (char d in decValue)
            {

                decChar = (char)((byte)d - 128);
                decryptString = decryptString + decChar;
            }
            return decryptString;
        }

        public static string Encrypt(string encValue)
        {
            //Encrypt
            char encChar;
            string encryptString = string.Empty;
            foreach (char e in encValue)
            {
                encChar = (char)((byte)e + 128);
                encryptString = encryptString + encChar;
            }
            return encryptString;
        }

        public static string RemoveSpecialCharacters(string _msgString)
        {
            StringBuilder sb = new StringBuilder(_msgString.Length);
            foreach (char c in _msgString.ToCharArray())
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '-' ||
                    c == '/' || c == ':' || c == ' ' || c == ',' || c == '#' || (c == Convert.ToChar(@"\")) ||
                    c == '=' || c == '?' || c == '&' || c == '|' || c == '_' || c== '\n' || c== '\r'
                    ) // ADDED NEW CHARACTERS (= ? & | _). It may lead to problem while sending pdu sms
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // To decrypt password using AES 16-byte (128-bit) using Rfc2898DeriveBytes class.
        public static string DecryptFrom128Bits(string cipherText)
        {
            string decryptedString = null;
            string EncryptionKey = "C5PROV587457";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    //int blockSize = encryptor.BlockSize;
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x64, 0x65, 0x76, 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x64, 0x65, 0x76 });
                    aesAlg.Key = pdb.GetBytes(32);
                    aesAlg.IV = pdb.GetBytes(16);

                    aesAlg.Mode = CipherMode.CBC;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        decryptedString = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                decryptedString = null;
            }

            return decryptedString;
        }

        // To encrypt password using AES 16-byte (128-bit) using Rfc2898DeriveBytes class.
        public static string EncryptTo128Bits(string clearText)
        {
            string encryptedString = null;
            string EncryptionKey = "C5PROV587457";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x64, 0x65, 0x76, 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x64, 0x65, 0x76 });
                    aesAlg.Key = pdb.GetBytes(32);
                    aesAlg.IV = pdb.GetBytes(16);

                    aesAlg.Mode = CipherMode.CBC;
                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        encryptedString = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                encryptedString = null;
            }

            return encryptedString;
        }

        public static DateTime ConvertDateTime(string strDT)
        {
            DateTime _DT = DateTime.Now;
            CultureInfo provider = CultureInfo.InvariantCulture;

            try
            {
                if (strDT == null || strDT.Equals(""))
                {
                    _DT = Convert.ToDateTime("01-01-1900", provider);
                }
                else if (!DateTime.TryParseExact(strDT, "MM/dd/yyyy hh:mm:ss", provider, System.Globalization.DateTimeStyles.AssumeLocal, out _DT))
                {
                    if (!DateTime.TryParse(strDT, out _DT))
                    {
                        _DT = Convert.ToDateTime(strDT, provider);
                    }
                }
                //else
                //{

                //}
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }

            return _DT;
        }

        public static string ConvertDateTimeAndReturnString(string strDT)
        {
            string _strDT = null;
            try
            {
                DateTime _dt = ConvertDateTime(strDT);
                _strDT = _dt.ToString();
            }
            catch (Exception)
            {
                _strDT = strDT;
            }

            return _strDT;
        }
    }
}
