using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Exult.Test
{
    class TestBuildEngine : IBuildEngine
    {
        public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public int ColumnNumberOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }

        public bool ContinueOnError
        {
            get { throw new NotImplementedException(); }
        }

        public int LineNumberOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
        }

        public void LogErrorEvent(BuildErrorEventArgs e)
        {
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {

        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
        }

        public string ProjectFileOfTaskNode
        {
            get { throw new NotImplementedException(); }
        }
    }
}
