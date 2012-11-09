using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;

namespace Exult.Tasks
{
    public class TaskItemTransform
    {
        public ITaskItem Input { get; private set; }
        public ITaskItem Output { get; private set; }
        public TaskItemTransform(ITaskItem input, ITaskItem output)
        {
            Input = input;
            Output = output;
        }   
    }
}
