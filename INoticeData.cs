using System;
using System.Collections.Generic;
using System.Text;

namespace Sharpbrake.Serilog
{
    public interface INoticeData
    {
        IDictionary<string, string> GetSessionVariables();
        IDictionary<string, string> GetEnvironmentVariables();
    }
}
