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
            
            Assert.IsTrue(interactionProvider.EditObject(typeof(Args), args, out args));
            
            Assert.AreEqual(args.Prop1, "argument for Prop1");
        }
        
        [Test]
        public void Copy()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "copy",
                "0",
                "ok"
            });
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            var args = new Args { Prop1 = "test" };
            
            Assert.IsTrue(interactionProvider.EditObject(typeof(Args), args, out args));
            
            Assert.IsTrue(((Args)interactionProvider.clipboard[0]).Prop1 == "test");
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
        
        [Test]
        public void Test3()
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

        [Test]
        public void Test4()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "2", // second method
                "1", // first property of parameter
                "abc", // value for property
                "ok", // 
                "x"
            });
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            var myService = new MyService();
            
            interactionProvider.RenderService(typeof(MyService), myService);

            Assert.IsTrue(myService.TestParamValue == "abc");
        }
    }

    public class MyDto
    {
        public string Prop { get; set; }
    }
    
    public class MyService
    {
        public string TestParamValue;
        public void PrintA()
        {
            Console.WriteLine("A");
        }
        
        public void TestParam(MyDto dto)
        {
            Console.WriteLine(dto.Prop);
            TestParamValue = dto.Prop;
        }
    }
    
    public class Args
    {
        public string Prop1 { get; set; }
    }
}