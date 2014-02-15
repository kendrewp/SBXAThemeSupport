using System;
using System.Reflection;

namespace SBXAThemeSupport.Utilities
{
    public class ReflectionAssistant
    {
        public static MemberInfo GetMemberInfo(Type type, string methodName, Type[] paramTypes)
        {
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, paramTypes, null);
        }
    }
}
