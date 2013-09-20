using System;
using SBXA.UI.WPFControls.SBDebug;
using System.Reflection;

namespace SBXAThemeSupport.DebugAssistant
{
    public class DebugWindowManager
    {

        public static void ShowDebugWindow()
        {
            typeof(SBDebug).InvokeMember("ShowDebugWindow", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic, Type.DefaultBinder, null, new object[] { "Y" });
        }

        public static void ShowDebugWindow(bool show)
        {
            typeof(SBDebug).InvokeMember("ShowDebugWindow", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic, Type.DefaultBinder, null, new object[] { show });
        }
    }
}
