
using System;
using System.Collections.Generic;
using System.Text;

namespace Nixxis.ClientV2
{
    public enum SupervisionItemTypes
    {
        Undefined = -1,
        Agent = 0,
        Inbound = 1,
        Outbound = 2,
        Queue = 3,
        Team = 4,
        Campaign = 5
    }

    public enum SupervisionKeys
    {
        Undefined = -1,
        Parameters = 0,
        RealTime = 1,
        History = 2,
        Production = 3,
        PeriodProduction = 4,
        ContactListInfo = 5,
        SystemColumns = 6,
        UserColumns = 7,
        PeakParameters = 8,
        PeakRealTime = 9,
        PeakHistory = 10,
        PeakProduction = 11
    }
}
