using System;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using SBXA.Shared;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;
using SBXA.UI.WPFControls.SBDebug;
using System.Reflection;
using System.Windows;
using SBXAThemeSupport.Models;
using SBXAThemeSupport.Utilities;
using SBXAThemeSupport.ViewModels;
using SBXAThemeSupport.Views;

namespace SBXAThemeSupport.DebugAssistant
{
    /// <summary>
    /// This class provides a means to manage the debug window.
    /// </summary>
    public class DebugWindowManager
    {
        private readonly object _Syncobj = new object();
        private static DebugWindowManager _DebugWindowManager;
        private readonly static Hashtable _WindowTable = new Hashtable();

        internal static DebugConsoleWindow DebugConsoleWindow { get; private set; }

        public static DebugWindowManager Instance()
        {
            if (_DebugWindowManager == null) _DebugWindowManager = new DebugWindowManager();
            return (_DebugWindowManager);
        }

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

        public static void BringTopMost()
        {
            Type sbPlusWindowType = typeof(SBPlusWindow);
            var fieldInfo = sbPlusWindowType.GetField("_Ghost", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null) return;

            var removeMethodInfo = ReflectionAssistant.GetMemberInfo(typeof(SBUISupport), "RemoveGhost", new[] { typeof(UIElement), typeof(SBGhost) });
            if (removeMethodInfo == null) return;

            if (SBPlus.Current.FormStack.RealStack.Count == 0) return;
            var formInfo = SBPlus.Current.FormStack.RealStack.Peek();

            var sbWindow = formInfo.ObjectHandle.ParentSBWindow as Window;
            if (sbWindow == null) return;
            sbWindow.Topmost = true;

            sbWindow.Top = 0d;
            sbWindow.Left = 0d;

            TroubleShooterViewModel.SendFreeze("User struck 'Ctrl-Shft-G");

/*
            Type sbPlusWindowType = typeof (SBPlusWindow);
            var fieldInfo  = sbPlusWindowType.GetField("_Ghost", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null) return;

            var removeMethodInfo = ReflectionAssistant.GetMemberInfo(typeof(SBUISupport), "RemoveGhost", new Type[] {typeof(UIElement), typeof(SBGhost) });
            if (removeMethodInfo == null) return;

            // if the ghost is on the form, remove it. If it is not put it on the form - unless it is the current form.
            foreach (var isbForm in SBPlus.Current.FormStack.RealStack)
            {
                var sbWindow = isbForm.ObjectHandle.ParentSBWindow;
                var sbPlusWindow = SBUISupport.FindParentByType((DependencyObject)isbForm.ObjectHandle, typeof(SBPlusWindow)) as SBPlusWindow;
                if (sbPlusWindow != null)
                {
                    var ghost = fieldInfo.GetValue(sbPlusWindow);
                    if (ghost != null)
                    {
                        typeof(SBUISupport).InvokeMember("RemoveGhost", BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, null, new object[] { sbPlusWindow, ghost });

                        //sbPlusWindowType.InvokeMember("SetupOrRemoveGhost", BindingFlags.InvokeMethod | BindingFlags.NonPublic, Type.DefaultBinder, sbPlusWindow, new object[] { (ISBForm)isbForm.ObjectHandle });

                        // SetupOrRemoveGhost(ISBForm form)
                    }
                    // var ghost = propInfo.GetValue(sbPlusWindow, BindingFlags.NonPublic, null, new object[0]);
                    Debug.WriteLine("[DebugWindowManager.SwitchGhost(26)] " + SBControl.GetIsGhostPresent(sbPlusWindow));
                }
            }
*/
        }

        private static void OpenDebugConsole()
        {
            var thread = new Thread(CreateDebugConsole);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }

        public static void CloseDebugConsole()
        {
            if (DebugConsoleWindow == null) return;
            if (_WindowTable.Count != 0) _WindowTable.Clear();

            DebugConsoleWindow.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            DebugConsoleWindow = null;
        }
        
        private static void CreateDebugConsole()
        {
            lock (Instance()._Syncobj)
            {
                try
                {
                    if (((SBPlus.Current != null) && (DebugConsoleWindow == null)) && !SBPlus.Current.IsSBPlusClosing)
                    {
                        try
                        {
                            DebugConsoleWindow = new DebugConsoleWindow();
                            DebugConsoleWindow.Closed += DebugConsoleIsClosed;
                            DebugConsoleWindow.Show();
                            Dispatcher.Run();
                        }
                        catch (Exception exception)
                        {
                            SBPlusClient.LogError("An exception was caught when creating the Debug Console.", exception);
                            DebugConsoleWindow = null;
                        }
                    }
                }
                catch (Exception exception2)
                {
                    SBPlusClient.LogError("An exception was caught when creating the Debug Console.", exception2);
                }
            }

        }

        private static void DebugConsoleIsClosed(object sender, EventArgs e)
        {
            if ((DebugConsoleWindow != null) && (DebugConsoleWindow.Dispatcher != null))
            {
                DebugConsoleWindow.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }
            DebugConsoleWindow = null;
        }

        public static void FlipDebugConsole()
        {
            if (DebugConsoleWindow == null)
            {
                OpenDebugConsole();
            }
            else
            {
                CloseDebugConsole();
            }
        }
        /// <summary>
        /// Adds a window to the collection of open windows.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="window"></param>
        public static void AddWindow(string key, Window window)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (window == null) return;
            try
            {
                if (!_WindowTable.ContainsKey(key))
                {
                    _WindowTable.Add(key, window);
                }
            }
            catch (Exception)
            {
                SBPlusClient.LogError("Exception caught removing window " + key);
                throw;
            }
        }
        /// <summary>
        /// Returns the window with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Window GetWindow(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            try
            {
                if (_WindowTable.ContainsKey(key))
                {
                    return _WindowTable[key] as Window;
                }
            }
            catch (Exception)
            {
                SBPlusClient.LogError("Exception caught removing window " + key);
                throw;
            }
            return null;
        }

        /// <summary>
        /// Removes a window with the supplied key from the colleciton of open windows.
        /// </summary>
        /// <param name="key"></param>
        public static void RemoveWindow(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            try
            {
                
                if (_WindowTable.ContainsKey(key))
                {
                    _WindowTable.Remove(key);
                }
            }
            catch (Exception)
            {
                SBPlusClient.LogError("Exception caught removing window "+key);
                throw;
            }
        }

        internal static void ShowSBString(string which, SBString data)
        {
            if (data == null) throw new ArgumentNullException("data");
            var sbStringViewWindow = GetWindow(which);
            
            if (sbStringViewWindow == null)
            {
                sbStringViewWindow = new SBStringViewerWindow
                    {
                        DataContext = NestedAttributeCollection.BuildFromSBString(which, data),
                        Owner = DebugConsoleWindow
                    };
                    sbStringViewWindow.Show();
                    AddWindow(which, sbStringViewWindow);
            }
            else
            {
                sbStringViewWindow.Activate();
            }

        }

    }
}
