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
        public void AssignProperty()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "1", // first property
                "argument for Prop1", // value for property
                "ok" // confirm
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
                "c",
                "0",
                "ok"
            });
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            var args = new Args { Prop1 = "test" };
            
            Assert.IsTrue(interactionProvider.EditObject(typeof(Args), args, out args));
            
            Assert.IsTrue(((Args)interactionProvider.clipboard[0]).Prop1 == "test");
        }

        [Test]
        public void Paste()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "p",
                "0",
                "ok"
            });
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            interactionProvider.clipboard.Add(new MyDto() { Prop = "abc" });
            
            var myDto = new MyDto() { Prop = "test" };
            
            Assert.IsTrue(interactionProvider.EditObject(typeof(MyDto), myDto, out myDto));
            
            Assert.IsTrue(myDto.Prop == "abc");
        }
        
        [Test]
        public void RenderServiceOverview()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "1", // first service
                "1", // first method
                "x", // back to overview
                "x" // exit overview
            });
            
            Container c = new Container();

            c.RegisterSingleton(typeof(MyService), typeof(MyService));
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            interactionProvider.RenderServiceOverview(c);
        }
        
        [Test]
        public void InvokeMethod()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "1", // first service
                "1", // first method
                "x", // back to overview
                "x" // exit overview
            });
            
            Container c = new Container();

            c.RegisterSingleton(typeof(MyService), typeof(MyService));
            
            var interactionProvider = new ConsoleInteractionProvider(console);
            
            interactionProvider.RenderServiceOverview(c);
        }

        [Test]
        public void InvokeMethodWithParameter()
        {
            var console = new UnitTestProvider(new List<string>
            {
                "2", // second method
                "1", // first property of parameter type
                "abc", // value for property
                "ok", // confirm changes on object
                "x" // 
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