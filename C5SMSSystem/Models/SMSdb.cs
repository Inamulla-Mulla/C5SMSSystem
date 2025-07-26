using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Collections.ObjectModel;

namespace C5SMSSystem
{
    public class SMSdb : ISMSdb
    {
        #region Sql Fields
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader reader;
        bool connectionStatus = false;
        #endregion

        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructor
        public SMSdb()
        {
            if (string.IsNullOrEmpty(ConnectionManager.GetConnectionString))
            {
                connectionStatus = ConnectionManager.CreateConnectionString("C5SDR_V5", "ProsoftUser", "ProUser@123");
            }
            else
            {
                connectionStatus = true;
            }
        }
        #endregion

        #region Methods

        public bool Save(SMS _sms)
        {
            bool result = false;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spSMSRequests", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MessageID", 0);
                            cmd.Parameters.AddWithValue("@SenderNumber", _sms.SenderNumber);
                            cmd.Parameters.AddWithValue("@ReceivedDateTime", _sms.ReceivedDateTime);
                            cmd.Parameters.AddWithValue("@Message", _sms.Message);
                            cmd.Parameters.AddWithValue("@MMSLink", string.Empty);
                            cmd.Parameters.AddWithValue("@MMSCode", string.Empty);
                            cmd.Parameters.AddWithValue("@Status", "Pending");
                            cmd.Parameters.AddWithValue("@RequestType", _sms.RequestType);
                            cmd.Parameters.AddWithValue("@AccessType", "Unauthorized");
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
            }
        }

        public bool Update(SMS _sms)
        {
            bool result = false;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spSMSRequests", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MessageID", _sms.MessageID);
                            cmd.Parameters.AddWithValue("@SenderNumber", _sms.SenderNumber);
                            cmd.Parameters.AddWithValue("@ReceivedDateTime", Convert.ToDateTime(_sms.ReceivedDateTime));
                            cmd.Parameters.AddWithValue("@Message", _sms.Message);
                            cmd.Parameters.AddWithValue("@MMSLink", string.Empty);
                            cmd.Parameters.AddWithValue("@MMSCode", string.Empty);
                            cmd.Parameters.AddWithValue("@Status", _sms.Status);
                            cmd.Parameters.AddWithValue("@RequestType", _sms.RequestType);
                            cmd.Parameters.AddWithValue("@AccessType", "Authorized");
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
                //throw new Exception(ex.Message);
            }
        }

        public bool SaveSentLog(long _messageID, string _requestNumber, string _message, SentSMSStatus _sentStatus, DateTime _sentDT)
        {
            bool result = false;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spSMSRequestsLog", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MessageID", _messageID);
                            cmd.Parameters.AddWithValue("@RequestNumber", _requestNumber);
                            cmd.Parameters.AddWithValue("@SentMessage", _message);
                            cmd.Parameters.AddWithValue("@SentStatus", _sentStatus.ToString());
                            cmd.Parameters.AddWithValue("@SentDateTime", _sentDT);
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
                //throw new Exception(ex.Message);
            }
        }

        public SMSAccessType CheckAuthorization(string _senderNumber)
        {
            SMSAccessType result = SMSAccessType.Unauthorized;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("SELECT Count(*) FROM dbo.SMSUserPhone WHERE PhoneNumber=@SenderNumber", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@SenderNumber", _senderNumber);
                            if ((Int32)cmd.ExecuteScalar() > 0)
                            {
                                result = SMSAccessType.Authorized;
                            }
                        }
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return result;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMS> GetSMSs()
        {
            ObservableCollection<SMS> smsList = new ObservableCollection<SMS>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSs_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMS _smsObj = new SMS();
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.SenderNumber = reader["SenderNumber"].ToString();
                                _smsObj.ReceivedDateTime = (DateTime)reader["ReceivedDateTime"];
                                _smsObj.Message = reader["Message"].ToString();
                                _smsObj.Status = reader["Status"].ToString();
                                _smsObj.RequestType = reader["RequestType"].ToString();
                                _smsObj.AccessType = reader["AccessType"].ToString();
                                smsList.Add(_smsObj);
                            }
                        }
                       
                        cmd.Dispose();
                    }
                }
                return smsList;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsList;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMS> GetPendingSMSs()
        {
            ObservableCollection<SMS> smsList = new ObservableCollection<SMS>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetPendingSMSs_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMS _smsObj = new SMS();
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.SenderNumber = reader["SenderNumber"].ToString();
                                _smsObj.ReceivedDateTime = (DateTime)reader["ReceivedDateTime"];
                                _smsObj.Message = reader["Message"].ToString();
                                _smsObj.Status = reader["Status"].ToString();
                                _smsObj.RequestType = reader["RequestType"].ToString();
                                _smsObj.AccessType = reader["AccessType"].ToString();
                                smsList.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsList;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsList;
                //throw new Exception(ex.Message);
            }
        }
                
        public ObservableCollection<SMS> GetSMSsByDate(DateTime todaysDate)
        {
            ObservableCollection<SMS> smsList = new ObservableCollection<SMS>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSsByDate_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@TodaysDate", todaysDate.Date);
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMS _smsObj = new SMS();
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.SenderNumber = reader["SenderNumber"].ToString();
                                _smsObj.ReceivedDateTime = (DateTime)reader["ReceivedDateTime"];
                                _smsObj.Message = reader["Message"].ToString();
                                _smsObj.Status = reader["Status"].ToString();
                                _smsObj.RequestType = reader["RequestType"].ToString();
                                _smsObj.AccessType = reader["AccessType"].ToString();
                                smsList.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsList;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsList;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMS> GetSMSsBetweenDates(DateTime fDate, DateTime tDate)
        {
            ObservableCollection<SMS> smsList = new ObservableCollection<SMS>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSsBetweenDates_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FromDate", fDate.Date);
                            cmd.Parameters.AddWithValue("@ToDate", tDate.Date);
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMS _smsObj = new SMS();
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.SenderNumber = reader["SenderNumber"].ToString();
                                _smsObj.ReceivedDateTime = (DateTime)reader["ReceivedDateTime"];
                                _smsObj.Message = reader["Message"].ToString();
                                _smsObj.Status = reader["Status"].ToString();
                                _smsObj.RequestType = reader["RequestType"].ToString();
                                _smsObj.AccessType = reader["AccessType"].ToString();
                                smsList.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsList;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsList;
                //throw new Exception(ex.Message);
            }
        }

        public SMS GetSMS()
        {
            SMS _sms = new SMS();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMS_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                _sms.MessageID = (long)reader["MessageID"];
                                _sms.Sender = reader["Sender"].ToString();
                                _sms.SenderNumber = reader["SenderNumber"].ToString();
                                _sms.ReceivedDateTime = (DateTime)reader["ReceivedDateTime"];
                                _sms.Message = reader["Message"].ToString();
                                _sms.Status = reader["Status"].ToString();
                                _sms.RequestType = reader["RequestType"].ToString();
                                _sms.AccessType = reader["AccessType"].ToString();
                            }
                        }
                        cmd.Dispose();
                    }
                }
                return _sms;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return _sms;
               // throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetLogs()
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSLogs_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetLogsByDate(DateTime todaysDate)
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSLogsByDate_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@TodaysDate", todaysDate.Date);
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetLogsBetweenDates(DateTime fDate, DateTime tDate)
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSLogsBetweenDates_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@FromDate", fDate.Date);
                            cmd.Parameters.AddWithValue("@ToDate", tDate.Date);
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.Sender = reader["Sender"].ToString();
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetLogsByMessageID(long _messageID)
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("Select * from SMSRequestsLog Where MessageID = @MessageID", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@MessageID", _messageID);
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetTestLogs()
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("Select * from SMSRequestsLog Where MessageID = 0 And SentStatus = 'Test'", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSLog> GetMaintenanceLogs()
        {
            ObservableCollection<SMSLog> smsLog = new ObservableCollection<SMSLog>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("Select * from SMSRequestsLog Where MessageID = 0 And SentStatus = 'Maintenance'", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSLog _smsObj = new SMSLog();
                                _smsObj.LogID = (long)reader["SMSRequestsLogID"];
                                _smsObj.MessageID = (long)reader["MessageID"];
                                _smsObj.RequestNumber = reader["RequestNumber"].ToString();
                                _smsObj.SentDateTime = (DateTime)reader["SentDateTime"];
                                _smsObj.SentMessage = reader["SentMessage"].ToString();
                                _smsObj.SentStatus = reader["SentStatus"].ToString();
                                smsLog.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsLog;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsLog;
                //throw new Exception(ex.Message);
            }
        }

        public T[] GetData<T>(string _spName, string _number, long _userID) where T : IBusinessEntity, new()
        {
            string[] arrOfRequests = _number.Split(',');
            int noOfRequests = arrOfRequests.Length;
            T[] dataItem = new T[0];
            
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(_spName, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@SearchingFor", _number);
                            cmd.Parameters.AddWithValue("@UserID", _userID);
                            reader = cmd.ExecuteReader();
                            int i = 0, count=0;
                            while (reader.Read())
                            {
                                count++;
                            }

                            reader.Dispose();
                            reader = cmd.ExecuteReader();

                            dataItem = new T[count];
                            while (reader.Read())
                            {
                                dataItem[i] = new T();
                                dataItem[i].Fill(reader);
                                i++;
                            }
                        }
                        cmd.Dispose();
                    }
                }
                return dataItem;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return dataItem;
                //throw new Exception(ex.Message);
            }
        }

        public bool SMSUserValidation(string phoneNo)
        {
            SqlDataReader reader;
            bool result = true;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.ValidateSMSUser_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@PhoneNumber", phoneNo);
                            //result = (Int32)cmd.ExecuteNonQuery() > 0 ? true : false;
                            reader = cmd.ExecuteReader();
                            if (reader.Read())
                                result = true;
                            else
                                result = false;
                            reader.Close();
                            reader.Dispose();
                        }
                        con.Close();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
                return result;
               // throw new Exception(ex.Message);
            }
        }

        public bool SaveUser(SMSUser _smsUser)
        {
            bool result = false;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spC5SMSUserManagement", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@@SMSUserID", 0);
                            cmd.Parameters.AddWithValue("@@Name", _smsUser.UserName);
                            cmd.Parameters.AddWithValue("@@Designation", string.IsNullOrEmpty(_smsUser.Designation) ? string.Empty : _smsUser.Designation);
                            cmd.Parameters.AddWithValue("@@Rank", string.IsNullOrEmpty(_smsUser.Rank) ? string.Empty :_smsUser.Rank);
                            cmd.Parameters.AddWithValue("@@Department", string.IsNullOrEmpty(_smsUser.Department) ? string.Empty :_smsUser.Department);
                            cmd.Parameters.AddWithValue("@@Office", string.IsNullOrEmpty(_smsUser.Office) ? string.Empty :_smsUser.Office);
                            cmd.Parameters.AddWithValue("@@Address", string.IsNullOrEmpty(_smsUser.Address) ? string.Empty :_smsUser.Address);
                            cmd.Parameters.AddWithValue("@@City", string.IsNullOrEmpty(_smsUser.City) ? string.Empty :_smsUser.City);
                            cmd.Parameters.AddWithValue("@@State", string.IsNullOrEmpty(_smsUser.State) ? string.Empty :_smsUser.State);
                            cmd.Parameters.AddWithValue("@@Country", string.IsNullOrEmpty(_smsUser.Country) ? string.Empty : _smsUser.Country);
                            cmd.Parameters.AddWithValue("@@Privileges", _smsUser.Privileges);
                            cmd.Parameters.AddWithValue("@@PhoneNumber", _smsUser.UserPhoneNumber);
                            cmd.Parameters.AddWithValue("@@CreatedDateTime", _smsUser.CreatedDateTime);
                            cmd.Parameters.AddWithValue("@@Validity", _smsUser.SMSValidity);
                            cmd.Parameters.AddWithValue("@@Limit", _smsUser.SMSLimit);
                            cmd.Parameters.AddWithValue("@@AMDFlag", 1);
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
               // throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<SMSUser> GetSMSUsers()
        {
            ObservableCollection<SMSUser> smsListUsers = new ObservableCollection<SMSUser>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSUsers_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                SMSUser _smsObj = new SMSUser();
                                _smsObj.userID = (int)reader["SMSUserID"];
                                _smsObj.UserName = reader["Name"].ToString();
                                _smsObj.Designation = reader["Designation"].ToString();
                                _smsObj.Rank = reader["Rank"].ToString();
                                _smsObj.Department = reader["Department"].ToString();
                                _smsObj.Office = reader["Office"].ToString();
                                _smsObj.Address = reader["Address"].ToString();
                                _smsObj.City = reader["City"].ToString();
                                _smsObj.State = reader["State"].ToString();
                                _smsObj.Country = reader["Country"].ToString();
                                _smsObj.Privileges = reader["Privileges"].ToString();
                                _smsObj.CreatedDateTime = (DateTime)reader["CreatedDateTime"];
                                _smsObj.UserPhoneNumber = reader["PhoneNumber"].ToString();
                                _smsObj.SMSValidity = reader["Validity"].ToString();
                                _smsObj.SMSLimit = Convert.IsDBNull(reader["Limit"]) ? null : (int?)reader["Limit"];
                                smsListUsers.Add(_smsObj);
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsListUsers;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsListUsers;
               // throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<string> GetSMSUserByName_PhoneNo()
        {
            ObservableCollection<string> smsListUsers = new ObservableCollection<string>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSUsers_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                smsListUsers.Add(reader["Name"].ToString() + "-" + reader["PhoneNumber"].ToString());
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsListUsers;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsListUsers;
                // throw new Exception(ex.Message);
            }
        }

        public ObservableCollection<string> GetSMSUsersByRank()
        {
            ObservableCollection<string> smsListUsersRank = new ObservableCollection<string>();
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand(SqlQueries.GetSMSUsersByRank_query, con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                smsListUsersRank.Add(reader["Rank"].ToString());
                            }
                        }

                        cmd.Dispose();
                    }
                }
                return smsListUsersRank;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return smsListUsersRank;
                //throw new Exception(ex.Message);
            }
        }

        public bool DeleteMultiUsers(string _userIDs)
        {
            bool result = false;
            int qyResult = 0;
            int qyResult2 = 0;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("Delete FROM SMSUserPhone Where SMSUserID IN (" + _userIDs + ")", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            qyResult = cmd.ExecuteNonQuery();
                        }

                        using (cmd = new SqlCommand("Delete FROM SMSValidity Where SMSUserID IN (" + _userIDs + ")", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.Text;
                            qyResult2 = cmd.ExecuteNonQuery();
                        }

                        if (qyResult > 0 && qyResult2 > 0)
                        {
                            using (cmd = new SqlCommand("Delete FROM SMSUser Where SMSUserID IN (" + _userIDs + ")", con))
                            {
                                cmd.CommandTimeout = 0;
                                cmd.CommandType = CommandType.Text;
                                qyResult = cmd.ExecuteNonQuery();
                            }
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return result;
               // throw new Exception(ex.Message);
            }
        }

        public bool UpdateUsersDetails(SMSUser _smsUser)
        {
            bool result = false;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spC5SMSUserManagement", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@@SMSUserID", _smsUser.userID);
                            cmd.Parameters.AddWithValue("@@Name", _smsUser.UserName);
                            cmd.Parameters.AddWithValue("@@Designation", _smsUser.Designation);
                            cmd.Parameters.AddWithValue("@@Rank", _smsUser.Rank);
                            cmd.Parameters.AddWithValue("@@Department", _smsUser.Department);
                            cmd.Parameters.AddWithValue("@@Office", _smsUser.Office);
                            cmd.Parameters.AddWithValue("@@Address", _smsUser.Address);
                            cmd.Parameters.AddWithValue("@@City", _smsUser.City);
                            cmd.Parameters.AddWithValue("@@State", _smsUser.State);
                            cmd.Parameters.AddWithValue("@@Country", _smsUser.Country);
                            cmd.Parameters.AddWithValue("@@Privileges", _smsUser.Privileges);
                            cmd.Parameters.AddWithValue("@@PhoneNumber", _smsUser.UserPhoneNumber);
                            cmd.Parameters.AddWithValue("@@CreatedDateTime", _smsUser.CreatedDateTime);
                            cmd.Parameters.AddWithValue("@@Validity", _smsUser.SMSValidity);
                            cmd.Parameters.AddWithValue("@@Limit", _smsUser.SMSValidity == "Unlimited" ? null : _smsUser.SMSLimit);
                            cmd.Parameters.AddWithValue("@@AMDFlag", 2);
                            cmd.ExecuteNonQuery();
                        }
                        result = true;
                        cmd.Dispose();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
                return false;
               // throw new Exception(ex.Message);
            }
        }

        public int CheckSMSValidity(SMSUser _smsUser)
        {
            int qyResult = 0;
            try
            {
                if (connectionStatus)
                {
                    using (con = new SqlConnection(ConnectionManager.GetConnectionString))
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        using (cmd = new SqlCommand("spCheckSMSValidity", con))
                        {
                            cmd.CommandTimeout = 0;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@SenderNumber", _smsUser.UserPhoneNumber);
                            cmd.Parameters.AddWithValue("@SMSValidity ", _smsUser.SMSValidity);
                            SqlDataReader dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                if (dr.Read())
                                {
                                    if (!Convert.IsDBNull(dr[1])) { qyResult = Convert.ToInt32(dr[1]); } else { qyResult = 0; } 
                                }
                            }
                        }
                        cmd.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ProsoftCommons.SaveExeptionToLog(ex);
               // throw new Exception(ex.Message);
            }
            return qyResult;
        }
        #endregion

    }


}
