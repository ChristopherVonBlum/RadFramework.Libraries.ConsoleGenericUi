using System;
using System.Collections.Generic;
using RadFramework.Libraries.ConsoleGenericUi.Abstractions;

namespace RadFramework.Libraries.ConsoleGenericUi.Interaction
{
    public class ConsoleGenericUiCommandLineDriver : IConsole
    {
        private readonly IConsole _console;

        private readonly Dictionary<string, object> userHeap;
        
        private List<string> writeLineBuffer = new List<string>();

        public ConsoleGenericUiCommandLineDriver(IConsole console)
        {
            _console = console;
        }
        
        public void RenderControls()
        {
            
        }

        public void RenderHeap()
        {
            int i = 1;
            
            foreach (KeyValuePair<string, object> variable in userHeap)
            {
                _console.WriteLine($"{i}) {variable.Key} of type {variable.Value.GetType().FullName}");
                i++;
            }
            
            
        }

        public string ReadLine()
        {
            var line = _console.ReadLine();

            string resolved = string.Empty;
            
            if (line.Contains("${"))
            {
                while (true)
                {
                    int makroStart = line.IndexOf("${") + 2;

                    int makroEnd = line.IndexOf("}");

                    var variableName = line.Substring(makroStart, makroEnd - makroStart);
                    
                    line = line.Replace($"${{{variableName}}}", userHeap[variableName].ToString());
                }
            }

            return resolved;
        }

        public void WriteLine(string value)
        {
            writeLineBuffer.Add(value);
        }
    }
}