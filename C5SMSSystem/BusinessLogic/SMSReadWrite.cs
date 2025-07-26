using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows;
using System.Collections.ObjectModel;
using C5SMSSystem.Properties;

namespace C5SMSSystem
{
    public class SMSReadWrite : ISMSReadWrite
    {
        int i = 1;

        #region Fields
        
        #endregion

        #region Properties
        public AutoResetEvent receiveNow { get; set; }
        #endregion

        #region Constructors
        public SMSReadWrite(AutoResetEvent autoReset)
        {
            this.receiveNow = autoReset;
        }
        #endregion

        #region Private Methods
        public ObservableCollection<SMS> Read(SerialPort _port, string _command)
        {
            ObservableCollection<SMS> messages = new ObservableCollection<SMS>();
            try
            {
                if (Settings.Default.isReadSMS)
                {
                    #region Execute Command

                    string input = ExecCommand(_port, "AT+CMGR=" + i, 0, "");

                    if (input.Length > 30)
                    {
                        if (!input.Contains("\"SM\",") || !input.Contains("\r\n\r\n+CMGR:"))
                        {
                            messages.Add(ParseSingleMessage(input));
                            DeleteSingleMessage(_port, i);
                        }
                        
                    }

                    i = i + 1;

                    if (i > 25)
                    {
                        i = 1;
                        Thread.Sleep(1000);
                    }
             
                    #endregion

                    #region Execute Command
                    //// Check connection
                    ////ExecCommand(_port, "AT", 300, "No phone connected");
                    //#region At Commands
                    ////// used to preinitialize modem with error type AT+CMEE=1 - Extended numeric codes ,AT+CMEE=2 - Extended syntax codes
                    ////string str = ExecCommand(_port, "ATE0+CMEE?", 300, "No phone connected");
                    ////// used to check network ? , if COPS=0 is set, automatic network selection.
                    ////string str1 = ExecCommand(_port, "AT+COPS?", 1000, "No phone connected");
                    ////// Used to check GSM Registration, if result is : 0,0 – SIM Error, 0,2 – Searching, 0,3 – Registration denied, 0,5 – Connected roaming, 0,1 - Connected
                    ////string str2 = ExecCommand(_port, "AT+CREG?", 300, "No phone connected");
                    //////Used to get signal strength: 2-9 marginal, 10-14 OK, 15-19 Good, 20-30 Excellent. 0= less then 113dbm 1= 113-29 dbm, 31= greater then 53 dbm
                    ////string str3 = ExecCommand(_port, "AT+CSQ", 300, "No phone connected");
                    //#endregion
                    //// Use message format "Text mode"
                    ////ExecCommand(_port, "AT+CMGF=1", 300, "Failed to set message format.");
                    //  //string recievedData = ExecCommand(_port, "AT+CMGF?", 300, "Failed to set message format.");
                    //// Read the messages
                    //string input;// = ExecCommand(_port, _command, 5000, "Failed to read the messages.");
                    //int i = 1;
                    //while ((input = ExecCommand(_port, "AT+CMGR=" + i, 300, "")).Length > 30)
                    //{
                    //    //input = ExecCommand(_port, "AT+CMGR=" + i, 3000, "");
                    //    messages = ParseMessages(input);
                    //    i = i + 1;

                    //    if (i > 20)
                    //    {
                    //        i = 1;
                    //        continue;
                    //    }
                    //}

                    #endregion
                    //messages = ParseMessages(input);
                }
                
                return messages;
                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                //ProsoftCommons.SaveExeptionToLog(ex);
                return messages;
            }
        }

        public static IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        public List< PartSMSs> createSMS(SerialPort C5SerialPort, string _phoneNo, string _msgString)
        {
            List<PartSMSs> result = new List< PartSMSs>(); 
            IEnumerable<string> multiPartMsg = SplitByLength(_msgString, 152);
            int partNumber = 0;
            foreach (string _part in multiPartMsg)
            {
                partNumber++;
                if (Send(C5SerialPort, _phoneNo, _part.Trim(), multiPartMsg.Count(), partNumber))
                {
                    result.Add(new PartSMSs() { PartMsgStatus = SentSMSStatus.Passed, PartMsg = _part });
                }
                else
                {
                    result.Add(new PartSMSs() { PartMsgStatus = SentSMSStatus.Failed, PartMsg = _part });
                }
            }
            return result;
        }

