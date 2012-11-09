using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Exult.Test
{
    public class TestTask : ITask
    {
        public TestTask()
        {
            BuildEngine = new TestBuildEngine();
        }

        public IBuildEngine BuildEngine
        {
            get;
            set;
        }

        public bool Execute()
        {
            throw new NotImplementedException();
        }

        public ITaskHost HostObject
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
