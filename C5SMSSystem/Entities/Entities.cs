using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace C5SMSSystem
{
    //Method Fill is defined on interface.
    public interface IBusinessEntity
    {
        void Fill(SqlDataReader reader);
    }

    public class ISDData : IBusinessEntity
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }

        //And provides custom implementation of interface method.

        public void Fill(SqlDataReader reader)
        {
            CountryName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);//reader.GetString(0);
            CountryCode = reader.IsDBNull(1) ? string.Empty : reader.GetInt64(1).ToString(); //reader.GetString(1);
        }
    }

    public class STDData : IBusinessEntity
    {
        public string nPhoneNo { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string District { get; set; }
        public string Place { get; set; }
        
        //And provides custom implementation of interface method.

        public void Fill(SqlDataReader reader)
        {
            try
            {
                nPhoneNo = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                CountryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);//reader.GetString(5);
                StateName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6); //reader.GetString(6);
                District = reader.IsDBNull(7) ? string.Empty : reader.GetString(7); //reader.GetString(7);
                Place = reader.IsDBNull(8) ? string.Empty : reader.GetString(8); //reader.GetString(8);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }
    }

    public class SERIESData : IBusinessEntity
    {
        public string nPhoneNo { get; set; }
        public string ServiceProviderName { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string ContactPhoneNo1 { get; set; }

        //And provides custom implementation of interface method.

        public void Fill(SqlDataReader reader)
        {
            try
            {
                nPhoneNo = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                ServiceProviderName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);//reader.GetString(1);
                CountryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);//reader.GetString(5);
                StateName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6); //reader.GetString(6);
                ContactPersonName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11); //reader.GetString(11);
                ContactPersonDesignation = reader.IsDBNull(12) ? string.Empty : reader.GetString(12); //reader.GetString(12);
                ContactPhoneNo1 = reader.IsDBNull(13) ? string.Empty : reader.GetString(13); //reader.GetString(16);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }
    }

    public class SSIData : IBusinessEntity
    {
        public string nPhoneNo { get; set; }
        public string ServiceProviderName { get; set; }
        public string Series_STD_ISD_Code { get; set; }
        public string Series_STD_ISD_Desc { get; set; }
        public string Description { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string District { get; set; }
        public string Place { get; set; }
        public string ServiceProviderMasterID { get; set; }
        public string CountryCode { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string EmailID_CP { get; set; }
        public string ContactPhoneNo1 { get; set; }
        public string ContactPhoneNo2 { get; set; }

        //And provides custom implementation of interface method.

        public void Fill(SqlDataReader reader)
        {
            try
            {
                nPhoneNo = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                ServiceProviderName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);//reader.GetString(1);
                Series_STD_ISD_Code = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);//reader.GetString(2);
                Series_STD_ISD_Desc = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);//reader.GetString(3);
                Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);//reader.GetString(4);
                CountryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);//reader.GetString(5);
                StateName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6); //reader.GetString(6);
                District = reader.IsDBNull(7) ? string.Empty : reader.GetString(7); //reader.GetString(7);
                Place = reader.IsDBNull(8) ? string.Empty : reader.GetString(8); //reader.GetString(8);
                ServiceProviderMasterID = reader.IsDBNull(9) ? string.Empty : reader.GetInt64(9).ToString(); //reader.GetString(9);
                CountryCode = reader.IsDBNull(10) ? string.Empty : reader.GetInt64(10).ToString(); //reader.GetString(10);
                ContactPersonName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11); //reader.GetString(11);
                ContactPersonDesignation = reader.IsDBNull(12) ? string.Empty : reader.GetString(12); //reader.GetString(12);
                Address1 = reader.IsDBNull(13) ? string.Empty : reader.GetString(13); //reader.GetString(13);
                Address2 = reader.IsDBNull(14) ? string.Empty : reader.GetString(14); //reader.GetString(14);
                EmailID_CP = reader.IsDBNull(15) ? string.Empty : reader.GetString(15); //reader.GetString(15);
                ContactPhoneNo1 = reader.IsDBNull(16) ? string.Empty : reader.GetString(16); //reader.GetString(16);
                ContactPhoneNo2 = reader.IsDBNull(17) ? string.Empty : reader.GetString(17); //reader.GetString(17);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }
    }

    public class CellsiteData : IBusinessEntity
    {
        public string TowerID { get; set; }
        public string TowerName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string TowerCity { get; set; }
        public string TowerState { get; set; }
        public string TowerCountry { get; set; }
        public string Azimuth { get; set; }
        public string MSCName { get; set; }
        public string MCC { get; set; }
        public string MNC { get; set; }
        public string LAC { get; set; }
        public string RequiredZeros { get; set; }
        public string ServiceProviderNameTD { get; set; }

        //And provides custom implementation of interface method.
        public void Fill(SqlDataReader reader)
        {
            try
            {
                TowerID = GetColumnValue(reader, "TowerID"); // reader.IsDBNull(0) ? string.Empty : reader.GetString(0); //reader.GetString(0);
                TowerName = GetColumnValue(reader, "TowerName"); // reader.IsDBNull(1) ? string.Empty : reader.GetString(1); //reader.GetString(1);
                Latitude = GetColumnValue(reader, "Latitude"); // reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader.GetDouble(2)); //reader.GetString(2).ToString();
                Longitude = GetColumnValue(reader, "Longitude"); // reader.IsDBNull(3) ? string.Empty : Convert.ToString(reader.GetDouble(3)); //reader.GetString(3).ToString();
                Address1 = GetColumnValue(reader, "Address1"); // reader.IsDBNull(4) ? string.Empty : reader.GetString(4); //reader.GetString(4);
                Address2 = GetColumnValue(reader, "Address2"); // reader.IsDBNull(5) ? string.Empty : reader.GetString(5); //reader.GetString(5);
                TowerCity = GetColumnValue(reader, "TowerCity"); // reader.IsDBNull(6) ? string.Empty : reader.GetString(6); //reader.GetString(6);
                TowerState = GetColumnValue(reader, "TowerState"); // reader.IsDBNull(7) ? string.Empty : reader.GetString(7); //reader.GetString(7);
                TowerCountry = GetColumnValue(reader, "TowerCountry"); // reader.IsDBNull(8) ? string.Empty : reader.GetString(8); //reader.GetString(8);
                Azimuth = GetColumnValue(reader, "Azimuth"); // reader.IsDBNull(9) ? string.Empty : reader.GetString(9); //reader.GetString(9);
                MSCName = GetColumnValue(reader, "MSCName"); // reader.IsDBNull(10) ? string.Empty : reader.GetString(10); //reader.GetString(10);
                MCC = GetColumnValue(reader, "MCC"); // reader.IsDBNull(11) ? string.Empty : reader.GetString(11); //reader.GetString(11);
                MNC = GetColumnValue(reader, "MNC"); // reader.IsDBNull(12) ? string.Empty : reader.GetString(12); //reader.GetString(12);
                LAC = GetColumnValue(reader, "LAC"); // reader.IsDBNull(13) ? string.Empty : reader.GetString(13); //reader.GetString(13);
                RequiredZeros = GetColumnValue(reader, "RequiredZeros"); // reader.IsDBNull(14) ? string.Empty : reader.GetString(14); //reader.GetString(15);
                ServiceProviderNameTD = GetColumnValue(reader, "ServiceProviderName"); // reader.IsDBNull(15) ? string.Empty : reader.GetString(15); //reader.GetString(16);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }

        private string GetColumnValue(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == null || Convert.IsDBNull(reader[columnName]) || string.IsNullOrEmpty(reader[columnName].ToString()))
            {
                return string.Empty;
            }
            else
            {
                return reader[columnName].ToString();
            }
        }
    }

    public class SDRData : IBusinessEntity
    {
        public string SDRDetailsID { get; set; }
        public string ServiceProviderMasterID { get; set; }
        public string ServiceProviderName { get; set; }
        public string MobileNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Place { get; set; }
        public string SDRStateName { get; set; }
        public string DateOfActivation { get; set; }
        public string Category { get; set; }
        public string Product { get; set; }
        public string AlternatePhoneNo { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Qualification { get; set; }
        public string Profession { get; set; }
        public string LocalReference { get; set; }
        public string Remarks { get; set; }
        public string TypeofLine { get; set; }
        public string IMSI { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string AgentAddress { get; set; }
        public string IDProof { get; set; }
        public string IDProofDetails { get; set; }

        public void Fill(SqlDataReader reader)
        {
            try
            {
                ServiceProviderName = GetColumnValue(reader, "ServiceProviderName"); // (reader["ServiceProviderName"] == null || string.IsNullOrEmpty(reader["ServiceProviderName"].ToString()) || reader["ServiceProviderName"].ToString().Contains("Null")) ? string.Empty : reader["ServiceProviderName"].ToString(); //reader.GetString(3);
                MobileNumber = GetColumnValue(reader, "MobileNumber"); // (reader["MobileNumber"] == null || string.IsNullOrEmpty(reader["MobileNumber"].ToString()) || reader["MobileNumber"].ToString().Contains("Null")) ? string.Empty : reader["MobileNumber"].ToString(); //reader.GetString(5);
                FirstName = GetColumnValue(reader, "FirstName"); // (reader["FirstName"] == null || string.IsNullOrEmpty(reader["FirstName"].ToString()) || reader["FirstName"].ToString().Contains("Null")) ? string.Empty : reader["FirstName"].ToString(); //reader.GetString(6);
                MiddleName = GetColumnValue(reader, "MiddleName"); // (reader["MiddleName"] == null || string.IsNullOrEmpty(reader["MiddleName"].ToString()) || reader["MiddleName"].ToString().Contains("Null")) ? string.Empty : reader["MiddleName"].ToString(); //reader.GetString(7);
                LastName = GetColumnValue(reader, "LastName"); // (reader["LastName"] == null || string.IsNullOrEmpty(reader["LastName"].ToString()) || reader["LastName"].ToString().Contains("Null")) ? string.Empty : reader["LastName"].ToString(); //reader.GetString(8);

                Address1 = GetColumnValue(reader, "PresentAddress"); //reader.GetString(9);
                if (string.IsNullOrEmpty(Address1))
                {
                    Address1 = GetColumnValue(reader, "PermanentAddress");
                }

                //Address2 = (reader["ServiceProviderName"] == null || string.IsNullOrEmpty(reader["ServiceProviderName"].ToString()) || reader["ServiceProviderName"].ToString().Contains("Null")) ? string.Empty : reader["ServiceProviderName"].ToString(); //reader.GetString(10);
                //Address3 = (reader["ServiceProviderName"] == null || string.IsNullOrEmpty(reader["ServiceProviderName"].ToString()) || reader["ServiceProviderName"].ToString().Contains("Null")) ? string.Empty : reader["ServiceProviderName"].ToString(); //reader.GetString(11);
                Place = GetColumnValue(reader, "Place"); // (reader["Place"] == null || string.IsNullOrEmpty(reader["Place"].ToString()) || reader["Place"].ToString().Contains("Null")) ? string.Empty : reader["Place"].ToString(); //reader.GetString(12);
                Category = GetColumnValue(reader, "Category"); // (reader["Category"] == null || string.IsNullOrEmpty(reader["Category"].ToString()) || reader["Category"].ToString().Contains("Null")) ? string.Empty : reader["Category"].ToString(); //reader.GetString(16);

                DateOfActivation = ProsoftCommons.ConvertDateTime(reader["DateOfActivation"].ToString()).ToString("dd-MM-yyyy");                
                //DateOfActivation = Convert.IsDBNull(reader.GetDateTime(14)) ? null : reader.GetDateTime(14);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }    

        //And provides custom implementation of interface method.
        //public void Fill(SqlDataReader reader)
        //{
        //    ServiceProviderName = (reader.IsDBNull(3) || string.IsNullOrEmpty(reader.GetString(3)) || reader.GetString(3).Contains("Null")) ? string.Empty : reader.GetString(3); //reader.GetString(3);
        //    MobileNumber = (reader.IsDBNull(5) || string.IsNullOrEmpty(reader.GetString(5)) || reader.GetString(5).Contains("Null")) ? string.Empty : reader.GetString(5); //reader.GetString(5);
        //    FirstName = (reader.IsDBNull(6) || string.IsNullOrEmpty(reader.GetString(6)) || reader.GetString(6).Contains("Null")) ? string.Empty : reader.GetString(6); //reader.GetString(6);
        //    MiddleName = (reader.IsDBNull(7) || string.IsNullOrEmpty(reader.GetString(7)) || reader.GetString(7).Contains("Null")) ? string.Empty : reader.GetString(7); //reader.GetString(7);
        //    LastName = (reader.IsDBNull(8) || string.IsNullOrEmpty(reader.GetString(8)) || reader.GetString(8).Contains("Null")) ? string.Empty : reader.GetString(8); //reader.GetString(8);
        //    Address1 = (reader.IsDBNull(9) || string.IsNullOrEmpty(reader.GetString(9)) || reader.GetString(9).Contains("Null")) ? string.Empty : reader.GetString(9); //reader.GetString(9);
        //    Address2 = (reader.IsDBNull(10) || string.IsNullOrEmpty(reader.GetString(10)) || reader.GetString(10).Contains("Null")) ? string.Empty : reader.GetString(10); //reader.GetString(10);
        //    Address3 = (reader.IsDBNull(11) || string.IsNullOrEmpty(reader.GetString(11)) || reader.GetString(11).Contains("Null")) ? string.Empty : reader.GetString(11); //reader.GetString(11);
        //    Place = (reader.IsDBNull(12) || string.IsNullOrEmpty(reader.GetString(12)) || reader.GetString(12).Contains("Null")) ? string.Empty : reader.GetString(12); //reader.GetString(12);
        //    DateOfActivation = (reader.IsDBNull(14) || string.IsNullOrEmpty(reader.GetString(14)) || reader.GetString(14).Contains("Null")) ? DateTime.Parse("1/1/1900") : reader.GetDateTime(14); //reader.GetDateTime(14);
        //}

        private string GetColumnValue(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == null || Convert.IsDBNull(reader[columnName]) || string.IsNullOrEmpty(reader[columnName].ToString())) // reader[columnName].ToString().Contains("Null")
            {
                return string.Empty;
            }
            else
            {
                return reader[columnName].ToString();
            }
        }
    }

    public class SUSData : IBusinessEntity
    {
        public string nPhoneNo { get; set; }
        public string SuspectListName { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }

        public void Fill(SqlDataReader reader)
        {
            try
            {
                nPhoneNo = GetColumnValue(reader, "ColumnValue"); // reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                SuspectListName = GetColumnValue(reader, "SuspectListName"); // reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                Remarks = GetColumnValue(reader, "Remarks"); // reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                CreatedBy = GetColumnValue(reader, "UserID"); // reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }

        private string GetColumnValue(SqlDataReader reader, string columnName)
        {
            if (reader[columnName] == null || Convert.IsDBNull(reader[columnName]) || string.IsNullOrEmpty(reader[columnName].ToString()))
            {
                return string.Empty;
            }
            else
            {
                return reader[columnName].ToString();
            }
        }
    }

    public class TIDData : IBusinessEntity
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string ServiceProviderName { get; set; }
        public string TowerID { get; set; }
        public string TowerName { get; set; }
        public string Address1 { get; set; }
        public string Azimuth { get; set; }
        public string LAC { get; set; }
        public string State { get; set; }

        public void Fill(SqlDataReader reader)
        {
            try
            {
                TowerID = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                TowerName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                Address1 = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                ServiceProviderName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                Latitude = reader.IsDBNull(4) ? string.Empty : Convert.ToString(reader.GetDouble(4));
                Longitude = reader.IsDBNull(5) ? string.Empty : Convert.ToString(reader.GetDouble(5));
                Azimuth = reader.IsDBNull(6) ? string.Empty : Convert.ToString(reader.GetString(6));
                LAC = reader.IsDBNull(7) ? string.Empty : Convert.ToString(reader.GetString(7));
                State = reader.IsDBNull(8) ? string.Empty : Convert.ToString(reader.GetString(8));
            }
            catch (Exception ex)
            {
                ProsoftCommons.SaveExeptionToLog(ex);
            }
        }
    }

    public class SMSPrivileges
    {
        public string SDR { get; set; }
        public string STD { get; set; }
        public string ISD { get; set; }
        public string Series { get; set; }
        public string CellID { get; set; }
    }

    public class SMSUser
    {
        public int userID { get; set; }
        public string UserName { get; set; }
        public string UserPhoneNumber { get; set; }
        public string Designation { get; set; }
        public string Rank { get; set; }
        public string Department { get; set; }
        public string Office { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Privileges { get; set; }
        public DateTime CreatedDateTime { get; set; }

        // For SMS Validation and Limitation
        public string SMSValidity { get; set; }
        public int? SMSLimit { get; set; }
    }

    public class PartSMSs
    {
        public SentSMSStatus PartMsgStatus { get; set; }
        public string PartMsg { get; set; }
    }

    [Serializable()]
    public class Template
    {
        public int TemplateID { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Description { get { return Text.Length > 30 ? Text.Substring(0, 30).Replace(Environment.NewLine, " ") + "...." : Text.Replace(Environment.NewLine, " "); } }
    }
}
