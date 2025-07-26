using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Configuration;
using System.Windows;
using System.Reflection;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using C5SMSSystem.Properties;
using System.Resources;


namespace C5SMSSystem
{
    [RunInstaller(true)]
    public partial class C5SMSInstaller : System.Configuration.Install.Installer
    {
        public C5SMSInstaller()
        {
            InitializeComponent();
        }

        private string CreateAndSaveConnectionString(string connectionStringName)
        {
            string oConnectionString = string.Empty;

            string dataSource = "Data Source =" + Context.Parameters["Server"];
            string initialcatalog = "Initial Catalog=" + Context.Parameters["Database"];
            string username = Context.Parameters["Username"];
            string dbPassword = Context.Parameters["Password"];

            if (dbPassword == null || dbPassword.Equals(""))
            {
                dbPassword = "ProUser@123";
            }

            string encryptedPassword = ProsoftCommons.EncryptTo128Bits(dbPassword);

            //MessageBox.Show(username + " AND " + dbPassword);
            dataSource = dataSource + ";" + initialcatalog;

            oConnectionString = dataSource + ";Integrated Security=False; User=" + username + "; Password=" + dbPassword + "; Connect Timeout=0;";
            dataSource = dataSource + ";Integrated Security=False; User=" + username + "; Password=" + encryptedPassword + "; Connect Timeout=0;";

            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            //Getting the path location 
            string configFile = string.Concat(Assembly.GetExecutingAssembly().Location, ".config");
            map.ExeConfigFilename = configFile;
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string connectionsection = config.ConnectionStrings.ConnectionStrings[connectionStringName].ConnectionString;

            ConnectionStringSettings connectionstring = null;
            if (connectionsection != null)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(connectionStringName);
            }

            connectionstring = new ConnectionStringSettings(connectionStringName, dataSource, "System.Data.SqlClient");
            config.ConnectionStrings.ConnectionStrings.Add(connectionstring);

            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");

            //return connectionstring.ConnectionString;
            return oConnectionString;
        }

        private string GetSql(string Name)
        {
            try
            {
                // Gets the current assembly.
                Assembly Asm = Assembly.GetExecutingAssembly();

                // Resources are named using a fully qualified name.
                Stream strm = Asm.GetManifestResourceStream(Asm.GetName().Name + "." + Name);
                // Reads the contents of the embedded file.
                StreamReader reader = new StreamReader(strm);
                return reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                MessageBox.Show("In GetSQL: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString() );
                throw ex;
            }
        }

        private void ExecuteSql(string connectionString, string Sql)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    if (con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    SqlCommand Command = new SqlCommand(Sql, con);
                    Command.ExecuteNonQuery();
                    Command.Dispose();
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Execution Error :" + ex.Message);
            }
        }

        protected void AddDBTable(string connectionString)
        {
            try
            {
                // Creates the database.
               // ExecuteSql("master", "CREATE DATABASE " + strDBName);

                // Creates the tables.
                ExecuteSql(connectionString, GetSql("Tables.txt"));

                // Creates the stored procedure.
                ExecuteSql(connectionString, GetSql("spC5SMSUserManagement.txt"));

                //ExecuteSql(connectionString, GetSql("DROP_spSearchBox_SDRs_SMS.txt"));
                //ExecuteSql(connectionString, GetSql("spSearchBox_SDRs_SMS.txt"));
                
                ExecuteSql(connectionString, GetSql("spSMSRequests.txt"));
                ExecuteSql(connectionString, GetSql("spSMSRequestsLog.txt"));
                ExecuteSql(connectionString, GetSql("spCheckSMSValidity.txt"));

            }
            catch (Exception ex)
            {
                // Reports any errors and abort.
                MessageBox.Show("In exception handler: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                throw ex;
            }
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
           AddDBTable(CreateAndSaveConnectionString("C5SMSConnectionString"));
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
           // ExecuteSql("master", "DROP DATABASE TestDB");
        }
    }
}
