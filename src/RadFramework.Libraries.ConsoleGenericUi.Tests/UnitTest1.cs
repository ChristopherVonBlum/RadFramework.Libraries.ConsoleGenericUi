using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using RadFramework.Libraries.ConsoleGenericUi.Abstractions;
using RadFramework.Libraries.ConsoleGenericUi.Interaction;
using RadFramework.Libraries.Ioc;

namespace RadFramework.Libraries.ConsoleGenericUi.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "1",
                "argument for Prop1",
                "ok"
            });
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            var args = new Args();
            
            Assert.IsTrue(interactionProvider.EditObject(typeof(Args), args));
            
            Assert.AreEqual(args.Prop1, "argument for Prop1");
        }
        
        [Test]
        public void Test2()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "1", // first service
                "1", // first method
                "x", // back to overview
                "x" 
            });
            
            Container c = new Container();

            c.RegisterSingleton(typeof(MyService), typeof(MyService));
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            interactionProvider.RenderServiceOverview(c);
        }
    }

    public class MyService
    {
        public MyService()
        {
            
        }
        public void PrintA()
        {
            Console.WriteLine("A");
        }
    }
    
    public class Args
    {
        public string Prop1 { get; set; }
    }
}