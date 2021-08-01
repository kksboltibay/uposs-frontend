using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UPOSS.Custom
{
    // For handling async Task(void) specifically
    public static class TaskExtensions
    {
        public async static void Await(this Task task)
        {
            await task;
        }
    }
}
