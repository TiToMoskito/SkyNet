using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkyNet
{
    public static class Extensions
    {
        public static IEnumerable<Type> FindInterfaceImplementations(this Type t)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass && t.IsAssignableFrom(type))
                        yield return type;
                }
            }
        }
    }
}
