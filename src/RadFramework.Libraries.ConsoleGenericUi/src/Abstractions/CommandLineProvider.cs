using System;

namespace RadFramework.Libraries.ConsoleGenericUi.Abstractions
{
    public class CommandLineProvider : IConsole
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }
}