using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5SMSSystem
{
    public static class AtComands
    {
        public static readonly string AllMessages = "AT+CMGL=\"ALL\"";
        public static readonly string ReadMessages = "AT+CMGL=\"REC READ\"";
        public static readonly string UnreadMessages = "AT+CMGL=\"REC UNREAD\"";
        public static readonly string SentMessages = "AT+CMGL=\"STO SENT\"";
        public static readonly string UnsentMessages = "AT+CMGL=\"STO UNSENT\"";
        public static readonly string DeleteAllMessages = "AT+CMGD=1,4";
        public static readonly string DeleteReadMessages = "AT+CMGD=1,3";
        public static readonly string PreferredMsgStore = "AT+CPMS?";
        public static readonly string NetworkProvider = "AT+COPS?";
        public static readonly string NetworkProvider_longstring = "AT+COPS=3,0";

        public static string CheckBalance(string servicenumber)
        {
            return string.Format("AT+CUSD=1,\"{0}\",15\r\n", servicenumber);
        }
    }
}
