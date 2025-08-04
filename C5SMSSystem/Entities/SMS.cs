using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace C5SMSSystem
{
    public class SMS : ViewModelBase
    {
        #region Private Variables
        private string index;
        private long messageid;
        private string status;
        private string sender;
        private string senderNumber;
        private string alphabet;
        private DateTime receivedDateTime;
        private string message;
        private string requestType;
        private string accessType;
        #endregion

        #region Public Properties
        public string Index
        {
            get { return index; }
            set { index = value; }
        }
        public long MessageID
        {
            get { return messageid; }
            set { messageid = value; RaisePropertyChanged("MessageID"); }
        }
        public string Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged("Status"); }
        }
        public string Sender
        {
            get { return sender; }
            set { sender = value; RaisePropertyChanged("Sender"); }
        }
        public string SenderNumber
        {
            get { return senderNumber; }
            set { senderNumber = value; RaisePropertyChanged("SenderNumber"); }
        }
        public string Alphabet
        {
            get { return alphabet; }
            set { alphabet = value; }
        }
        public DateTime ReceivedDateTime
        {
            get { return receivedDateTime; }
            set { receivedDateTime = value; RaisePropertyChanged("ReceivedDateTime"); }
        }
        public string Message
        {
            get { return message; }
            set { message = value; RaisePropertyChanged("Message"); }
        }
        public string RequestType
        {
            get { return requestType; }
            set { requestType = value; RaisePropertyChanged("RequestType"); }
        }
        public string AccessType
        {
            get { return accessType; }
            set { accessType = value; RaisePropertyChanged("AccessType"); }
        }
        #endregion
    }

    public class SMSLog : ViewModelBase
    {
        #region Private Variables
        private long logId;
        private long messageId;
        private string sender;
        private string requestNumber;
        private string sentMessage;
        private string sentStatus;
        private DateTime sentDateTime;
        #endregion

        #region Public Properties
        public long LogID
        {
            get { return logId; }
            set { logId = value; RaisePropertyChanged("LogID"); }
        }
        public long MessageID
        {
            get { return messageId; }
            set { messageId = value; RaisePropertyChanged("MessageID"); }
        }
        public string SentStatus
        {
            get { return sentStatus; }
            set { sentStatus = value; RaisePropertyChanged("SentStatus"); }
        }
        public string RequestNumber
        {
            get { return requestNumber; }
            set { requestNumber = value; RaisePropertyChanged("RequestNumber"); }
        }
        public string Sender
        {
            get { return sender; }
            set { sender = value; RaisePropertyChanged("Sender"); }
        }

        public DateTime SentDateTime
        {
            get { return sentDateTime; }
            set { sentDateTime = value; RaisePropertyChanged("SentDateTime"); }
        }
        public string SentMessage
        {
            get { return sentMessage; }
            set { sentMessage = value; RaisePropertyChanged("SentMessage"); }
        }
        #endregion
    }
}
