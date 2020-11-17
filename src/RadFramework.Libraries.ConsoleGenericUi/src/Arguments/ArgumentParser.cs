using System.Collections.Generic;
using RadFramework.Libraries.Reflection.Caching;
using RadFramework.Libraries.Reflection.Caching.Queries;

namespace RadFramework.Libraries.ConsoleGenericUi.Arguments
{
    public static class ArgumentParser
    {
        public static T ParseArgs<T>(string[] args) where T : new()
        {
            Dictionary<string, string> namedArgs = new Dictionary<string, string>();
            
            for (int i = 0; i < args.Length; i++)
            {
                string str = args[i];
                if (str.StartsWith("-"))
                {
                    namedArgs[str.Substring(1).ToLower()] = args[++i];
                }
            }

            CachedType t = typeof(T);
            
            T instance = new T();
            
            foreach (var propertyInfo in t.Query(ClassQueries.GetPublicImplementedProperties))
            {
                string argKey = propertyInfo.Name.ToLower();
                
                propertyInfo.SetValue(instance, namedArgs[argKey]);
            }

            return instance;
        }
    }
}