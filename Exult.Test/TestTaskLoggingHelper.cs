using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace Exult.Test
{
    class TestTaskLoggingHelper : TaskLoggingHelper
    {
        public TestTaskLoggingHelper() : base(new TestTask())
        {

        }
    }
}
