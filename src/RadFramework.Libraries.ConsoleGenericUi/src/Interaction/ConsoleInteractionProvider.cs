using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using RadFramework.Libraries.ConsoleGenericUi.Abstractions;
using RadFramework.Libraries.Ioc;
using RadFramework.Libraries.Reflection.Caching;
using RadFramework.Libraries.Reflection.Caching.Queries;
using Activator = RadFramework.Libraries.Reflection.Activation.Activator;

namespace RadFramework.Libraries.ConsoleGenericUi.Interaction
{
    public class ConsoleInteractionProvider
    {
        private readonly IConsole _console;

        public ConsoleInteractionProvider(IConsole console)
        {
            _console = console;
        }

        public void RenderServiceOverview(IContainer container)
        {
            while (true)
            {            
                _console.WriteLine("Choose a service:");
                
                int i = 1;
                
                Dictionary<int,(Type serviceType, Func<object> resolve)> choices = new Dictionary<int, (Type serviceType, Func<object> resolve)>();

                foreach ((Type serviceType, Func<object> resolve) service in container.Services)
                {
                 _console.WriteLine($"{i}) {service.serviceType.FullName}");
                 choices[i] = service;
                 i++;
                }
                
                string input = _console.ReadLine();
                
                if (input == "x")
                {
                    return;
                }
                
                int choice;

                try
                {
                    choice = int.Parse(input);
                }
                catch
                {
                    continue;
                }
                
                var selectedService = choices[choice];

                EditObject(selectedService.serviceType, container.Resolve(selectedService.serviceType));
            }
        }
        
        public bool EditObject(CachedType t, object o)
        {
            _console.WriteLine($"Edit object of type {t.InnerMetaData.FullName}:");
            
            Dictionary<int, object> choose = new Dictionary<int, object>();
            
            int i = 1;

            IEnumerable<CachedPropertyInfo> properties = t.Query(TypeQueries.GetProperties).Select(p => (CachedPropertyInfo)p);

            IEnumerable<CachedMethodInfo> methods = t.Query(TypeQueries.GetMethods).Select(m => (CachedMethodInfo)m);
            
            foreach (var propertyInfo in properties)
            {
                choose[i] = propertyInfo;
                i++;
            }
            
            foreach (var methodInfo in methods)
            {
                choose[i] = methodInfo;
                i++;
            }

            string input;

            while(true)
            {
                i = 1;

                if (properties.Any())
                {
                    _console.WriteLine("Properties:");
                    
                    foreach (var propertyInfo in properties)
                    {
                        _console.WriteLine($"{i}) {propertyInfo.InnerMetaData.Name}");
                        i++;
                    }
                }

                if (methods.Any())
                {
                    _console.WriteLine("Methods:");
                    
                    foreach (var methodInfo in methods)
                    {
                        _console.WriteLine($"{i}) {methodInfo.InnerMetaData.Name}");
                        i++;
                    }
                }

                _console.WriteLine("Choose property or method: (x to cancel, ok to confirm)");
                
                input = _console.ReadLine();
                
                if (input == "x")
                {
                    return false;
                }
                else if (input == "ok")
                {
                    return true;
                }

                int choice;

                try
                {
                    choice = int.Parse(input);
                }
                catch
                {
                    continue;
                }

                object metaData = choose[choice];

                if (metaData is CachedPropertyInfo p)
                {
                    AssignProperty(p, o);
                }
                else if (metaData is CachedMethodInfo m)
                {
                    CreateMethodInvocation(m, o);
                }
            }
        }

        private void CreateMethodInvocation(CachedMethodInfo cachedMethodInfo, object o)
        {
            var parameters = cachedMethodInfo.Query(MethodBaseQueries.GetParameters);
            
            Dictionary<ParameterInfo, object> arguments = new Dictionary<ParameterInfo, object>();

            foreach (var parameterInfo in parameters)
            {
                CachedType parameterType = parameterInfo.ParameterType;

                _console.WriteLine($"Provide Argument {parameterInfo.Name} of type {parameterType.InnerMetaData.FullName}:");
                
                string argument = _console.ReadLine();
                
                if (parameterType.InnerMetaData == typeof(string))
                {
                    arguments[parameterInfo] = argument;
                }
                else if (parameterType.InnerMetaData.IsPrimitive)
                {
                    var parseMethod = GetParseMethod(parameterType);

                    object parsedValue = null;
                
                    while (true)
                    {
                        try
                        {
                            parsedValue = parseMethod.InnerMetaData.Invoke(null, new object[] {argument});
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.WriteLine($"Provide Argument {parameterInfo.Name} of type {parameterType.InnerMetaData.FullName}:");
                        }
                    }
                    
                    arguments[parameterInfo] = parsedValue;
                }
                else if (parameterType.InnerMetaData.IsClass || parameterType.InnerMetaData.IsInterface)
                {
                    var obj = Activator.Activate(parameterType);

                    EditObject(parameterType, obj);

                    arguments[parameterInfo] = obj;
                }
            }

            var args= parameters.Select(p => arguments[p]).ToArray();

            Console.WriteLine(cachedMethodInfo.InnerMetaData.Invoke(o, args));
        }

        private void AssignProperty(CachedPropertyInfo cachedPropertyInfo, object o)
        {
            _console.WriteLine($"Assign value to {cachedPropertyInfo.InnerMetaData.Name} of type {cachedPropertyInfo.InnerMetaData.PropertyType.FullName}:");
            
            var value = _console.ReadLine();
            
            if (cachedPropertyInfo.InnerMetaData.PropertyType == typeof(string))
            {
                cachedPropertyInfo.InnerMetaData.SetValue(o, value);
            }
            else if (cachedPropertyInfo.InnerMetaData.PropertyType.IsPrimitive)
            {
                var parseMethod = GetParseMethod(cachedPropertyInfo.InnerMetaData.DeclaringType);

                object parsedValue = null;
                
                while (true)
                {
                    try
                    {
                        parsedValue = parseMethod.InnerMetaData.Invoke(null, new object[] {value});
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                
                cachedPropertyInfo.InnerMetaData.SetValue(o, parsedValue);
            }
            else if (cachedPropertyInfo.InnerMetaData.PropertyType.IsClass || cachedPropertyInfo.InnerMetaData.PropertyType.IsInterface)
            {
                EditObject(cachedPropertyInfo.InnerMetaData.PropertyType, o);
            }
        }

        private CachedMethodInfo GetParseMethod(CachedType t)
        {
            return t.InnerMetaData.GetMethod(
                "Parse",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new Type[]{typeof(string)},
                null);
        }
    }
}