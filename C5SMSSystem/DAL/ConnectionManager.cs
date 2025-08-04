using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace C5SMSSystem
{
    public class ConnectionManager
    {
        private static string _connectionString;
        private static string _connectionError;
        private static string _dsnN;
        public static string dsnName
        {
            get { return _dsnN; }
            set { _dsnN = value; }
        }

        private static string _userN;
        public static string userName
        {
            get { return _userN; }
            set { _userN = value; }
        }

        private static string _pwd;
        public static string password
        {
            get { return _pwd; }
            set { _pwd = value; }
        }

        public static bool CreateConnectionString(string _dsnName, string _userName, string _password)
        {
            try
            {
                dsnName = _dsnName;
                userName = _userName;
                password = _password;
                bool result = false;

                if (dsnName != string.Empty && userName != string.Empty && password != string.Empty)
                {
                    _connectionString = string.Empty;
                    _connectionError = string.Empty;
                    Dictionary<bool, string> conString = connectionString(dsnName, userName, password);
                    if (conString.ContainsKey(true))
                    {
                        result = true;
                        _connectionString = conString.Single(c => c.Key == true).Value;
                    }
                    else
                    {
                        result = false;
                        _connectionError = conString.Single(v => v.Key == false).Value;
                    }
                    return result;
                }
                else
                {
                    return false; //"No data available to configure connection string.";
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetConnectionString
        {
            get
            {
                string connectionString = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.ConnectionStrings["C5SMSConnectionString"].ConnectionString) ? System.Configuration.ConfigurationManager.ConnectionStrings["C5SMSConnectionString"].ConnectionString : string.Empty;//_connectionString;
                try
                {
                    if (!connectionString.Equals(string.Empty))
                    {
                        string encryptedPassword = string.Empty;

                        int passwordStartIndex = connectionString.IndexOf("Password=") + 9; // Adding 9 in passwordIndex bcoz 'Password=' contains 9 characters
                        int passwordEndIndex = connectionString.IndexOf("; Connect");

                        if (passwordStartIndex != -1 && passwordEndIndex != -1)
                        {
                            encryptedPassword = connectionString.Substring(passwordStartIndex, (passwordEndIndex - passwordStartIndex));
                        }

                        string decryptedPassword = ProsoftCommons.DecryptFrom128Bits(encryptedPassword);
                        connectionString = connectionString.Replace("Password=" + encryptedPassword, "Password=" + decryptedPassword);
                    }
                }
                catch (Exception ex)
                {
                    connectionString = string.Empty;
                    ProsoftCommons.SaveExeptionToLog(ex);
                }

                return connectionString;
            }
        }

        public static string GetConnectionError
        {
            get { return _connectionError; }
        }

        static OdbcConnection ODBCConn;
        private static Dictionary<bool, string> connectionString(string _dsnName, string _userName, string _password)
        {
            try
            {
                string _connectionString = string.Empty;
                string _createstring = @"DSN=" + _dsnName + ";Uid=" + _userName + ";Pwd=" + _password;  //"DSN=" + _dsnName + "; UID=" + _userName + "; PWD=" + _password;
                try
                {
                    ODBCConn = new OdbcConnection(_createstring);
                }
                catch (Exception e1)
                {
                    return new Dictionary<bool, string>() { { false, "This error occured when initializing connection object: " + e1.Message } };
                }
                try
                {
                    ODBCConn.Open();
                }
                catch (Exception e2)
                {
                    return new Dictionary<bool, string>() { { false, "This error occured while opening connection: " + e2.Message } };
                }
                // _connectionString = @"Data Source=" + ODBCConn.DataSource + "; Initial Catalog=" + ODBCConn.Database + "; Integrated Security=False; User=" + _userName + "; Password=" + _password + "; connection timeout=0";
                _connectionString = @"Data Source=" + ODBCConn.DataSource + ";Initial Catalog=" + ODBCConn.Database + ";User ID=" + _userName + ";Password=" + _password + ";connection timeout=1";
                ODBCConn.Close();
                ODBCConn.Dispose();
                ODBCConn = null;

                if (_connectionString != string.Empty)
                {
                    return new Dictionary<bool, string>() { { true, _connectionString } };
                }
                else
                {
                    return new Dictionary<bool, string>() { { false, "Connection cannot be created." } };
                }
            }
            catch (Exception ex)
            {
                return new Dictionary<bool, string>() { { false, "This error because input was improper: " + ex.Message } };
            }
            finally
            {
                if (ODBCConn != null)
                {
                    ODBCConn.Close();
                }
            }
        }
    }
}
