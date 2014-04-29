﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugWindowManager.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.DebugAssistant
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;
    using SBXA.UI.WPFControls.SBDebug;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;
    using SBXAThemeSupport.Utilities;
    using SBXAThemeSupport.ViewModels;
    using SBXAThemeSupport.Views;

    /// <summary>
    ///     This class provides a means to manage the debug window.
    /// </summary>
    public class DebugWindowManager
    {
        #region Static Fields

        private static readonly Hashtable WindowTable = new Hashtable();

        private static DebugWindowManager debugWindowManager;

        #endregion

        #region Fields

        private readonly object syncobj = new object();

        #endregion

        #region Properties

        internal static DebugConsoleWindow DebugConsoleWindow { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds a window to the collection of open windows.
        /// </summary>
        /// <param name="key">
        ///     The key which is used to store the window against.
        /// </param>
        /// <param name="window">
        ///     The window to add to the collection of windows.
        /// </param>
        public static void AddWindow(string key, Window window)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (window == null)
            {
                return;
            }

            try
            {
                if (!WindowTable.ContainsKey(key))
                {
                    WindowTable.Add(key, window);
                }
            }
            catch (Exception)
            {
                SBPlusClient.LogError("Exception caught removing window " + key);
                throw;
            }
        }

        /// <summary>
        ///     The bring top most.
        /// </summary>
        public static void BringTopMost()
        {
            Type sbPlusWindowType = typeof(SBPlusWindow);
            var fieldInfo = sbPlusWindowType.GetField("_Ghost", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo == null)
            {
                return;
            }

            var removeMethodInfo = ReflectionAssistant.GetMemberInfo(
                typeof(SBUISupport),
                "RemoveGhost",
                new[] { typeof(UIElement), typeof(SBGhost) });
            if (removeMethodInfo == null)
            {
                return;
            }

            if (SBPlus.Current.FormStack.RealStack.Count == 0)
            {
                return;
            }

            var formInfo = SBPlus.Current.FormStack.RealStack.Peek();

            var sbWindow = formInfo.ObjectHandle.ParentSBWindow as Window;
            if (sbWindow == null)
            {
                return;
            }

            sbWindow.Topmost = true;

            sbWindow.Top = 0d;
            sbWindow.Left = 0d;

            TroubleShooterViewModel.SendFreeze("User struck 'Ctrl-Shft-G", true);
        }

        /// <summary>
        ///     The close debug console.
        /// </summary>
        public static void CloseDebugConsole()
        {
            if (DebugConsoleWindow == null)
            {
                return;
            }

            if (WindowTable.Count != 0)
            {
                WindowTable.Clear();
            }

            DebugConsoleWindow.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            DebugConsoleWindow = null;
        }

        /// <summary>
        ///     The flip debug console.
        /// </summary>
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
        ///     Returns the window with the specified key.
        /// </summary>
        /// <param name="key">
        ///     The key which is used to get the window.
        /// </param>
        /// <returns>
        ///     The <see cref="Window" />.
        /// </returns>
        public static Window GetWindow(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            try
            {
                if (WindowTable.ContainsKey(key))
                {
                    return WindowTable[key] as Window;
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
        ///     The instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="DebugWindowManager" />.
        /// </returns>
        public static DebugWindowManager Instance()
        {
            return debugWindowManager ?? (debugWindowManager = new DebugWindowManager());
        }

        /// <summary>
        ///     Removes a window with the supplied key from the colleciton of open windows.
        /// </summary>
        /// <param name="key">
        ///     The key which is used to remove the window.
        /// </param>
        public static void RemoveWindow(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            try
            {
                if (WindowTable.ContainsKey(key))
                {
                    WindowTable.Remove(key);
                }
            }
            catch (Exception)
            {
                SBPlusClient.LogError("Exception caught removing window " + key);
                throw;
            }
        }

        /// <summary>
        ///     The show debug window.
        /// </summary>
        public static void ShowDebugWindow()
        {
            ShowDebugWindow(true);
        }

        /// <summary>
        ///     Shows or hides the debug window.
        /// </summary>
        /// <param name="show">
        ///     True to show the window, false to hide it.
        /// </param>
        public static void ShowDebugWindow(bool show)
        {
            typeof(SBDebug).InvokeMember(
                "ShowDebugWindow",
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                | BindingFlags.NonPublic,
                Type.DefaultBinder,
                null,
                new object[] { show });

            DebugViewModel.Instance.ApplicationInsightState.IsDebugWindowOpen = show;
        }

        #endregion

        #region Methods

        internal static void ShowSBString(string which, SBString data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

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

        private static void CreateDebugConsole()
        {
            lock (Instance().syncobj)
            {
                try
                {
                    if (((SBPlus.Current == null) || (DebugConsoleWindow != null)) || SBPlus.Current.IsSBPlusClosing)
                    {
                        return;
                    }

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
                catch (Exception exception2)
                {
                    SBPlusClient.LogError("An exception was caught when creating the Debug Console.", exception2);
                }
            }
        }

        private static void CurrentOnExit(object sender, ExitEventArgs exitEventArgs)
        {
            CloseDebugConsole();
        }

        private static void DebugConsoleIsClosed(object sender, EventArgs e)
        {
            if ((DebugConsoleWindow != null) && (DebugConsoleWindow.Dispatcher != null))
            {
                DebugConsoleWindow.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            }

            DebugConsoleWindow = null;
        }

        private static void OpenDebugConsole()
        {
            var thread = new Thread(CreateDebugConsole);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Priority = ThreadPriority.Normal;
            thread.Start();

            Application.Current.Exit += CurrentOnExit;
        }

        #endregion
    }
}