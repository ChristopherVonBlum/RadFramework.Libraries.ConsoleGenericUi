using System;
using RadFramework.Libraries.ConsoleGenericUi.Abstractions;
using RadFramework.Libraries.ConsoleGenericUi.Interaction;
using RadFramework.Libraries.Ioc;

namespace POC
{
    class Program
    {
        static void Main(string[] args)
        {
            Container c = new Container();

            c.RegisterSingleton<MyService, MyService>();

            var provider = new ConsoleInteractionProvider(new CommandLineProvider());
            
            provider.RenderServiceOverview(c);
            
            Console.ReadLine();
        }
    }

    public class MyService
    {
        public int TestProp { get; set; }
        public DateTime TestDate { get; set; }
        public float TestFloat { get; set; }

        public int TestResult(int a, int b)
        {
            return a + b * a;
        }
    }
}