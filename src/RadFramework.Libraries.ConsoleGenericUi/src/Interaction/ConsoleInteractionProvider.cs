using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using RadFramework.Libraries.ConsoleGenericUi.Abstractions;
using RadFramework.Libraries.Ioc;
using RadFramework.Libraries.Reflection.Caching;
using RadFramework.Libraries.Reflection.Caching.Queries;
using RadFramework.Libraries.TextTranslation;
using RadFramework.Libraries.TextTranslation.Abstractions;
using RadFramework.Libraries.TextTranslation.Loaders;
using Activator = RadFramework.Libraries.Reflection.Activation.Activator;

namespace RadFramework.Libraries.ConsoleGenericUi.Interaction
{
    public class ConsoleInteractionProvider
    {
        private readonly IConsole _console;
        private readonly ITranslationProvider _translationProvider;

        public readonly List<object> clipboard = new List<object>();

        public ConsoleInteractionProvider(IConsole console, ITranslationProvider translationProvider)
        {
            _console = console;
            _translationProvider = translationProvider;
        }

        public ConsoleInteractionProvider(IConsole console)
            : this(
                console, 
                new TranslationProvider(
                    new TranslationDictionaryEmbeddedResourceLoader(
                        Assembly.GetExecutingAssembly(), 
                        "RadFramework.Libraries.ConsoleGenericUi.ConsoleInteractionProviderTranslations.json")))
        {
        }
        
        public void RenderServiceOverview(IContainer container)
        {
            while (true)
            {
                _console.WriteLine(_translationProvider.Translate("ChooseService"));
                
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
                
                if (choice >= choices.Count + 1)
                {
                    _console.WriteLine($"{choice} {_translationProvider.Translate("IsOutOfRange")}");
                    continue;
                }
                
                var selectedService = choices[choice];

                RenderService(selectedService.serviceType, container.Resolve(selectedService.serviceType));
            }
        }

        public void RenderService(CachedType tService, object serviceObject)
        {
            Dictionary<int, CachedMethodInfo> choose = new Dictionary<int, CachedMethodInfo>();
            
            int i = 1;
            
            IEnumerable<CachedMethodInfo> methods = tService.Query(TypeQueries.GetMethods).Select(m => (CachedMethodInfo)m);
            
            foreach (var methodInfo in methods)
            {
                choose[i] = methodInfo;
                i++;
            }
            
            while(true)
            {
                _console.WriteLine($"{_translationProvider.Translate("ServiceOfType")} {tService.InnerMetaData.FullName}:");
                
                i = 1;
                
                if (methods.Any())
                {
                    _console.WriteLine($"{_translationProvider.Translate("Methods")}:");
                    
                    foreach (var methodInfo in methods)
                    {
                        _console.WriteLine($"{i}) {methodInfo.InnerMetaData.Name}");
                        i++;
                    }
                }

                string input;
                
                _console.WriteLine($"{_translationProvider.Translate("ChoosePropertyOrMethod")}");
                
                input = _console.ReadLine();
                
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

                if (choice > choose.Count)
                {
                    _console.WriteLine($"{choice} {_translationProvider.Translate("IsOutOfRange")}");
                    continue;
                }

                CachedMethodInfo metaData = choose[choice];
                
                CreateMethodInvocation(metaData, serviceObject);
            }
        }
        
        public bool EditObject<T>(CachedType t, T obj, out T modified)
        {
            T cloned = (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(obj, t, Formatting.None,
                new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }), t);
            
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
                _console.WriteLine($"{_translationProvider.Translate("EditObjectOfType")} {t.InnerMetaData.FullName}:");
                
                i = 1;

                if (properties.Any())
                {
                    _console.WriteLine($"{_translationProvider.Translate("Properties")}:");
                    
                    foreach (var propertyInfo in properties)
                    {
                        _console.WriteLine($"{i}) {propertyInfo.InnerMetaData.Name}");
                        i++;
                    }
                }

                if (methods.Any())
                {
                    _console.WriteLine($"{_translationProvider.Translate("Methods")}:");
                    
                    foreach (var methodInfo in methods)
                    {
                        _console.WriteLine($"{i}) {methodInfo.InnerMetaData.Name}");
                        i++;
                    }
                }

                _console.WriteLine($"{_translationProvider.Translate("EditObjectMenu")}");
                
                input = _console.ReadLine();
                
                if (input == "x")
                {
                    modified = obj;
                    return false;
                }
                else if (input == "ok")
                {
                    modified = cloned;
                    return true;
                }
                else if (input == "e")
                {
                    _console.WriteLine(JsonConvert.SerializeObject(cloned, Formatting.Indented));
                    continue;
                }
                else if (input == "c")
                {
                    StoreObjectInClipboard(obj);
                    continue;
                }
                else if (input == "p")
                {
                    cloned = modified = (T)PasteObjectFromClipboard();
                    continue;
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

                if (choice > choose.Count)
                {
                    _console.WriteLine($"{choice} {_translationProvider.Translate("IsOutOfRange")}");
                    continue;
                }

                object metaData = choose[choice];

                if (metaData is CachedPropertyInfo p)
                {
                    AssignProperty(p, cloned);
                }
                else if (metaData is CachedMethodInfo m)
                {
                    CreateMethodInvocation(m, cloned);
                }
            }
        }

