using System;
using System.Collections.Generic;

namespace RadFramework.Libraries.ConsoleGenericUi.Abstractions
{
    public class UnitTestProvider : IConsole
    {
        private readonly Queue<string> _readLineInstructions;

        public UnitTestProvider(List<string> readLineInstructions)
        {
            _readLineInstructions = new Queue<string>(readLineInstructions);
        }

        public string ReadLine()
        {
            string input = _readLineInstructions.Dequeue();
            Console.WriteLine(input);
            return input;
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }
}