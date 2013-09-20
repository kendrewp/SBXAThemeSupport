﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SBXA.Shared;
using SBXA.Shared.Definitions;
using SBXA.UI.Client;
using SBXA.UI.WPFControls;
using SBXAThemeSupport.DebugAssistant;
using ICommand = System.Windows.Input.ICommand;

namespace SBXAThemeSupport
{
    public class UiAssistant : DependencyObject
    {
        private static UiAssistant _Current;
        private static AssemblyLoader _AssemblyLoader;
        public static readonly RoutedUICommand ExecuteProcessInContextCommand = new RoutedUICommand("ExecuteProcessInContextCommand", "ExecuteProcessInContextCommand", typeof(UiAssistant));
        public static CommandBinding ExecuteProcessInContextCommandBinding = new CommandBinding(ExecuteProcessInContextCommand);

        public UiAssistant()
        {
            KeyUpCommand = new RelayCommand(KeyUpCommandExecuted);
            ExecuteProcessInContextCommandBinding.CanExecute += CanExecuteExecuteProcessInContextCommand;
            ExecuteProcessInContextCommandBinding.Executed += ExecutedExecuteProcessInContextCommand;

            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;

            SBPlusClient.AccountChanged += HandleSBPlusClientAccountChanged;

            Application.Current.MainWindow.StateChanged += HandleMainWindowStateChanged;

            SBPlus.Current.ApplicationShutdown += HandleApplicationShutdown;

            SBPlus.Current.CommandBindings.Add(ExecuteProcessInContextCommandBinding);
        }

        static void CanExecuteExecuteProcessInContextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SBProcessRunner.CanSendServerCommands();
        }

        static void ExecutedExecuteProcessInContextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SBProcessRunner.ExecuteSBPlusProcess(e.Parameter as string, true);
        }

        static void HandleApplicationShutdown(object sender, ApplicationShutdownEventArgs e)
        {
            // Close the debug window is it is there.
            DebugWindowManager.ShowDebugWindow(false);
        }

        static void HandleMainWindowStateChanged(object sender, EventArgs e)
        {
            SBMainWindow sbMainWindow = sender as SBMainWindow;
            if (sbMainWindow == null) return;

            string minimizedTitle = GetMainWindowMinimizedTitle(SBPlus.Current);
            string normalTitle = GetMainWindowTitle(SBPlus.Current);

            if (sbMainWindow.WindowState == WindowState.Minimized && !String.IsNullOrEmpty(minimizedTitle))
            {
                sbMainWindow.Title = minimizedTitle;
            }
            else
            {
                if (!String.IsNullOrEmpty(normalTitle))
                {
                    sbMainWindow.Title = normalTitle;
                }
            }
        }

        static void HandleSBPlusClientAccountChanged(object sender, SBAccountChangedEventArgs e)
        {
            if (e.AccountName.Equals(GetDevelopmentAccountName(SBPlus.Current)))
            {
                SetMainWindowBorderBackground(SBPlus.Current, GetDevelopmentBackground(SBPlus.Current));
                return;
            }
            if (e.AccountName.Equals(GetQaAccountName(SBPlus.Current)))
            {
                SetMainWindowBorderBackground(SBPlus.Current, GetQaBackground(SBPlus.Current));
                return;
            }

            if (e.AccountName.Equals(GetLiveAccountName(SBPlus.Current)))
            {
                SetMainWindowBorderBackground(SBPlus.Current, GetLiveBackground(SBPlus.Current));
                return;
            }

            SetMainWindowBorderBackground(SBPlus.Current, GetDefaultBackground(SBPlus.Current));
            
        }

        public void Init()
        {
            SBFormSupport.AddHandlers();
        }

        private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            if (ExtensibilityManager.CustomAssemblyResolver != null)
            {
                assembly = ExtensibilityManager.CustomAssemblyResolver.Resolve(args.Name);
            }
            if (assembly == null)
            {
                // only load thi assembly loaded if the base one could not be loaded, or find the assembly.
                if (_AssemblyLoader == null)
                {
                    _AssemblyLoader = new AssemblyLoader("CustomSBPlusTheme");
                }
                assembly = _AssemblyLoader.Resolve(args.Name);
            }
            return (assembly);
        }

        public ICommand KeyUpCommand
        {
            get;
            private set;
        }

        public static UiAssistant Current
        {
            get { return _Current ?? (_Current = new UiAssistant()); }
            set { _Current = value; }
        }

        private static void KeyUpCommandExecuted(object parameter)
        {
            var keyEventArgs = parameter as KeyEventArgs;
            if (keyEventArgs == null) return;

            bool isCtrlShift = (Keyboard.Modifiers &
                                (ModifierKeys.Control | ModifierKeys.Shift)) ==
                                (ModifierKeys.Control | ModifierKeys.Shift);

            if (!isCtrlShift)
            {
                // Check if the user hit Ctrl-X, if so send a Ctr-X to the server.
                if (keyEventArgs.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control && !keyEventArgs.Handled)
                {
                    SendControlX();
                }
                return;
            }
            switch (keyEventArgs.Key)
            {
                case Key.O:
                    // Settings.Instance.IsOptionsVisible = Visibility.Visible;
                    Current.OptionsVisibility = Current.OptionsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case Key.D:
                    DebugWindowManager.ShowDebugWindow();
                    break;
            }
        }

        private static void SendControlX()
        {
            ISBField field2 = SBFocusManager.FocusedControl as ISBField;
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if (field2 != null)
            {
                SendResponse(new GuiInputEvent(field2.SBValue, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, field2.CursorPosition + 1).ResponseString);
            }
            else
            {
                SendResponse(new GuiInputEvent(String.Empty, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, 1).ResponseString);
            }
// ReSharper restore ConvertIfStatementToConditionalTernaryExpression
        }

        public static void SendResponse(SBString response)
        {
            SBPlus.Current.SetInputState(SBInputState.Dormant, "Sending response to SB/XA server.");
            SBPlusClient.LogInformation("Responded to message." + response.GetStandardString() + "(" + Convert.ToString(DateTime.Now.Millisecond) + ")");
            try
            {
                SBPlus.Current.SBPlusRuntime.RespondToLastMessage(response);
            }
            catch (SBApplicationException exception)
            {
                if (exception.ErrorCode.Equals("CS0049"))
                {
                    SBDisplay.Display(SBDisplayTypes.Fatal, "An Encoding error occurred handing a message", exception);
                    SBPlus.Current.SetInputState(SBInputState.WaitingForInput, "Encoding error.");
                }
            }
            catch (Exception exception2)
            {
                SBPlusClient.LogError("An error occurred handing a message.", exception2);
                SBPlus.Current.SetInputState(SBInputState.WaitingForInput, "An error occurred handing a message.");
            }
        }

        #region OptionsVisibility Property
        /// <summary>
        /// Setting this property will hide or show the Options menu on the StatusBar
        /// </summary>
        public static readonly DependencyProperty OptionsVisibilityProperty =
            DependencyProperty.Register(
                "OptionsVisibility",
                typeof(Visibility),
                typeof (UiAssistant),
                new PropertyMetadata(Visibility.Collapsed)
                );


        /// <summary>
        /// Gets or sets the IsOptionsVisibleProperty. This is a DependencyProperty.
        /// </summary>
        public Visibility OptionsVisibility
        {
            get
            {
                return ((Visibility)GetValue(OptionsVisibilityProperty));
            }
            set
            {
                SetValue(OptionsVisibilityProperty, value);
            }
        }

        #endregion OptionsVisibility Property

        #region CloseWindowButtonVisiblity Property

        /// <summary>
        /// Determines the visiblity of the close (X) on the main window.
        /// </summary>
        public static readonly DependencyProperty CloseWindowButtonVisiblityProperty =
            DependencyProperty.RegisterAttached(
                "CloseWindowButtonVisiblity",
                typeof (Visibility),
                typeof (UiAssistant),
                new PropertyMetadata(Visibility.Visible)
                );

        /// <summary>
        /// Sets the value of the CloseWindowButtonVisiblity property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetCloseWindowButtonVisiblity(DependencyObject target, Visibility value)
        {
            target.SetValue(CloseWindowButtonVisiblityProperty, value);
        }

        /// <summary>
        /// Gets the value of CloseWindowButtonVisiblity
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of CloseWindowButtonVisiblity</returns>
        public static Visibility GetCloseWindowButtonVisiblity(DependencyObject target)
        {
            return ((Visibility) target.GetValue(CloseWindowButtonVisiblityProperty));
        }

        #endregion CloseWindowButtonVisiblity Property

        #region DevelopmentAccountName Property

        public static readonly DependencyProperty DevelopmentAccountNameProperty =
            DependencyProperty.RegisterAttached(
                "DevelopmentAccountName",
                typeof (string),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the DevelopmentAccountName property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetDevelopmentAccountName(DependencyObject target, string value)
        {
            target.SetValue(DevelopmentAccountNameProperty, value);
        }

        /// <summary>
        /// Gets the value of DevelopmentAccountName
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of DevelopmentAccountName</returns>
        public static string GetDevelopmentAccountName(DependencyObject target)
        {
            return ((string) target.GetValue(DevelopmentAccountNameProperty));
        }

        #endregion DevelopmentAccountName Property

        #region QaAccountName Property

        public static readonly DependencyProperty QaAccountNameProperty =
            DependencyProperty.RegisterAttached(
                "QaAccountName",
                typeof (string),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the QaAccountName property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetQaAccountName(DependencyObject target, string value)
        {
            target.SetValue(QaAccountNameProperty, value);
        }

        /// <summary>
        /// Gets the value of QaAccountName
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of QaAccountName</returns>
        public static string GetQaAccountName(DependencyObject target)
        {
            return ((string) target.GetValue(QaAccountNameProperty));
        }

        #endregion QaAccountName Property

        #region LiveAccountName Property

        public static readonly DependencyProperty LiveAccountNameProperty =
            DependencyProperty.RegisterAttached(
                "LiveAccountName",
                typeof (string),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the LiveAccountName property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetLiveAccountName(DependencyObject target, string value)
        {
            target.SetValue(LiveAccountNameProperty, value);
        }

        /// <summary>
        /// Gets the value of LiveAccountName
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of LiveAccountName</returns>
        public static string GetLiveAccountName(DependencyObject target)
        {
            return ((string) target.GetValue(LiveAccountNameProperty));
        }

        #endregion LiveAccountName Property

        #region MainWindowBorderBackground Property

        public static readonly DependencyProperty MainWindowBorderBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "MainWindowBorderBackground",
                typeof (Brush),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the MainWindowBorderBackground property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetMainWindowBorderBackground(DependencyObject target, Brush value)
        {
            target.SetValue(MainWindowBorderBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the value of MainWindowBorderBackground
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of MainWindowBorderBackground</returns>
        public static Brush GetMainWindowBorderBackground(DependencyObject target)
        {
            return ((Brush) target.GetValue(MainWindowBorderBackgroundProperty));
        }

        #endregion MainWindowBorderBackground Property

        #region LiveBackground Property

        public static readonly DependencyProperty LiveBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "LiveBackground",
                typeof (Brush),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the LiveBackground property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetLiveBackground(DependencyObject target, Brush value)
        {
            target.SetValue(LiveBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the value of LiveBackground
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of LiveBackground</returns>
        public static Brush GetLiveBackground(DependencyObject target)
        {
            return ((Brush) target.GetValue(LiveBackgroundProperty));
        }

        #endregion LiveBackground Property

        #region DevelopmentBackground Property

        public static readonly DependencyProperty DevelopmentBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "DevelopmentBackground",
                typeof (Brush),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the DevelopmentBackground property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetDevelopmentBackground(DependencyObject target, Brush value)
        {
            target.SetValue(DevelopmentBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the value of DevelopmentBackground
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of DevelopmentBackground</returns>
        public static Brush GetDevelopmentBackground(DependencyObject target)
        {
            return ((Brush) target.GetValue(DevelopmentBackgroundProperty));
        }

        #endregion DevelopmentBackground Property

        #region QaBackground Property

        public static readonly DependencyProperty QaBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "QaBackground",
                typeof (Brush),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the QaBackground property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetQaBackground(DependencyObject target, Brush value)
        {
            target.SetValue(QaBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the value of QaBackground
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of QaBackground</returns>
        public static Brush GetQaBackground(DependencyObject target)
        {
            return ((Brush) target.GetValue(QaBackgroundProperty));
        }

        #endregion QaBackground Property

        #region DefaultBackground Property

        public static readonly DependencyProperty DefaultBackgroundProperty =
            DependencyProperty.RegisterAttached(
                "DefaultBackground",
                typeof (Brush),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the DefaultBackground property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetDefaultBackground(DependencyObject target, Brush value)
        {
            target.SetValue(DefaultBackgroundProperty, value);
        }

        /// <summary>
        /// Gets the value of DefaultBackground
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of DefaultBackground</returns>
        public static Brush GetDefaultBackground(DependencyObject target)
        {
            return ((Brush) target.GetValue(DefaultBackgroundProperty));
        }

        #endregion DefaultBackground Property

        #region MainWindowMinimizedTitle Property

        /// <summary>
        /// Holds the value that will be set as the title to the MainWindow when it is minimized.
        /// </summary>
        /// <example>
        /// <code>
        /// LOCAL ERR, RETVAL, STATUS, STATUSDESC
        /// RETVAL = ""
        /// STATUS = ""
        /// STATUSDESC = ""
        /// CALL SB.CALL.STATIC.METHOD("", A"CustomSBPlusTheme", "CustomSBPlusTheme.UiAssistant", "SetMainWindowMinimizedTitle", @MAINWIN:@VM:@PARAM, RETVAL, STATUS, STATUSDESC)
        /// </code>
        /// </example>
        public static readonly DependencyProperty MainWindowMinimizedTitleProperty =
            DependencyProperty.RegisterAttached(
                "MainWindowMinimizedTitle",
                typeof (string),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the MainWindowMinimizedTitle property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetMainWindowMinimizedTitle(DependencyObject target, string value)
        {
            target.SetValue(MainWindowMinimizedTitleProperty, value);
        }

        /// <summary>
        /// Gets the value of MainWindowMinimizedTitle
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of MainWindowMinimizedTitle</returns>
        public static string GetMainWindowMinimizedTitle(DependencyObject target)
        {
            return ((string) target.GetValue(MainWindowMinimizedTitleProperty));
        }

        #endregion MainWindowMinimizedTitle Property

        #region MainWindowTitle Property

        public static readonly DependencyProperty MainWindowTitleProperty =
            DependencyProperty.RegisterAttached(
                "MainWindowTitle",
                typeof (string),
                typeof (UiAssistant));

        /// <summary>
        /// Sets the value of the MainWindowTitle property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetMainWindowTitle(DependencyObject target, string value)
        {
            target.SetValue(MainWindowTitleProperty, value);
        }

        /// <summary>
        /// Gets the value of MainWindowTitle
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of MainWindowTitle</returns>
        public static string GetMainWindowTitle(DependencyObject target)
        {
            return ((string) target.GetValue(MainWindowTitleProperty));
        }

        #endregion MainWindowTitle Property

        #region CustomSBPlusThemeVersion Property

        public static readonly DependencyProperty CustomSBPlusThemeVersionProperty =
            DependencyProperty.RegisterAttached(
                "CustomSBPlusThemeVersion",
                typeof (string),
                typeof (UiAssistant)
                );

        /// <summary>
        /// Sets the value of the CustomSBPlusThemeVersion property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetCustomSBPlusThemeVersion(DependencyObject target, string value)
        {
            target.SetValue(CustomSBPlusThemeVersionProperty, value);
        }

        /// <summary>
        /// Gets the value of CustomSBPlusThemeVersion
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of CustomSBPlusThemeVersion</returns>
        public static string GetCustomSBPlusThemeVersion(DependencyObject target)
        {
            return ((string)target.GetValue(CustomSBPlusThemeVersionProperty));
        }

        #endregion CustomSBPlusThemeVersion Property

        #region RecognizesAccessKey Property

        public static readonly DependencyProperty RecognizesAccessKeyProperty =
            DependencyProperty.RegisterAttached(
                "RecognizesAccessKey",
                typeof (bool),
                typeof (UiAssistant),
                new PropertyMetadata(true)
                );

        /// <summary>
        /// Sets the value of the RecognizesAccessKey property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetRecognizesAccessKey(DependencyObject target, bool value)
        {
            target.SetValue(RecognizesAccessKeyProperty, value);
        }

        /// <summary>
        /// Gets the value of RecognizesAccessKey
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of RecognizesAccessKey</returns>
        public static bool GetRecognizesAccessKey(DependencyObject target)
        {
            return ((bool) target.GetValue(RecognizesAccessKeyProperty));
        }

        #endregion RecognizesAccessKey Property

        public string Version
        {
            get { return FileVersionInfo.GetVersionInfo(typeof (UiAssistant).Assembly.Location).FileVersion; }
        }

        #region HyperlinkParameter Property

        public static readonly DependencyProperty HyperlinkParameterProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkParameter",
                typeof(string),
                typeof(UiAssistant));

        /// <summary>
        /// Sets the value of the HyperlinkParameter property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetHyperlinkParameter(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkParameterProperty, value);
        }

        /// <summary>
        /// Gets the value of HyperlinkParameter
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of HyperlinkParameter</returns>
        public static string GetHyperlinkParameter(DependencyObject target)
        {
            return ((string)target.GetValue(HyperlinkParameterProperty));
        }

        #endregion HyperlinkParameter Property

        #region HyperlinkProcessName Property

        public static readonly DependencyProperty HyperlinkProcessNameProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkProcessName",
                typeof(string),
                typeof(UiAssistant));

        /// <summary>
        /// Sets the value of the HyperlinkProcessName property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetHyperlinkProcessName(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkProcessNameProperty, value);
        }

        /// <summary>
        /// Gets the value of HyperlinkProcessName
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of HyperlinkProcessName</returns>
        public static string GetHyperlinkProcessName(DependencyObject target)
        {
            return ((string)target.GetValue(HyperlinkProcessNameProperty));
        }

        #endregion HyperlinkProcessName Property

        #region HyperlinkText Property

        public static readonly DependencyProperty HyperlinkTextProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkText",
                typeof(string),
                typeof(UiAssistant));

        /// <summary>
        /// Sets the value of the HyperlinkText property.
        /// </summary>
        /// <param name="target">The depdendency object that this property is attached to.</param>
        /// <param name="value">The value to set it to.</param>
        public static void SetHyperlinkText(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkTextProperty, value);
        }

        /// <summary>
        /// Gets the value of HyperlinkText
        /// </summary>
        /// <param name="target"></param>
        /// <returns>The value of HyperlinkText</returns>
        public static string GetHyperlinkText(DependencyObject target)
        {
            return ((string)target.GetValue(HyperlinkTextProperty));
        }

        #endregion HyperlinkText Property
    }

    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Predicate<object> _CanExecute;
        private readonly Action<object> _Execute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _Execute = execute;
            _CanExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }

        #endregion
    }
}