        private object PasteObjectFromClipboard()
        {
            int i = 1;
            
            foreach (object obj in clipboard)
            {
                _console.WriteLine($"{i}) {obj.GetType().FullName}");
                i++;
            }

            while (true)
            {
                _console.WriteLine(_translationProvider.Translate("IndexToPasteFrom"));
                
                string cmd = _console.ReadLine();
            
                int index;
            
                try
                {
                    index = int.Parse(cmd);
                }
                catch
                {
                    continue;
                }

                if (index >= clipboard.Count)
                {
                    continue;
                }
            
                return clipboard[index];
            }
        }

        private void StoreObjectInClipboard(object value)
        {
            int i = 1;
            
            foreach (object obj in clipboard)
            {
                _console.WriteLine($"{i}) {obj.GetType().FullName}");
                i++;
            }
            
            while (true)
            {
                _console.WriteLine(_translationProvider.Translate("IndexToStoreObject"));
                
                string cmd = _console.ReadLine();

                if (cmd == "x")
                {
                    return;
                }

                int index;
                
                try
                { 
                    index = int.Parse(cmd);
                }
                catch
                {
                    continue;
                }

                if (index >= clipboard.Count)
                {
                    clipboard.Add(value);
                    return;
                }
                
                clipboard[index] = value;
            }
        }
        
        private void CreateMethodInvocation(CachedMethodInfo cachedMethodInfo, object o)
        {
            var parameters = cachedMethodInfo.Query(MethodBaseQueries.GetParameters);
            
            Dictionary<ParameterInfo, object> arguments = new Dictionary<ParameterInfo, object>();

            foreach (var parameterInfo in parameters)
            {
                CachedType parameterType = parameterInfo.ParameterType;

                if (parameterType.InnerMetaData == typeof(string))
                {
                    _console.WriteLine(string.Format(_translationProvider.Translate("ProvideArgumentOfType"), parameterInfo.Name, parameterType.InnerMetaData.FullName));
                    
                    string argument = _console.ReadLine();
                    
                    arguments[parameterInfo] = argument;
                }
                else if (parameterType.InnerMetaData.IsPrimitive)
                {
                    _console.WriteLine(string.Format(_translationProvider.Translate("ProvideArgumentOfType"), parameterInfo.Name, parameterType.InnerMetaData.FullName));
                    
                    var parseMethod = GetParseMethod(parameterType);

                    object parsedValue = null;
                
                    string argument = _console.ReadLine();
                    
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
                            _console.WriteLine(string.Format(_translationProvider.Translate("ProvideArgumentOfType"), parameterInfo.Name, parameterType.InnerMetaData.FullName));
                        }
                    }
                    
                    arguments[parameterInfo] = parsedValue;
                }
                else if (parameterType.InnerMetaData.IsClass || parameterType.InnerMetaData.IsInterface)
                {
                    var obj = Activator.Activate(parameterType);

                    EditObject(parameterType, obj, out obj);

                    arguments[parameterInfo] = obj;
                }
            }

            var args= parameters.Select(p => arguments[p]).ToArray();

            object result = cachedMethodInfo.InnerMetaData.Invoke(o, args);
            
            if (cachedMethodInfo.InnerMetaData.ReturnType == typeof(void))
            {
                return;
            }
            else if (cachedMethodInfo.InnerMetaData.ReturnType.IsPrimitive || cachedMethodInfo.InnerMetaData.ReturnType == typeof(string))
            {
                _console.WriteLine(_translationProvider.Translate("ReturnValue"));
                _console.WriteLine(result.ToString());
                return;
            }

            EditObject(cachedMethodInfo.InnerMetaData.ReturnType, result, out object v);
        }

        private void AssignProperty(CachedPropertyInfo cachedPropertyInfo, object o)
        {
            _console.WriteLine(string.Format(_translationProvider.Translate("AssignValueOfType"), cachedPropertyInfo.InnerMetaData.Name, cachedPropertyInfo.InnerMetaData.PropertyType.FullName));
            
            var value = _console.ReadLine();
            
            if (cachedPropertyInfo.InnerMetaData.PropertyType == typeof(string))
            {
                cachedPropertyInfo.InnerMetaData.SetValue(o, value);
            }
            else if (cachedPropertyInfo.InnerMetaData.PropertyType.IsPrimitive)
            {
                var parseMethod = GetParseMethod(cachedPropertyInfo.InnerMetaData.PropertyType);

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
                EditObject(cachedPropertyInfo.InnerMetaData.PropertyType, o, out o);
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