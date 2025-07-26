using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace C5SMSSystem
{
     public interface ISMSdb
    {
        SMSAccessType CheckAuthorization(string _senderNumber);
        T[] GetData<T>(string _spName, string _number, long _userID) where T : IBusinessEntity, new();
        ObservableCollection<SMSLog> GetLogs();
        ObservableCollection<SMSLog> GetLogsByDate(DateTime todaysDate);
        ObservableCollection<SMSLog> GetLogsBetweenDates(DateTime fDate, DateTime tDate);
        ObservableCollection<SMSLog> GetLogsByMessageID(long _messageID);
        ObservableCollection<SMSLog> GetTestLogs();
        ObservableCollection<SMSLog> GetMaintenanceLogs();
        SMS GetSMS();
        ObservableCollection<SMS> GetSMSs();
        ObservableCollection<SMS> GetPendingSMSs();
        ObservableCollection<SMS> GetSMSsByDate(DateTime todaysDate);
        ObservableCollection<SMS> GetSMSsBetweenDates(DateTime fDate, DateTime tDate);
        bool Save(C5SMSSystem.SMS _sms);
        bool SaveSentLog(long _messageID, string _requestNumber, string _message, SentSMSStatus _sentStatus, DateTime _sentDT);
        bool Update(SMS _sms);
        bool SMSUserValidation(string phoneNo);
        bool SaveUser(SMSUser _smsUser);
        ObservableCollection<SMSUser> GetSMSUsers();
        ObservableCollection<string> GetSMSUsersByRank();
        ObservableCollection<string> GetSMSUserByName_PhoneNo();
        bool UpdateUsersDetails(SMSUser _smsUser);
        bool DeleteMultiUsers(string _userIDs);
        int CheckSMSValidity(SMSUser _smsUser);
    }
}
