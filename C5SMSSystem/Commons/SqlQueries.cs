using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5SMSSystem
{
    public static class SqlQueries
    {
        public static readonly string SDR_sp = @"SPSearchBox_SDRs_SMS";
        public static readonly string CellID_sp = @"C5CDR_V5..SPSearchBox_CellIDs";
        public static readonly string SSI_sp = @"C5CDR_V5..SPSMS_SP"; // public static readonly string SSI_sp = @"C5CDR_V5..SPSearchBox_SSI_CP";
        public static readonly string ISD_sp = @"C5CDR_V5..SPSMS_ISD";
        public static readonly string SUS_sp = @"C5CDR_V5..SPSMS_SUS";
        public static readonly string TID_sp = @"C5CDR_V5..SPSMS_TID"; //SPSMS_TID
        public static readonly string GetSMSUsers_query = @"SELECT su.*,sup.PhoneNumber, suv.Validity, suv.Limit FROM dbo.SMSUser su INNER JOIN dbo.SMSUserPhone sup ON sup.SMSUserID = su.SMSUserID LEFT JOIN dbo.SMSValidity suv ON suv.SMSUserID = su.SMSUserID";
        public static readonly string GetSMSUsersByRank_query = @"SELECT DISTINCT Rank FROM dbo.SMSUser Where Rank <> '' AND Rank <> NULL";
        public static readonly string ValidateSMSUser_query = @"Select Top 1 su.SMSUserID from dbo.SMSUser su INNER JOIN dbo.SMSUserPhone sup ON sup.SMSUserID = su.SMSUserID Where sup.PhoneNumber = @PhoneNumber";
        public static readonly string GetSMSs_query = @"set concat_null_yields_null OFF;
                                                            Select sr.MessageID, sr.SenderNumber + ' - ' + su.Name as 'Sender', sr.SenderNumber, sr.Message, sr.ReceivedDateTime, sr.Status ,sr.RequestType, sr.AccessType from SMSRequests sr
                                                            Left Outer join SMSUserPhone sup on sup.PhoneNumber = sr.SenderNumber
                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID";
        public static readonly string GetPendingSMSs_query = @"set concat_null_yields_null OFF;
                                                            Select sr.MessageID, sr.SenderNumber + ' - ' + su.Name as 'Sender', sr.SenderNumber, sr.Message, sr.ReceivedDateTime, sr.Status ,sr.RequestType, sr.AccessType from SMSRequests sr
                                                            Left Outer join SMSUserPhone sup on sup.PhoneNumber = sr.SenderNumber
                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID where sr.Status = 'Pending'";
        public static readonly string GetSMSsByDate_query = @"set concat_null_yields_null OFF;
                                                            Select sr.MessageID, sr.SenderNumber + ' - ' + su.Name as 'Sender', sr.SenderNumber, sr.Message, sr.ReceivedDateTime, sr.Status ,sr.RequestType, sr.AccessType from SMSRequests sr
                                                            Left Outer join SMSUserPhone sup on sup.PhoneNumber = sr.SenderNumber
                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                            Where Cast(sr.ReceivedDateTime as DATE) = @TodaysDate";
        
        public static readonly string GetSMSsBetweenDates_query = @"set concat_null_yields_null OFF;
                                                            Select sr.MessageID, sr.SenderNumber + ' - ' + su.Name as 'Sender', sr.SenderNumber, sr.Message, sr.ReceivedDateTime, sr.Status ,sr.RequestType, sr.AccessType from SMSRequests sr
                                                            Left Outer join SMSUserPhone sup on sup.PhoneNumber = sr.SenderNumber
                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                            Where Cast(sr.ReceivedDateTime as DATE) >= @FromDate AND Cast(sr.ReceivedDateTime as DATE) <= @ToDate";

        public static readonly string GetSMS_query = @"set concat_null_yields_null OFF;
                                                            Select sr.MessageID, sr.SenderNumber + ' - ' + su.Name as 'Sender', sr.SenderNumber, sr.Message, sr.ReceivedDateTime, sr.Status ,sr.RequestType, sr.AccessType from SMSRequests sr
                                                            Left Outer join SMSUserPhone sup on sup.PhoneNumber = sr.SenderNumber
                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                            Where sr.MessageID = IDENT_CURRENT('SMSRequests')";

        public static readonly string GetSMSLogs_query = @"set concat_null_yields_null OFF;
                                                                Select 
                                                                CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                'ADMIN' 
                                                                ELSE
	                                                                sr.SenderNumber + ' - ' + su.Name  
                                                                END as 'Sender'
                                                                ,
                                                                CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                sl.RequestNumber + ' - ' + su.Name  
                                                                ELSE
	                                                                sl.RequestNumber
                                                                END as 'RequestNumber'
                                                                ,SMSRequestsLogID, sl.MessageID, SentMessage, SentStatus, SentDateTime

                                                                from SMSRequestsLog sl
                                                                Left outer join SMSRequests sr on sr.MessageID = sl.MessageID
                                                                Left Outer join SMSUserPhone sup on RIGHT(sup.PhoneNumber,10) = RIGHT(sr.SenderNumber,10)OR RIGHT(sup.PhoneNumber,10) = RIGHT(sl.RequestNumber,10)
                                                                Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                                WHERE SentStatus <>'TestFailed' AND SentStatus <>'MaintenanceFailed'";

        public static readonly string GetSMSLogsByDate_query = @"set concat_null_yields_null OFF;
                                                                    Select 
                                                                    CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                    'ADMIN' 
                                                                    ELSE
	                                                                    sr.SenderNumber + ' - ' + su.Name  
                                                                    END as 'Sender'
                                                                    ,
                                                                    CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                    sl.RequestNumber + ' - ' + su.Name  
                                                                    ELSE
	                                                                    sl.RequestNumber
                                                                    END as 'RequestNumber'
                                                                    ,SMSRequestsLogID, sl.MessageID, SentMessage, SentStatus, SentDateTime

                                                                    from SMSRequestsLog sl
                                                                    Left outer join SMSRequests sr on sr.MessageID = sl.MessageID
                                                                    Left Outer join SMSUserPhone sup on RIGHT(sup.PhoneNumber,10) = RIGHT(sr.SenderNumber,10)OR RIGHT(sup.PhoneNumber,10) = RIGHT(sl.RequestNumber,10)
                                                                    Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                                    Where Cast(SentDateTime as DATE) = @TodaysDate AND (SentStatus <>'TestFailed' AND SentStatus <>'MaintenanceFailed')";

        public static readonly string GetSMSLogsBetweenDates_query = @"set concat_null_yields_null OFF;
                                                                            Select 
                                                                            CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                            'ADMIN' 
                                                                            ELSE
	                                                                            sr.SenderNumber + ' - ' + su.Name  
                                                                            END as 'Sender'
                                                                            ,
                                                                            CASE WHEN SentStatus = 'Test' or SentStatus = 'Maintenance' THEN
	                                                                            sl.RequestNumber + ' - ' + su.Name  
                                                                            ELSE
	                                                                            sl.RequestNumber
                                                                            END as 'RequestNumber'
                                                                            ,SMSRequestsLogID, sl.MessageID, SentMessage, SentStatus, SentDateTime

                                                                            from SMSRequestsLog sl
                                                                            Left outer join SMSRequests sr on sr.MessageID = sl.MessageID
                                                                            Left Outer join SMSUserPhone sup on RIGHT(sup.PhoneNumber,10) = RIGHT(sr.SenderNumber,10)OR RIGHT(sup.PhoneNumber,10) = RIGHT(sl.RequestNumber,10)
                                                                            Left outer join SMSUser su on su.SMSUserID = sup.SMSUserID
                                                                            Where Cast(SentDateTime as DATE) >= @FromDate AND Cast(SentDateTime as DATE) <= @ToDate AND (SentStatus <>'TestFailed' AND SentStatus <>'MaintenanceFailed')";
    }
}
