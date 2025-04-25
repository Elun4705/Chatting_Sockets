using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileLogger
{

    public class CustomFileLogProvider : ILoggerProvider
    {
        CustomFileLogger logger;
        public CustomFileLogProvider()
        {
            logger = new CustomFileLogger("Chat Log");
        }
        public ILogger CreateLogger(string categoryName)
        {
            return logger;
        }

        public void Dispose()
        {
            //Already handled for us
        }
    }
}
