using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace subtitles
{
    internal enum SyncType
    {
        None,
        Valid,
        InValid
    }

    internal enum ShiftType
    {
        None,
        OnlyMilliseconds,
        LineAndTimeStamp,
        InValid
    }
}