        public bool Send(SerialPort _port, string _phoneNo, string _message, int totalParts, int Part)
        {
            bool isSend = false;

            try
            {
                #region PDU Mode
                
                string recievedData;
                
                pduSMS sms = new pduSMS();
                string pduMessage = string.Empty;

                sms.Direction = SMSDirection.Submited;
                sms.PhoneNumber = _phoneNo.Substring(0, 2) == Settings.Default.CountryCode ? "+" + _phoneNo : _phoneNo;

                sms.ValidityPeriod = new TimeSpan(4, 0, 0, 0);
                sms.Message = _message;

                pduMessage = sms.Compose(pduSMS.SMSEncoding._7bit, totalParts, Part);

                int smsLength = 14 + 6 + (int)(sms.Message.Length - Math.Floor(((double)sms.Message.Length / 8)));

                String command = "AT+CMGS=" + smsLength + "\r";

                changeCMGF(_port, 0);
                
                ExecCommand(_port, command, 300, "Failed to accept phoneNo");
                Thread.Sleep(500);

                command = pduMessage + char.ConvertFromUtf32(26) + "\r";  // 00 padding for special (character + number) issue
                recievedData = ExecSendSMSCommand(_port, command, 2000, "Failed to send message"); //3 seconds -- ExecCommand (for old modem) -- 23-12-2016
                Thread.Sleep(500);

                changeCMGF(_port, 1);

                if (recievedData.Contains("\r\nOK\r\n"))
                {
                    isSend = true;
                }
                else if (recievedData.Equals("ERROR"))
                {
                    isSend = false;
                    ProsoftCommons.SaveExeptionToLog(new Exception("Modem internal error."));
                }
                else if (recievedData.Equals("MODEM ERROR") || recievedData.Equals("INVALID"))
                {
                    isSend = false;
                    ProsoftCommons.SaveExeptionToLog(new Exception("Modem could not send sms, low balance or modem internal error might have caused this error."));
                }

                return isSend;

                #endregion

                #region Text Mode

                //Settings.Default.isReadSMS = false;
                //string recievedData;
                ////recievedData = ExecCommand(_port, "AT", 300, "No phone connected");

                ////string recievedData1 = ExecCommand(_port, "AT+CMGF?", 300, "Failed to set message format.");
                //pduSMS sms = new pduSMS();
                //string pduMessage = string.Empty;

                //sms.Direction = SMSDirection.Submited;
                //sms.PhoneNumber = _phoneNo.Substring(0, 2) == Settings.Default.CountryCode ? "+" + _phoneNo : _phoneNo;

                //sms.ValidityPeriod = new TimeSpan(4, 0, 0, 0);
                //sms.Message = _message;

                //String command = "AT+CMGS=" + sms.PhoneNumber + "\r";

                //recievedData = ExecCommand(_port, command, 3000, "Failed to accept phoneNo");
                //command = _message + char.ConvertFromUtf32(26) + "\r";  // 00 padding for special (character + number) issue

                //recievedData = ExecCommand(_port, command, 10000, "Failed to send message"); //3 seconds

                //if (recievedData.EndsWith("\r\nOK\r\n"))
                //{
                //    isSend = true;
                //}
                //else if (recievedData.Contains("ERROR"))
                //{
                //    isSend = false;
                //}

                //Settings.Default.isReadSMS = true;

                //return isSend;

                #endregion

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                
                Settings.Default.isSendSMS = false;
                changeCMGF(_port, 1);
                Settings.Default.isReadSMS = true;
                Settings.Default.isSendSMS = true;
                return false;
            }
        }

        private void changeCMGF(SerialPort _port ,int cmgf)
        {
            String str = ExecCommand(_port, "AT+CMGF=" + cmgf, 300, "");
            Thread.Sleep(500);
           
            //while (!(str = ExecCommand(_port, "AT+CMGF?", 300, "")).Contains("CMGF: "+ cmgf +"\r\n\r\nOK\r\n"))
            //{
            //    ExecCommand(_port, "AT+CMGF=" + cmgf, 300, "");
            //}
        }
        
