using System;

namespace Apocryph.FunctionApp.Command
{
    public class ReminderCommand : ICommand
    {
        public TimeSpan Time { get; set; }
        public object Data { get; set; }
    }
}