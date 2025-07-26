using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace C5SMSSystem
{
    public class pduSMS : pduSMSBase
    {
        #region Members
        protected bool _moreMessagesToSend;
        protected bool _rejectDuplicates;
        protected byte _messageReference;
        protected string _phoneNumber;
        protected byte _protocolIdentifier;
        protected byte _dataCodingScheme;
        protected byte _validityPeriod;
        protected DateTime _serviceCenterTimeStamp;
        protected string _userData;
        protected byte[] _userDataHeader;
        protected string _message;
        #endregion

        public pduSMS()
        {
            GetGSM7Char = CreateGSM7CharTable();
        }

        #region Properties
        public static Dictionary<char, Gsm7Characters> GetGSM7Char { get; set; }

        public DateTime ServiceCenterTimeStamp { get { return _serviceCenterTimeStamp; } }

        public byte MessageReference
        {
            get { return _messageReference; }
            set { _messageReference = value; }
        }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                //if (value.Length > 70)
                //    throw new ArgumentOutOfRangeException("Message.Length", value.Length, "Message length can not be greater that 70 chars.");

                _message = value;
            }
        }

        public bool RejectDuplicates
        {
            get
            {
                if (Direction == SMSDirection.Received)
                    throw new InvalidOperationException("Received message can not contains 'reject duplicates' property");

                return _rejectDuplicates;
            }
            set
            {
                if (Direction == SMSDirection.Received)
                    throw new InvalidOperationException("Received message can not contains 'reject duplicates' property");

                _rejectDuplicates = value;
            }
        }

        public bool MoreMessagesToSend
        {
            get
            {
                if (Direction == SMSDirection.Received)
                    throw new InvalidOperationException("Submited message can not contains 'more message to send' property");

                return _moreMessagesToSend;
            }
            set
            {
                if (Direction == SMSDirection.Received)
                    throw new InvalidOperationException("Submited message can not contains 'more message to send' property");

                _moreMessagesToSend = value;
            }
        }

        public TimeSpan ValidityPeriod
        {
            get
            {
                if (_validityPeriod > 196)
                    return new TimeSpan((_validityPeriod - 192) * 7, 0, 0, 0);

                if (_validityPeriod > 167)
                    return new TimeSpan((_validityPeriod - 166), 0, 0, 0);

                if (_validityPeriod > 143)
                    return new TimeSpan(12, (_validityPeriod - 143) * 30, 0);

                return new TimeSpan(0, (_validityPeriod + 1) * 5, 0);
            }
            set
            {
                if (value.Days > 441)
                    throw new ArgumentOutOfRangeException("TimeSpan.Days", value.Days, "Value must be not greater 441 days.");

                if (value.Days > 30) //Up to 441 days
                    _validityPeriod = (byte)(192 + (int)(value.Days / 7));
                else if (value.Days > 1) //Up to 30 days
                    _validityPeriod = (byte)(166 + value.Days);
                else if (value.Hours > 12) //Up to 24 hours
                    _validityPeriod = (byte)(143 + (value.Hours - 12) * 2 + value.Minutes / 30);
                else if (value.Hours > 1 || value.Minutes > 1) //Up to 12 days
                    _validityPeriod = (byte)(value.Hours * 12 + value.Minutes / 5 - 1);
                else
                {
                    _validityPeriodFormat = ValidityPeriodFormat.FieldNotPresent;

                    return;
                }

                _validityPeriodFormat = ValidityPeriodFormat.Relative;
            }
        }

        public virtual byte[] UserDataHeader { get { return _userDataHeader; } }

        #region "in parts" message properties
        public bool InParts
        {
            get
            {
                if (_userDataHeader == null || _userDataHeader.Length < 5)
                    return false;

                return (_userDataHeader[0] == 0x00 && _userDataHeader[1] == 0x03); // | 08 04 00 | 9F 02 | i have this header from siemenes in "in parts" message
            }
        }

        public int InPartsID
        {
            get
            {
                if (!InParts)
                    return 0;

                return (_userDataHeader[2] << 8) + _userDataHeader[3];
            }
        }

        public int Part
        {
            get
            {
                if (!InParts)
                    return 0;

                return _userDataHeader[4];
            }
        }
        #endregion

        public override SMSType Type { get { return SMSType.SMS; } }
        #endregion

        #region Public Statics
        public static void Fetch(pduSMS sms, ref string source)
        {
            pduSMSBase.Fetch(sms, ref source);

            if (sms._direction == SMSDirection.Submited)
                sms._messageReference = PopByte(ref source);

            sms._phoneNumber = PopPhoneNumber(ref source);
            sms._protocolIdentifier = PopByte(ref source);
            sms._dataCodingScheme = PopByte(ref source);

            if (sms._direction == SMSDirection.Submited)
                sms._validityPeriod = PopByte(ref source);

            if (sms._direction == SMSDirection.Received)
                sms._serviceCenterTimeStamp = PopDate(ref source);

            sms._userData = source;

            if (source == string.Empty)
                return;

            int userDataLength = PopByte(ref source);

            if (userDataLength == 0)
                return;

            if (sms._userDataStartsWithHeader)
            {
                byte userDataHeaderLength = PopByte(ref source);

                sms._userDataHeader = PopBytes(ref source, userDataHeaderLength);

                userDataLength -= userDataHeaderLength + 1;
            }

            if (userDataLength == 0)
                return;

            switch ((SMSEncoding)sms._dataCodingScheme & SMSEncoding.ReservedMask)
            {
                case SMSEncoding._7bit:
                    sms._message = Decode7bit(source, userDataLength);
                    break;
                case SMSEncoding._8bit:
                    sms._message = Decode8bit(source, userDataLength);
                    break;
                case SMSEncoding.UCS2:
                    sms._message = DecodeUCS2(source, userDataLength);
                    break;
            }
        }
        #endregion

        #region Publics

        public override void ComposePDUType()
        {
            base.ComposePDUType();

            if (_moreMessagesToSend || _rejectDuplicates)
                _pduType = (byte)(_pduType | 0x04);
        }

        public virtual string Compose(SMSEncoding messageEncoding, int totalParts, int Part)
        {
            ComposePDUType();

            string encodedData = "00"; //Length of SMSC information. Here the length is 0, which means that the SMSC stored in the phone should be used. Note: This octet is optional. On some phones this octet should be omitted! (Using the SMSC stored in phone is thus implicit)

            //encodedData += Convert.ToString(_pduType, 16).PadLeft(2, '0'); //PDU type (forst octet)
            encodedData += "41";   // UDH + UD
            encodedData += Convert.ToString(MessageReference, 16).PadLeft(2, '0');
            encodedData += EncodePhoneNumber(PhoneNumber);
            encodedData += "00"; //Protocol identifier (Short Message Type 0)
            encodedData += Convert.ToString((int)messageEncoding, 16).PadLeft(2, '0'); //Data coding scheme


            #region SMS Validity
            //if (_validityPeriodFormat != ValidityPeriodFormat.FieldNotPresent)
            //{
            //    //encodedData += Convert.ToString(_validityPeriod, 16).PadLeft(2, '0'); //Validity Period
            //    _validityPeriod = 255;
            //    encodedData += Convert.ToString(_validityPeriod, 16).PadLeft(2, '0'); //Validity Period
            //}
            #endregion

            byte[] messageBytes = null;

            switch (messageEncoding)
            {
                case SMSEncoding.UCS2:
                    messageBytes = EncodeUCS2(_message);
                    break;
                case SMSEncoding._7bit:
                    messageBytes = Encode7bit(_message);
                    break;
                default:
                    messageBytes = new byte[0];
                    break;
            }

            encodedData += Convert.ToString(_message.Length + 8, 16).PadLeft(2, '0'); //Length of message

            encodedData += AddMessageHeader(totalParts, Part);
            //encodedData += GetHexValueOfMessage(_message);
            foreach (byte b in messageBytes)
            {
                if (b != 0)
                    encodedData += Convert.ToString(b, 16).PadLeft(2, '0');
            }

            return encodedData.ToUpper();
        }

        private string GetHexValueOfMessage(string messaage)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char item in messaage)
            {
                sb.Append(GetHexofGSMChar(item));
            }
            return sb.ToString();
        }

        private string AddMessageHeader(int totalParts, int part)
        {
            string messageHeader = string.Empty;
            string strTotalParts = totalParts.ToString();
            string strPart = part.ToString();

            messageHeader = "0608040000" + strTotalParts.PadLeft(2, '0') + strPart.PadLeft(2, '0');

            return messageHeader;
        }

        public static byte[] GSMChar(string PlainText)
        {
            // ` is not a conversion, just a untranslatable letter
            string strGSMTable = "";
            strGSMTable += "@£$¥èéùìòÇ`Øø`Åå";
            strGSMTable += "Δ_ΦΓΛΩΠΨΣΘΞ`ÆæßÉ";
            strGSMTable += " !\"#¤%&'()*=,-./";
            strGSMTable += "0123456789:;<=>?";
            strGSMTable += "¡ABCDEFGHIJKLMNO";
            strGSMTable += "PQRSTUVWXYZÄÖÑÜ`";
            strGSMTable += "¿abcdefghijklmno";
            strGSMTable += "pqrstuvwxyzäöñüà";

            string strExtendedTable = "";
            strExtendedTable += "````````````````";
            strExtendedTable += "````^```````````";
            strExtendedTable += "````````{}`````\\";
            strExtendedTable += "````````````[~]`";
            strExtendedTable += "|```````````````";
            strExtendedTable += "````````````````";
            strExtendedTable += "`````€``````````";
            strExtendedTable += "````````````````";

            //string strGSMOutput = "";
            //foreach (char cPlainText in PlainText.ToCharArray())
            //{
            //    int intGSMTable = strGSMTable.IndexOf(cPlainText);
            //    if (intGSMTable != -1)
            //    {
            //        strGSMOutput += intGSMTable.ToString("X2");
            //        continue;
            //    }
            //    int intExtendedTable = strExtendedTable.IndexOf(cPlainText);
            //    if (intExtendedTable != -1)
            //    {
            //        strGSMOutput += (27).ToString("X2");
            //        strGSMOutput += intExtendedTable.ToString("X2");
            //    }
            //}
            List<byte> bt = new List<byte>();
            foreach (var item in PlainText)
            {
                int intGSMTable = strGSMTable.IndexOf(item);
                if (intGSMTable != -1)
                {
                    bt.Add((byte)intGSMTable);
                    continue;
                }
                int intExtendedTable = strExtendedTable.IndexOf(item);
                if (intExtendedTable != -1)
                {
                    bt.Add((byte)intExtendedTable);
                }
            }
            return bt.ToArray();
        }

        public string EncodeMultiPartHeader(byte[] header)
        {
            byte mask = 0;
            byte shiftedMask = 0;
            byte bitsRequired = 0;
            byte invertMask = 0;
            byte previuosbitsRequired = 0;
            //header will now contain 7 bytes
            byte[] encodedHeader = new byte[7];
            int i = 0;
            for (i = 0; i < header.Length; i++)
            {
                mask = (byte)((mask * 2) + 1);
                shiftedMask = (byte)(mask << 7 - i);
                bitsRequired = (byte)(header[i] & shiftedMask);
                invertMask = (byte)~shiftedMask;
                encodedHeader[i] = (byte)(header[i] & invertMask);
                bitsRequired = (byte)(bitsRequired >> 7 - i);
                encodedHeader[i] = (byte)(encodedHeader[i] << i);
                encodedHeader[i] = (byte)(encodedHeader[i] | previuosbitsRequired);
                previuosbitsRequired = bitsRequired;
            }
            encodedHeader[i] = previuosbitsRequired;

            return Encoding.ASCII.GetString(encodedHeader);
        }

        public static byte[] Encode7bit(string sms)
        {
            //Get bytes for this SMS string
            //byte[] smsBytes = Encoding.ASCII.GetBytes(sms);
            byte[] smsBytes = GetGSM7Byte(sms);
            byte mask = 0;
            byte shiftedMask = 0;
            byte bitsRequired = 0;
            byte invertMask = 0;
            int encodedMessageIndex = 0;
            int shiftCount = 0;
            //calculating new encoded message length
            int arrayLength = (int)(smsBytes.Length - Math.Floor(((double)sms.Length / 8)));
            byte[] encodedMessage = new byte[arrayLength];
            int i = 0;
            for (i = 0; i < smsBytes.Length; i++)
            {
                mask = (byte)((mask * 2) + 1);
                if (i < smsBytes.Length - 1)
                {
                    bitsRequired = (byte)(smsBytes[i + 1] & mask);
                    shiftedMask = (byte)(bitsRequired << 7 - shiftCount);
                    encodedMessage[encodedMessageIndex] = (byte)(smsBytes[i] >> shiftCount);
                    encodedMessage[encodedMessageIndex] = (byte)(encodedMessage[encodedMessageIndex] | shiftedMask);
                }
                else
                {
                    //last byte
                    encodedMessage[encodedMessageIndex] = (byte)(smsBytes[i] >> shiftCount);
                }
                encodedMessageIndex++;
                shiftCount++;
                //reseting the cycle when 1 ASCII is completely packed
                if (shiftCount == 7)
                {
                    i++;
                    mask = 0;
                    shiftCount = 0;
                }
            }
            //Encoded Message
            return encodedMessage;
        }

        private static byte[] GetGSM7Byte(string sms)
        {
            List<byte> bt = new List<byte>();
            foreach (var item in sms)
            {
                if (item == '{' || item == '}' || item == '`' || item == '~' || item == '\\' || item == '\t' || item == '^')
                    bt.Add((byte)42);
                else
                    bt.Add((byte)GetGSM7Char.Single(x => x.Key == item).Value.DecimalValue);
            }
            return bt.ToArray();
        }

        private string GetHexofGSMChar(char val)
        {
            Gsm7Characters result = GetGSM7Char.Single(x => x.Key == val).Value;
            return result.HexaValue;
        }

        private Dictionary<char, Gsm7Characters> CreateGSM7CharTable()
        {
            Dictionary<char, Gsm7Characters> gsm7Char = new Dictionary<char, Gsm7Characters>();
            gsm7Char.Add('@', new Gsm7Characters { Character = '@', DecimalValue = 0, HexaValue = "00" });
            gsm7Char.Add('£', new Gsm7Characters { Character = '£', DecimalValue = 1, HexaValue = "01" });
            gsm7Char.Add('$', new Gsm7Characters { Character = '$', DecimalValue = 2, HexaValue = "02" });
            gsm7Char.Add('¥', new Gsm7Characters { Character = '¥', DecimalValue = 3, HexaValue = "03" });
            gsm7Char.Add('è', new Gsm7Characters { Character = 'è', DecimalValue = 4, HexaValue = "04" });
            gsm7Char.Add('é', new Gsm7Characters { Character = 'é', DecimalValue = 5, HexaValue = "05" });
            gsm7Char.Add('ù', new Gsm7Characters { Character = 'ù', DecimalValue = 6, HexaValue = "06" });
            gsm7Char.Add('ì', new Gsm7Characters { Character = 'ì', DecimalValue = 7, HexaValue = "07" });
            gsm7Char.Add('ò', new Gsm7Characters { Character = 'ò', DecimalValue = 8, HexaValue = "08" });
            gsm7Char.Add('Ç', new Gsm7Characters { Character = 'Ç', DecimalValue = 9, HexaValue = "09" });
            gsm7Char.Add('\n', new Gsm7Characters { Character = '\n', DecimalValue = 10, HexaValue = "0A" });
            gsm7Char.Add('Ø', new Gsm7Characters { Character = 'Ø', DecimalValue = 11, HexaValue = "0B" });
            gsm7Char.Add('ø', new Gsm7Characters { Character = 'ø', DecimalValue = 12, HexaValue = "0C" });
            gsm7Char.Add('\r', new Gsm7Characters { Character = '\r', DecimalValue = 13, HexaValue = "0D" });
            gsm7Char.Add('Å', new Gsm7Characters { Character = 'Å', DecimalValue = 14, HexaValue = "0E" });
            gsm7Char.Add('å', new Gsm7Characters { Character = 'å', DecimalValue = 15, HexaValue = "0F" });
            gsm7Char.Add('Δ', new Gsm7Characters { Character = 'Δ', DecimalValue = 16, HexaValue = "10" });
            gsm7Char.Add('_', new Gsm7Characters { Character = '_', DecimalValue = 17, HexaValue = "11" });
            gsm7Char.Add('Φ', new Gsm7Characters { Character = 'Φ', DecimalValue = 18, HexaValue = "12" });
            gsm7Char.Add('Γ', new Gsm7Characters { Character = 'Γ', DecimalValue = 19, HexaValue = "13" });
            gsm7Char.Add('Λ', new Gsm7Characters { Character = 'Λ', DecimalValue = 20, HexaValue = "14" });
            gsm7Char.Add('Π', new Gsm7Characters { Character = 'Π', DecimalValue = 22, HexaValue = "16" });
            gsm7Char.Add('Ψ', new Gsm7Characters { Character = 'Ψ', DecimalValue = 23, HexaValue = "17" });
            gsm7Char.Add('Ω', new Gsm7Characters { Character = 'Ω', DecimalValue = 21, HexaValue = "15" });
            gsm7Char.Add('Σ', new Gsm7Characters { Character = 'Σ', DecimalValue = 24, HexaValue = "18" });
            gsm7Char.Add('Θ', new Gsm7Characters { Character = 'Θ', DecimalValue = 25, HexaValue = "19" });
            gsm7Char.Add('Ξ', new Gsm7Characters { Character = 'Ξ', DecimalValue = 26, HexaValue = "1A" });
            gsm7Char.Add('€', new Gsm7Characters { Character = '€', DecimalValue = 27, HexaValue = "1B" });
            gsm7Char.Add('Æ', new Gsm7Characters { Character = 'Æ', DecimalValue = 28, HexaValue = "1C" });
            gsm7Char.Add('æ', new Gsm7Characters { Character = 'æ', DecimalValue = 29, HexaValue = "1D" });
            gsm7Char.Add('ß', new Gsm7Characters { Character = 'ß', DecimalValue = 30, HexaValue = "1E" });
            gsm7Char.Add('É', new Gsm7Characters { Character = 'É', DecimalValue = 31, HexaValue = "1F" });
            gsm7Char.Add(' ', new Gsm7Characters { Character = ' ', DecimalValue = 32, HexaValue = "20" });
            gsm7Char.Add('!', new Gsm7Characters { Character = '!', DecimalValue = 33, HexaValue = "21" });
            gsm7Char.Add('"', new Gsm7Characters { Character = '"', DecimalValue = 34, HexaValue = "22" });
            gsm7Char.Add('#', new Gsm7Characters { Character = '#', DecimalValue = 35, HexaValue = "23" });
            gsm7Char.Add('¤', new Gsm7Characters { Character = '¤', DecimalValue = 36, HexaValue = "24" });
            gsm7Char.Add('%', new Gsm7Characters { Character = '%', DecimalValue = 37, HexaValue = "25" });
            gsm7Char.Add('&', new Gsm7Characters { Character = '&', DecimalValue = 38, HexaValue = "26" });
            gsm7Char.Add('\'', new Gsm7Characters { Character = '\'', DecimalValue = 39, HexaValue = "27" });
            gsm7Char.Add('(', new Gsm7Characters { Character = '(', DecimalValue = 40, HexaValue = "28" });
            gsm7Char.Add(')', new Gsm7Characters { Character = ')', DecimalValue = 41, HexaValue = "29" });
            gsm7Char.Add('*', new Gsm7Characters { Character = '*', DecimalValue = 42, HexaValue = "2A" });
            gsm7Char.Add('+', new Gsm7Characters { Character = '+', DecimalValue = 43, HexaValue = "2B" });
            gsm7Char.Add(',', new Gsm7Characters { Character = ',', DecimalValue = 44, HexaValue = "2C" });
            gsm7Char.Add('-', new Gsm7Characters { Character = '-', DecimalValue = 45, HexaValue = "2D" });
            gsm7Char.Add('.', new Gsm7Characters { Character = '.', DecimalValue = 46, HexaValue = "2E" });
            gsm7Char.Add('/', new Gsm7Characters { Character = '/', DecimalValue = 47, HexaValue = "2F" });
            gsm7Char.Add('0', new Gsm7Characters { Character = '0', DecimalValue = 48, HexaValue = "30" });
            gsm7Char.Add('1', new Gsm7Characters { Character = '1', DecimalValue = 49, HexaValue = "31" });
            gsm7Char.Add('2', new Gsm7Characters { Character = '2', DecimalValue = 50, HexaValue = "32" });
            gsm7Char.Add('3', new Gsm7Characters { Character = '3', DecimalValue = 51, HexaValue = "33" });
            gsm7Char.Add('4', new Gsm7Characters { Character = '4', DecimalValue = 52, HexaValue = "34" });
            gsm7Char.Add('5', new Gsm7Characters { Character = '5', DecimalValue = 53, HexaValue = "35" });
            gsm7Char.Add('6', new Gsm7Characters { Character = '6', DecimalValue = 54, HexaValue = "36" });
            gsm7Char.Add('7', new Gsm7Characters { Character = '7', DecimalValue = 55, HexaValue = "37" });
            gsm7Char.Add('8', new Gsm7Characters { Character = '8', DecimalValue = 56, HexaValue = "38" });
            gsm7Char.Add('9', new Gsm7Characters { Character = '9', DecimalValue = 57, HexaValue = "39" });
            gsm7Char.Add(':', new Gsm7Characters { Character = ':', DecimalValue = 58, HexaValue = "3A" });
            gsm7Char.Add(';', new Gsm7Characters { Character = ';', DecimalValue = 59, HexaValue = "3B" });
            gsm7Char.Add('<', new Gsm7Characters { Character = '<', DecimalValue = 60, HexaValue = "3C" });
            gsm7Char.Add('=', new Gsm7Characters { Character = '=', DecimalValue = 61, HexaValue = "3D" });
            gsm7Char.Add('>', new Gsm7Characters { Character = '>', DecimalValue = 62, HexaValue = "3E" });
            gsm7Char.Add('?', new Gsm7Characters { Character = '?', DecimalValue = 63, HexaValue = "3F" });
            gsm7Char.Add('¡', new Gsm7Characters { Character = '¡', DecimalValue = 64, HexaValue = "40" });
            gsm7Char.Add('A', new Gsm7Characters { Character = 'A', DecimalValue = 65, HexaValue = "41" });
            gsm7Char.Add('B', new Gsm7Characters { Character = 'B', DecimalValue = 66, HexaValue = "42" });
            gsm7Char.Add('C', new Gsm7Characters { Character = 'C', DecimalValue = 67, HexaValue = "43" });
            gsm7Char.Add('D', new Gsm7Characters { Character = 'D', DecimalValue = 68, HexaValue = "44" });
            gsm7Char.Add('E', new Gsm7Characters { Character = 'E', DecimalValue = 69, HexaValue = "45" });
            gsm7Char.Add('F', new Gsm7Characters { Character = 'F', DecimalValue = 70, HexaValue = "46" });
            gsm7Char.Add('G', new Gsm7Characters { Character = 'G', DecimalValue = 71, HexaValue = "47" });
            gsm7Char.Add('H', new Gsm7Characters { Character = 'H', DecimalValue = 72, HexaValue = "48" });
            gsm7Char.Add('I', new Gsm7Characters { Character = 'I', DecimalValue = 73, HexaValue = "49" });
            gsm7Char.Add('J', new Gsm7Characters { Character = 'J', DecimalValue = 74, HexaValue = "4A" });
            gsm7Char.Add('K', new Gsm7Characters { Character = 'K', DecimalValue = 75, HexaValue = "4B" });
            gsm7Char.Add('L', new Gsm7Characters { Character = 'L', DecimalValue = 76, HexaValue = "4C" });
            gsm7Char.Add('M', new Gsm7Characters { Character = 'M', DecimalValue = 77, HexaValue = "4D" });
            gsm7Char.Add('N', new Gsm7Characters { Character = 'N', DecimalValue = 78, HexaValue = "4E" });
            gsm7Char.Add('O', new Gsm7Characters { Character = 'O', DecimalValue = 79, HexaValue = "4F" });
            gsm7Char.Add('P', new Gsm7Characters { Character = 'P', DecimalValue = 80, HexaValue = "50" });
            gsm7Char.Add('Q', new Gsm7Characters { Character = 'Q', DecimalValue = 81, HexaValue = "51" });
            gsm7Char.Add('R', new Gsm7Characters { Character = 'R', DecimalValue = 82, HexaValue = "52" });
            gsm7Char.Add('S', new Gsm7Characters { Character = 'S', DecimalValue = 83, HexaValue = "53" });
            gsm7Char.Add('T', new Gsm7Characters { Character = 'T', DecimalValue = 84, HexaValue = "54" });
            gsm7Char.Add('U', new Gsm7Characters { Character = 'U', DecimalValue = 85, HexaValue = "55" });
            gsm7Char.Add('V', new Gsm7Characters { Character = 'V', DecimalValue = 86, HexaValue = "56" });
            gsm7Char.Add('W', new Gsm7Characters { Character = 'W', DecimalValue = 87, HexaValue = "57" });
            gsm7Char.Add('X', new Gsm7Characters { Character = 'X', DecimalValue = 88, HexaValue = "58" });
            gsm7Char.Add('Y', new Gsm7Characters { Character = 'Y', DecimalValue = 89, HexaValue = "59" });
            gsm7Char.Add('Z', new Gsm7Characters { Character = 'Z', DecimalValue = 90, HexaValue = "5A" });
            gsm7Char.Add('Ä', new Gsm7Characters { Character = 'Ä', DecimalValue = 91, HexaValue = "5B" });
            gsm7Char.Add('Ö', new Gsm7Characters { Character = 'Ö', DecimalValue = 92, HexaValue = "5C" });
            gsm7Char.Add('Ñ', new Gsm7Characters { Character = 'Ñ', DecimalValue = 93, HexaValue = "5D" });
            gsm7Char.Add('Ü', new Gsm7Characters { Character = 'Ü', DecimalValue = 94, HexaValue = "5E" });
            gsm7Char.Add('§', new Gsm7Characters { Character = '§', DecimalValue = 95, HexaValue = "5F" });
            gsm7Char.Add('¿', new Gsm7Characters { Character = '¿', DecimalValue = 96, HexaValue = "60" });
            gsm7Char.Add('a', new Gsm7Characters { Character = 'a', DecimalValue = 97, HexaValue = "61" });
            gsm7Char.Add('b', new Gsm7Characters { Character = 'b', DecimalValue = 98, HexaValue = "62" });
            gsm7Char.Add('c', new Gsm7Characters { Character = 'c', DecimalValue = 99, HexaValue = "63" });
            gsm7Char.Add('d', new Gsm7Characters { Character = 'd', DecimalValue = 100, HexaValue = "64" });
            gsm7Char.Add('e', new Gsm7Characters { Character = 'e', DecimalValue = 101, HexaValue = "65" });
            gsm7Char.Add('f', new Gsm7Characters { Character = 'f', DecimalValue = 102, HexaValue = "66" });
            gsm7Char.Add('g', new Gsm7Characters { Character = 'g', DecimalValue = 103, HexaValue = "67" });
            gsm7Char.Add('h', new Gsm7Characters { Character = 'h', DecimalValue = 104, HexaValue = "68" });
            gsm7Char.Add('i', new Gsm7Characters { Character = 'i', DecimalValue = 105, HexaValue = "69" });
            gsm7Char.Add('j', new Gsm7Characters { Character = 'j', DecimalValue = 106, HexaValue = "6A" });
            gsm7Char.Add('k', new Gsm7Characters { Character = 'k', DecimalValue = 107, HexaValue = "6B" });
            gsm7Char.Add('l', new Gsm7Characters { Character = 'l', DecimalValue = 108, HexaValue = "6C" });
            gsm7Char.Add('m', new Gsm7Characters { Character = 'm', DecimalValue = 109, HexaValue = "6D" });
            gsm7Char.Add('n', new Gsm7Characters { Character = 'n', DecimalValue = 110, HexaValue = "6E" });
            gsm7Char.Add('o', new Gsm7Characters { Character = 'o', DecimalValue = 111, HexaValue = "6F" });
            gsm7Char.Add('p', new Gsm7Characters { Character = 'p', DecimalValue = 112, HexaValue = "70" });
            gsm7Char.Add('q', new Gsm7Characters { Character = 'q', DecimalValue = 113, HexaValue = "71" });
            gsm7Char.Add('r', new Gsm7Characters { Character = 'r', DecimalValue = 114, HexaValue = "72" });
            gsm7Char.Add('s', new Gsm7Characters { Character = 's', DecimalValue = 115, HexaValue = "73" });
            gsm7Char.Add('t', new Gsm7Characters { Character = 't', DecimalValue = 116, HexaValue = "74" });
            gsm7Char.Add('u', new Gsm7Characters { Character = 'u', DecimalValue = 117, HexaValue = "75" });
            gsm7Char.Add('v', new Gsm7Characters { Character = 'v', DecimalValue = 118, HexaValue = "76" });
            gsm7Char.Add('w', new Gsm7Characters { Character = 'w', DecimalValue = 119, HexaValue = "77" });
            gsm7Char.Add('x', new Gsm7Characters { Character = 'x', DecimalValue = 120, HexaValue = "78" });
            gsm7Char.Add('y', new Gsm7Characters { Character = 'y', DecimalValue = 121, HexaValue = "79" });
            gsm7Char.Add('z', new Gsm7Characters { Character = 'z', DecimalValue = 122, HexaValue = "7A" });
            gsm7Char.Add('ä', new Gsm7Characters { Character = 'ä', DecimalValue = 123, HexaValue = "7B" });
            gsm7Char.Add('ö', new Gsm7Characters { Character = 'ö', DecimalValue = 124, HexaValue = "7C" });
            gsm7Char.Add('ñ', new Gsm7Characters { Character = 'ñ', DecimalValue = 125, HexaValue = "7D" });
            gsm7Char.Add('ü', new Gsm7Characters { Character = 'ü', DecimalValue = 126, HexaValue = "7E" });
            gsm7Char.Add('à', new Gsm7Characters { Character = 'à', DecimalValue = 127, HexaValue = "7F" });
            gsm7Char.Add('{', new Gsm7Characters { Character = '{', DecimalValue = 2740, HexaValue = "1B28" });
            gsm7Char.Add('}', new Gsm7Characters { Character = '}', DecimalValue = 2741, HexaValue = "1B29" });
            gsm7Char.Add('[', new Gsm7Characters { Character = '[', DecimalValue = 2760, HexaValue = "1B3C" });
            gsm7Char.Add(']', new Gsm7Characters { Character = ']', DecimalValue = 2762, HexaValue = "1B3E" });
            gsm7Char.Add('\\', new Gsm7Characters { Character = '\\', DecimalValue = 2747, HexaValue = "1B2F" });
            gsm7Char.Add('|', new Gsm7Characters { Character = '|', DecimalValue = 2764, HexaValue = "1B40" });
            gsm7Char.Add('^', new Gsm7Characters { Character = '^', DecimalValue = 2720, HexaValue = "1B14" });
            return gsm7Char;
        }

        #endregion

        public enum SMSEncoding
        {
            ReservedMask = 0x0C /*1100*/,
            _7bit = 0,
            _8bit = 0x04 /*0100*/,
            UCS2 = 0x08 /*1000*/
        }
    }
}