        #region Execute ATComands
        private string ExecCommand(SerialPort port, string command, int responseTimeout, string errorMessage)
        {
            try
            {
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                receiveNow.Reset();
                port.Write(command + "\r");

                //Thread.Sleep(responseTimeout);
                
                string input = ReadResponse(port, responseTimeout);

                if (input.Equals("INVALID"))
                {
                    input = "";
                }

                //if ((input.Length == 0) || ((!input.EndsWith("\r\n> ")) && (!input.EndsWith("\r\nOK\r\n"))))
                //    throw new ApplicationException("No success message was received.");
                return input;
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
        }

        private string ExecSendSMSCommand(SerialPort port, string command, int responseTimeout, string errorMessage)
        {
            try
            {
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                receiveNow.Reset();
                port.Write(command + "\r");

                //Thread.Sleep(responseTimeout);

                string input = ReadSendSMSResponse(port, responseTimeout);

                if (input.Equals("INVALID"))
                {
                    input = "";
                }

                //if ((input.Length == 0) || ((!input.EndsWith("\r\n> ")) && (!input.EndsWith("\r\nOK\r\n"))))
                //    throw new ApplicationException("No success message was received.");
                return input;
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
        }
        #endregion

        private string ReadResponse(SerialPort port, int timeout)
        {
            string buffer = string.Empty;
            try
            {
                string rBuffer = string.Empty;

                do
                {
                    string t = port.ReadExisting();
                    buffer += t;
                }
                while (!buffer.Contains("\r\nOK\r\n") && !buffer.Contains("\r\n> ") && !buffer.Contains("\r\nERROR\r\n") 
                    && !buffer.Contains("+CMS ERROR: PS busy") && !buffer.Contains("CMS ERROR") && !buffer.Contains("CME ERROR")
                    && !buffer.Contains("CMTI:"));

                rBuffer = buffer;

                if (buffer.Contains("+CMS ERROR: PS busy"))
                {
                    buffer = "MODEM ERROR";
                }
                else if (buffer.Contains("\r\nERROR\r\n"))
                {
                    buffer = "ERROR";
                }
                else if (buffer.Contains("CMS ERROR") || buffer.Contains("CME ERROR"))
                {
                    buffer = "ERROR";
                }
                else if (buffer.Contains("CMTI:"))
                {
                    buffer = "INVALID";
                }

                if (rBuffer != null && !rBuffer.Equals("") && !rBuffer.Equals("OK") && !rBuffer.Equals("\r\nOK\r\n") && !rBuffer.ToUpper().Contains("AT+CMGR="))
                    ProsoftCommons.LogModemResponseInTxtFile(rBuffer);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
            return buffer;
        }

        private string ReadSendSMSResponse(SerialPort port, int timeout)
        {
            string buffer = string.Empty;
            try
            {
                string rBuffer = string.Empty;

                do
                {
                    string t = port.ReadExisting();
                    buffer += t;
                }
                while (!buffer.Contains("\r\nOK\r\n") && !buffer.Contains("\r\nERROR\r\n")
                    && !buffer.Contains("+CMS ERROR: PS busy") && !buffer.Contains("CMS ERROR") && !buffer.Contains("CME ERROR")
                    && !buffer.Contains("CMTI:"));

                rBuffer = buffer;

                if (buffer.Contains("+CMS ERROR: PS busy"))
                {
                    buffer = "MODEM ERROR";
                }
                else if (buffer.Contains("\r\nERROR\r\n"))
                {
                    buffer = "ERROR";
                }
                else if (buffer.Contains("CMS ERROR") || buffer.Contains("CME ERROR"))
                {
                    buffer = "ERROR";
                }
                else if (buffer.Contains("CMTI:"))
                {
                    buffer = "INVALID";
                }

                if (rBuffer != null && !rBuffer.Equals("") && !rBuffer.Equals("OK") && !rBuffer.Equals("\r\nOK\r\n") && !rBuffer.ToUpper().Contains("AT+CMGR="))
                    ProsoftCommons.LogModemResponseInTxtFile(rBuffer);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
            return buffer;
        }

        private ObservableCollection<SMS> ParseMessages(string input)
        {
            string prvMsg = string.Empty;
            string prvSender = string.Empty;
            ObservableCollection<SMS> messages = new ObservableCollection<SMS>();
            try
            {
                //Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");

                Regex r1 = new Regex(@"\+CMGR: ""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");
                                                      
                Match m = r1.Match(input);
                while (m.Success)
                {
                    SMS msg = new SMS();
                    //msg.Index = int.Parse(m.Groups[1].Value);
                    msg.Index = m.Groups[1].Value;
                    msg.Status = m.Groups[1].Value;
                    msg.SenderNumber = m.Groups[2].Value.Replace("+", "");
                    //msg.Alphabet = m.Groups[4].Value;
                    string[] dateTime = m.Groups[4].Value.Split(new char[] { ',' });
                    string smsDate = dateTime[0];
                    string[] smsTime = dateTime[1].Split(new char[] { '+' });
                    msg.ReceivedDateTime = DateTime.ParseExact(smsDate + " " + smsTime[0], "yy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    msg.Message = m.Groups[5].Value.ToUpper().Contains("SP") ? m.Groups[5].Value.ToUpper().Replace("SP","Series") : m.Groups[5].Value;
                    msg.Message = msg.Message.Contains(",") ? msg.Message.Replace(",", " ").Trim() : msg.Message.Trim();
                    string[] msgArray = msg.Message.Split(new char[] { ' ' });
                    msg.RequestType = msgArray[0];

                    //if (msg.Message.Length < 153)
                    //{
                    //    if (prvSender == msg.SenderNumber)
                    //    {
                    //        msg.Message = prvMsg + msg.Message;
                    //        prvSender = "";
                    //    }
                    messages.Add(msg);
                    //}
                    //else if (msg.Message.Length >= 153)
                    //{
                    //    prvSender = msg.SenderNumber;
                    //    prvMsg = msg.Message;
                    //}

                    m = m.NextMatch();
                }

            }
            catch (Exception ex)
            {
                //ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
            return messages;
        }

        private SMS ParseSingleMessage(string input)
        {
            string prvMsg = string.Empty;
            string prvSender = string.Empty;
            
            SMS msg = new SMS();
            try
            {
                int requiredStringIndex = input.IndexOf ("+CMGR:");
                input = input.Substring(requiredStringIndex);
                
                //Sample response string from GSM Modem
                //+CMGR: "REC READ","+919738856040",,"14/07/17,15:52:03+22"
                //SDR 8904517127

                Regex r1 = new Regex(@"\+CMGR: ""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\n\r");
                //Regex r1 = new Regex(@"\+CMGR: ""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n"); // Before 18-July-2014

                Match m = r1.Match(input);
                if (m.Success)
                {
                    
                    //msg.Index = int.Parse(m.Groups[1].Value);
                    msg.Index = m.Groups[1].Value;
                    msg.Status = m.Groups[1].Value;
                    msg.SenderNumber = m.Groups[2].Value.Replace("+", "");
                    //msg.Alphabet = m.Groups[4].Value;
                    string[] dateTime = m.Groups[4].Value.Split(new char[] { ',' });
                    string smsDate = dateTime[0];
                    string[] smsTime = dateTime[1].Split(new char[] { '+' });
                    msg.ReceivedDateTime = DateTime.ParseExact(smsDate + " " + smsTime[0], "yy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                    msg.Message = m.Groups[5].Value.ToUpper().Contains("SP") ? m.Groups[5].Value.ToUpper().Replace("SP", "Series") : m.Groups[5].Value;
                    msg.Message = msg.Message.Contains(",") ? msg.Message.Replace(",", " ").Trim() : msg.Message.Trim();
                    string[] msgArray = msg.Message.Split(new char[] { ' ' });
                    msg.RequestType = msgArray[0];

                }

            }
            catch (Exception ex)
            {
                //ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
            return msg;
        }

        private void DeleteSingleMessage(SerialPort _port, int msgIndex)
        {
            string input = ExecCommand(_port, "AT+CMGD=" + msgIndex, 300, "Failed to read the messages.");
        }

        public bool Delete(SerialPort _port, string _command)
        {
            try
            {
                #region Execute Command
                // Check connection
                //ExecCommand(_port, "AT", 300, "No phone connected");
                // Use message format "Text mode"
                //ExecCommand(_port, "AT+CMGF=1", 300, "Failed to set message format.");
                // Use character set "PCCP437"
                //ExecCommand(_port, "AT+CSCS=\"PCCP437\"", 300, "Failed to set character set.");
                // Select SIM storage
                //ExecCommand(_port, "AT+CPMS=\"SM\"", 300, "Failed to select message storage.");
                // Read the messages
                string input = ExecCommand(_port, _command, 5000, "Failed to read the messages.");
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
            }
        }

        public string Balance(SerialPort _port, string _command)
        {
            string _bal = string.Empty;

            try
            {

                string recievedData = ExecCommand(_port, "AT", 300, "No phone connected");
                recievedData = ExecCommand(_port, "AT+CMGF=1", 300, "Failed to set message format.");
                _bal = ExecCommand(_port, _command, 3000, "Failed to check balance message"); //3 seconds

                return _bal;
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                throw ex;
            }
        }
        #endregion
    }
}
