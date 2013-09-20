using System;
using SBXA.UI.WPFControls.SBDebug;
using System.Reflection;

namespace SBXAThemeSupport.DebugAssistant
{
    /// <summary>
    /// This class provides a means to manage the debug window.
    /// </summary>
    public class DebugWindowManager
    {
        /// <summary>
        /// Shows the debug window.
        /// </summary>
        public static void ShowDebugWindow()
        {
            typeof(SBDebug).InvokeMember("ShowDebugWindow", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic, Type.DefaultBinder, null, new object[] { "Y" });
        }
        /// <summary>
        /// Shows or hides the debug window.
        /// </summary>
        /// <param name="show">True to show the window, false to hide it.</param>
        public static void ShowDebugWindow(bool show)
        {
            typeof(SBDebug).InvokeMember("ShowDebugWindow", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic, Type.DefaultBinder, null, new object[] { show });
        }
    }
}
