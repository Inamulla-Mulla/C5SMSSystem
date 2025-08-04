using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C5SMSSystem
{
    public enum SMSStatus
    {
        Success = 0,
        Failed = 1,
        Pending = 2,
        Cancelled = 3
    }

    public enum SentSMSStatus
    {
        Passed = 0,
        Failed = 1,
        NotFound = 2,
        NotFoundAndFailed = 3,
        Maintenance = 4,
        MaintenanceFailed = 5,
        Test = 6,
        TestFailed = 7
    }

    public enum SMSRequestType
    {
        SDR = 0,
        SDRi = 1,
        ISD = 2,
        ISDi = 3,
        STD = 4,
        STDi = 5,
        SERIES = 6,
        SERIESi = 7,
        CELLID = 8,
        TID = 9,
        SUS = 10
    }

    public enum SMSAccessType
    {
        Authorized = 0,
        Unauthorized = 1
    }

    public enum GroupSelected
    {
        Users = 0,
        Ranks = 1
    }
}
