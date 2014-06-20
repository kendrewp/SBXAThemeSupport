// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UiAssistant.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    using SBXA.Runtime;
    using SBXA.Shared;
    using SBXA.Shared.Definitions;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    using SBXAThemeSupport.DebugAssistant;

    using ICommand = System.Windows.Input.ICommand;

    /// <summary>
    ///     The ui assistant.
    /// </summary>
    public class UiAssistant : DependencyObject
    {
        #region Static Fields

        /// <summary>
        ///     Determines the visiblity of the close (X) on the main window.
        /// </summary>
        public static readonly DependencyProperty CloseWindowButtonVisiblityProperty =
            DependencyProperty.RegisterAttached(
                "CloseWindowButtonVisiblity", 
                typeof(Visibility), 
                typeof(UiAssistant), 
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// The custom sb plus theme version property.
        /// </summary>
        public static readonly DependencyProperty CustomSBPlusThemeVersionProperty =
            DependencyProperty.RegisterAttached("CustomSBPlusThemeVersion", typeof(string), typeof(UiAssistant));

        /// <summary>
        /// The default background property.
        /// </summary>
        public static readonly DependencyProperty DefaultBackgroundProperty = DependencyProperty.RegisterAttached(
            "DefaultBackground", 
            typeof(Brush), 
            typeof(UiAssistant));

        /// <summary>
        /// The development account name property.
        /// </summary>
        public static readonly DependencyProperty DevelopmentAccountNameProperty =
            DependencyProperty.RegisterAttached("DevelopmentAccountName", typeof(string), typeof(UiAssistant));

        /// <summary>
        /// The development background property.
        /// </summary>
        public static readonly DependencyProperty DevelopmentBackgroundProperty =
            DependencyProperty.RegisterAttached("DevelopmentBackground", typeof(Brush), typeof(UiAssistant));

        /// <summary>
        /// The execute process in context command.
        /// </summary>
        public static readonly RoutedUICommand ExecuteProcessInContextCommand = new RoutedUICommand(
            "ExecuteProcessInContextCommand", 
            "ExecuteProcessInContextCommand", 
            typeof(UiAssistant));

        /// <summary>
        /// The hyperlink parameter property.
        /// </summary>
        public static readonly DependencyProperty HyperlinkParameterProperty = DependencyProperty.RegisterAttached(
            "HyperlinkParameter", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        /// The hyperlink process name property.
        /// </summary>
        public static readonly DependencyProperty HyperlinkProcessNameProperty = DependencyProperty.RegisterAttached(
            "HyperlinkProcessName", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        /// The hyperlink text property.
        /// </summary>
        public static readonly DependencyProperty HyperlinkTextProperty = DependencyProperty.RegisterAttached(
            "HyperlinkText", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        /// The is connected property.
        /// </summary>
        public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register(
            "IsConnected", 
            typeof(bool), 
            typeof(UiAssistant));

        /// <summary>
        /// The live account name property.
        /// </summary>
        public static readonly DependencyProperty LiveAccountNameProperty = DependencyProperty.RegisterAttached(
            "LiveAccountName", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        /// The live background property.
        /// </summary>
        public static readonly DependencyProperty LiveBackgroundProperty = DependencyProperty.RegisterAttached(
            "LiveBackground", 
            typeof(Brush), 
            typeof(UiAssistant));

        /// <summary>
        /// The main window border background property.
        /// </summary>
        public static readonly DependencyProperty MainWindowBorderBackgroundProperty =
            DependencyProperty.RegisterAttached("MainWindowBorderBackground", typeof(Brush), typeof(UiAssistant));

        /// <summary>
        ///     Holds the value that will be set as the title to the MainWindow when it is minimized.
        /// </summary>
        /// <example>
        ///     <code>
        /// LOCAL ERR, RETVAL, STATUS, STATUSDESC
        /// RETVAL = ""
        /// STATUS = ""
        /// STATUSDESC = ""
        /// CALL SB.CALL.STATIC.METHOD("", A"CustomSBPlusTheme", "CustomSBPlusTheme.UiAssistant", "SetMainWindowMinimizedTitle", @MAINWIN:@VM:@PARAM, RETVAL, STATUS, STATUSDESC)
        /// </code>
        /// </example>
        public static readonly DependencyProperty MainWindowMinimizedTitleProperty =
            DependencyProperty.RegisterAttached("MainWindowMinimizedTitle", typeof(string), typeof(UiAssistant));

        /// <summary>
        /// The main window title property.
        /// </summary>
        public static readonly DependencyProperty MainWindowTitleProperty = DependencyProperty.RegisterAttached(
            "MainWindowTitle", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        ///     Setting this property will hide or show the Options menu on the StatusBar
        /// </summary>
        public static readonly DependencyProperty OptionsVisibilityProperty = DependencyProperty.Register(
            "OptionsVisibility", 
            typeof(Visibility), 
            typeof(UiAssistant), 
            new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// The qa account name property.
        /// </summary>
        public static readonly DependencyProperty QaAccountNameProperty = DependencyProperty.RegisterAttached(
            "QaAccountName", 
            typeof(string), 
            typeof(UiAssistant));

        /// <summary>
        /// The qa background property.
        /// </summary>
        public static readonly DependencyProperty QaBackgroundProperty = DependencyProperty.RegisterAttached(
            "QaBackground", 
            typeof(Brush), 
            typeof(UiAssistant));

        /// <summary>
        /// The recognizes access key property.
        /// </summary>
        public static readonly DependencyProperty RecognizesAccessKeyProperty = DependencyProperty.RegisterAttached(
            "RecognizesAccessKey", 
            typeof(bool), 
            typeof(UiAssistant), 
            new PropertyMetadata(true));

        /// <summary>
        /// The resize grip visiblity property.
        /// </summary>
        public static readonly DependencyProperty ResizeGripVisiblityProperty = DependencyProperty.RegisterAttached(
            "ResizeGripVisiblity", 
            typeof(Visibility), 
            typeof(UiAssistant), 
            new PropertyMetadata(Visibility.Visible, OnResizeGripVisiblityChanged));

        /// <summary>
        /// The set drawable property.
        /// </summary>
        public static readonly DependencyProperty SetDrawableProperty = DependencyProperty.RegisterAttached(
            "SetDrawable", 
            typeof(bool), 
            typeof(UiAssistant), 
            new PropertyMetadata(true, OnSetDrawableChanged));

        /// <summary>
        /// The execute process in context command binding.
        /// </summary>
        private static readonly CommandBinding ExecuteProcessInContextCommandBinding = new CommandBinding(ExecuteProcessInContextCommand);

        /// <summary>
        /// The assembly loader.
        /// </summary>
        private static AssemblyLoader assemblyLoader;

        /// <summary>
        /// The current.
        /// </summary>
        private static UiAssistant current;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="UiAssistant" /> class.
        /// </summary>
        static UiAssistant()
        {
            ShowHideOptionsCommand = new RelayCommand(ShowHideOptionsCommandExecuted);
            ShowHideApplicationInsightCommand = new RelayCommand(ShowHideApplicationInsightCommandExecuted);
            BringCurrentFormTopMostUpCommand = new RelayCommand(BringCurrentFormTopMostCommandExecuted);
            SendControlXCommand = new RelayCommand(SendControlXCommandExecuted);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UiAssistant" /> class.
        /// </summary>
        public UiAssistant()
        {
            ExecuteProcessInContextCommandBinding.CanExecute += CanExecuteExecuteProcessInContextCommand;
            ExecuteProcessInContextCommandBinding.Executed += ExecutedExecuteProcessInContextCommand;

            AppDomain.CurrentDomain.AssemblyResolve += HandleAssemblyResolve;

            SBPlusClient.AccountChanged += HandleSBPlusClientAccountChanged;

            Application.Current.MainWindow.StateChanged += HandleMainWindowStateChanged;

            SBPlus.Current.ApplicationShutdown += HandleApplicationShutdown;

            SBPlus.Current.CommandBindings.Add(ExecuteProcessInContextCommandBinding);

            SBPlusClient.Connected += this.HandleConnected;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the bring current form top most up command.
        /// </summary>
        /// <value>
        ///     The bring current form top most up command.
        /// </value>
        public static ICommand BringCurrentFormTopMostUpCommand { get; private set; }

        /// <summary>
        ///     Gets or sets the current.
        /// </summary>
        public static UiAssistant Current
        {
            get
            {
                return current ?? (current = new UiAssistant());
            }

            set
            {
                current = value;
            }
        }

        /// <summary>
        ///     Gets the send control x command command.
        /// </summary>
        /// <value>
        ///     The send control x command command.
        /// </value>
        public static ICommand SendControlXCommand { get; private set; }

        /// <summary>
        ///     Gets the show hide application insight command.
        /// </summary>
        /// <value>
        ///     The show hide application insight command.
        /// </value>
        public static ICommand ShowHideApplicationInsightCommand { get; private set; }

        /// <summary>
        ///     Gets the show hide options command.
        /// </summary>
        /// <value>
        ///     The show hide options command.
        /// </value>
        public static ICommand ShowHideOptionsCommand { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [is connected].
        /// </summary>
        /// <example>
        ///     <code lang="XAML">
        /// ...
        /// {Binding Path=IsConnected, Source={Static SBXAThemeSupport:UiAssitant.Current}}
        /// ...
        /// </code>
        /// </example>
        /// <value>
        ///     <c>true</c> if [is connected]; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return (bool)this.GetValue(IsConnectedProperty);
            }

            set
            {
                this.SetValue(IsConnectedProperty, value);
            }
        }

        /// <summary>
        ///     Gets or sets the IsOptionsVisibleProperty. This is a DependencyProperty.
        /// </summary>
        public Visibility OptionsVisibility
        {
            get
            {
                return (Visibility)this.GetValue(OptionsVisibilityProperty);
            }

            set
            {
                this.SetValue(OptionsVisibilityProperty, value);
            }
        }

        /// <summary>
        ///     Gets the version.
        /// </summary>
        public string Version
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(typeof(UiAssistant).Assembly.Location).FileVersion;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the value of CloseWindowButtonVisiblity
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of CloseWindowButtonVisiblity
        /// </returns>
        public static Visibility GetCloseWindowButtonVisiblity(DependencyObject target)
        {
            return (Visibility)target.GetValue(CloseWindowButtonVisiblityProperty);
        }

        /// <summary>
        /// Gets the value of CustomSBPlusThemeVersion
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of CustomSBPlusThemeVersion
        /// </returns>
        public static string GetCustomSBPlusThemeVersion(DependencyObject target)
        {
            return (string)target.GetValue(CustomSBPlusThemeVersionProperty);
        }

        /// <summary>
        /// Gets the value of DefaultBackground
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of DefaultBackground
        /// </returns>
        public static Brush GetDefaultBackground(DependencyObject target)
        {
            return (Brush)target.GetValue(DefaultBackgroundProperty);
        }

        /// <summary>
        /// Gets the value of DevelopmentAccountName
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of DevelopmentAccountName
        /// </returns>
        public static string GetDevelopmentAccountName(DependencyObject target)
        {
            return (string)target.GetValue(DevelopmentAccountNameProperty);
        }

        /// <summary>
        /// Gets the value of DevelopmentBackground
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of DevelopmentBackground
        /// </returns>
        public static Brush GetDevelopmentBackground(DependencyObject target)
        {
            return (Brush)target.GetValue(DevelopmentBackgroundProperty);
        }

        /// <summary>
        /// Gets the value of HyperlinkParameter
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of HyperlinkParameter
        /// </returns>
        public static string GetHyperlinkParameter(DependencyObject target)
        {
            return (string)target.GetValue(HyperlinkParameterProperty);
        }

        /// <summary>
        /// Gets the value of HyperlinkProcessName
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of HyperlinkProcessName
        /// </returns>
        public static string GetHyperlinkProcessName(DependencyObject target)
        {
            return (string)target.GetValue(HyperlinkProcessNameProperty);
        }

        /// <summary>
        /// Gets the value of HyperlinkText
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of HyperlinkText
        /// </returns>
        public static string GetHyperlinkText(DependencyObject target)
        {
            return (string)target.GetValue(HyperlinkTextProperty);
        }

        /// <summary>
        /// Gets the value of LiveAccountName
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of LiveAccountName
        /// </returns>
        public static string GetLiveAccountName(DependencyObject target)
        {
            return (string)target.GetValue(LiveAccountNameProperty);
        }

        /// <summary>
        /// Gets the value of LiveBackground
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of LiveBackground
        /// </returns>
        public static Brush GetLiveBackground(DependencyObject target)
        {
            return (Brush)target.GetValue(LiveBackgroundProperty);
        }

        /// <summary>
        /// Gets the value of MainWindowBorderBackground
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of MainWindowBorderBackground
        /// </returns>
        public static Brush GetMainWindowBorderBackground(DependencyObject target)
        {
            return (Brush)target.GetValue(MainWindowBorderBackgroundProperty);
        }

        /// <summary>
        /// Gets the value of MainWindowMinimizedTitle
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of MainWindowMinimizedTitle
        /// </returns>
        public static string GetMainWindowMinimizedTitle(DependencyObject target)
        {
            return (string)target.GetValue(MainWindowMinimizedTitleProperty);
        }

        /// <summary>
        /// Gets the value of MainWindowTitle
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of MainWindowTitle
        /// </returns>
        public static string GetMainWindowTitle(DependencyObject target)
        {
            return (string)target.GetValue(MainWindowTitleProperty);
        }

        /// <summary>
        /// Gets the value of QaAccountName
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of QaAccountName
        /// </returns>
        public static string GetQaAccountName(DependencyObject target)
        {
            return (string)target.GetValue(QaAccountNameProperty);
        }

        /// <summary>
        /// Gets the value of QaBackground
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of QaBackground
        /// </returns>
        public static Brush GetQaBackground(DependencyObject target)
        {
            return (Brush)target.GetValue(QaBackgroundProperty);
        }

        /// <summary>
        /// Gets the value of RecognizesAccessKey
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of RecognizesAccessKey
        /// </returns>
        public static bool GetRecognizesAccessKey(DependencyObject target)
        {
            return (bool)target.GetValue(RecognizesAccessKeyProperty);
        }

        /// <summary>
        /// Gets the resize grip visiblity.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// Returns if the proerpty is visible or not.
        /// </returns>
        public static Visibility GetResizeGripVisiblity(DependencyObject target)
        {
            return (Visibility)target.GetValue(ResizeGripVisiblityProperty);
        }

        /// <summary>
        /// Gets the value of SetDrawable
        /// </summary>
        /// <param name="target">
        /// The <see cref="DependencyObject"/> that <see cref="DependencyProperty"/> will be applied to.
        /// </param>
        /// <returns>
        /// The value of SetDrawable
        /// </returns>
        public static bool GetSetDrawable(DependencyObject target)
        {
            return (bool)target.GetValue(SetDrawableProperty);
        }

        /// <summary>
        /// The send response.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        public static void SendResponse(SBString response)
        {
            SBPlus.Current.SetInputState(SBInputState.Dormant, "Sending response to SB/XA server.");
            SBPlusClient.LogInformation(
                "Responded to message." + response.GetStandardString() + "(" + Convert.ToString(DateTime.Now.Millisecond) + ")");
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

        /// <summary>
        /// Sets the value of the CloseWindowButtonVisiblity property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetCloseWindowButtonVisiblity(DependencyObject target, Visibility value)
        {
            target.SetValue(CloseWindowButtonVisiblityProperty, value);
        }

        /// <summary>
        /// Sets the value of the CustomSBPlusThemeVersion property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetCustomSBPlusThemeVersion(DependencyObject target, string value)
        {
            target.SetValue(CustomSBPlusThemeVersionProperty, value);
        }

        /// <summary>
        /// Sets the value of the DefaultBackground property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetDefaultBackground(DependencyObject target, Brush value)
        {
            target.SetValue(DefaultBackgroundProperty, value);
        }

        /// <summary>
        /// Sets the value of the DevelopmentAccountName property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetDevelopmentAccountName(DependencyObject target, string value)
        {
            target.SetValue(DevelopmentAccountNameProperty, value);
        }

        /// <summary>
        /// Sets the value of the DevelopmentBackground property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetDevelopmentBackground(DependencyObject target, Brush value)
        {
            target.SetValue(DevelopmentBackgroundProperty, value);
        }

        /// <summary>
        /// Sets the value of the HyperlinkParameter property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetHyperlinkParameter(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkParameterProperty, value);
        }

        /// <summary>
        /// Sets the value of the HyperlinkProcessName property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetHyperlinkProcessName(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkProcessNameProperty, value);
        }

        /// <summary>
        /// Sets the value of the HyperlinkText property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetHyperlinkText(DependencyObject target, string value)
        {
            target.SetValue(HyperlinkTextProperty, value);
        }

        /// <summary>
        /// Sets the value of the LiveAccountName property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetLiveAccountName(DependencyObject target, string value)
        {
            target.SetValue(LiveAccountNameProperty, value);
        }

        /// <summary>
        /// Sets the value of the LiveBackground property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetLiveBackground(DependencyObject target, Brush value)
        {
            target.SetValue(LiveBackgroundProperty, value);
        }

        /// <summary>
        /// Sets the value of the MainWindowBorderBackground property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetMainWindowBorderBackground(DependencyObject target, Brush value)
        {
            target.SetValue(MainWindowBorderBackgroundProperty, value);
        }

        /// <summary>
        /// Sets the value of the MainWindowMinimizedTitle property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetMainWindowMinimizedTitle(DependencyObject target, string value)
        {
            target.SetValue(MainWindowMinimizedTitleProperty, value);
        }

        /// <summary>
        /// Sets the value of the MainWindowTitle property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetMainWindowTitle(DependencyObject target, string value)
        {
            target.SetValue(MainWindowTitleProperty, value);
        }

        /// <summary>
        /// Sets the value of the QaAccountName property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetQaAccountName(DependencyObject target, string value)
        {
            target.SetValue(QaAccountNameProperty, value);
        }

        /// <summary>
        /// Sets the value of the QaBackground property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetQaBackground(DependencyObject target, Brush value)
        {
            target.SetValue(QaBackgroundProperty, value);
        }

        /// <summary>
        /// Sets the value of the RecognizesAccessKey property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetRecognizesAccessKey(DependencyObject target, bool value)
        {
            target.SetValue(RecognizesAccessKeyProperty, value);
        }

        /// <summary>
        /// Sets the resize grip visiblity.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public static void SetResizeGripVisiblity(DependencyObject target, Visibility value)
        {
            target.SetValue(ResizeGripVisiblityProperty, value);
        }

        /// <summary>
        /// Sets the value of the SetDrawable property.
        /// </summary>
        /// <param name="target">
        /// The depdendency object that this property is attached to.
        /// </param>
        /// <param name="value">
        /// The value to set it to.
        /// </param>
        public static void SetSetDrawable(DependencyObject target, bool value)
        {
            target.SetValue(SetDrawableProperty, value);
        }

        /// <summary>
        ///     The init.
        /// </summary>
        public void Init()
        {
            SBFormSupport.AddHandlers();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The bring current form top most command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private static void BringCurrentFormTopMostCommandExecuted(object parameter)
        {
            var keyEventArgs = parameter as KeyEventArgs;
            if (keyEventArgs == null || keyEventArgs.Handled)
            {
                return;
            }

            DebugWindowManager.BringTopMost();
            keyEventArgs.Handled = true;
        }

        /// <summary>
        /// The can execute execute process in context command.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void CanExecuteExecuteProcessInContextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationHelper.CanSendServerCommands();
        }

        /// <summary>
        /// The executed execute process in context command.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void ExecutedExecuteProcessInContextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SbProcessHandler.CallProcess(e.Parameter as string, true);
        }

        /// <summary>
        /// The handle application shutdown.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HandleApplicationShutdown(object sender, ApplicationShutdownEventArgs e)
        {
            // Close the debug window is it is there.
            DebugWindowManager.Dispose();
        }

        /// <summary>
        /// The handle assembly resolve.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="Assembly"/>.
        /// </returns>
        private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            if (ExtensibilityManager.CustomAssemblyResolver != null)
            {
                assembly = ExtensibilityManager.CustomAssemblyResolver.Resolve(args.Name);
            }

            if (assembly == null)
            {
                // only load the assembly loaded if the base one could not be loaded, or find the assembly.
                if (assemblyLoader == null)
                {
                    assemblyLoader = new AssemblyLoader("CustomSBPlusTheme");
                }

                assembly = assemblyLoader.Resolve(args.Name);
            }

            return assembly;
        }

        /// <summary>
        /// The handle main window state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HandleMainWindowStateChanged(object sender, EventArgs e)
        {
            var sbMainWindow = sender as SBMainWindow;
            if (sbMainWindow == null)
            {
                return;
            }

            string minimizedTitle = GetMainWindowMinimizedTitle(SBPlus.Current);
            string normalTitle = GetMainWindowTitle(SBPlus.Current);

            if (sbMainWindow.WindowState == WindowState.Minimized && !string.IsNullOrEmpty(minimizedTitle))
            {
                sbMainWindow.Title = minimizedTitle;
            }
            else
            {
                if (!string.IsNullOrEmpty(normalTitle))
                {
                    sbMainWindow.Title = normalTitle;
                }
            }
        }

        /// <summary>
        /// The handle sb plus client account changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void HandleSBPlusClientAccountChanged(object sender, SBAccountChangedEventArgs e)
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

        /// <summary>
        /// Called when [resize grip visiblity changed].
        /// </summary>
        /// <param name="d">
        /// The <see cref="DependencyObject"/> that the property will act on.
        /// </param>
        /// <param name="e">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnResizeGripVisiblityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // find the parent window, then use that to find the resizegrip
                var parentWindow = SBUISupport.FindParentByType(d, typeof(Window)) as Window;
                if (parentWindow == null)
                {
                    var sbForm = d as SBForm;
                    if (sbForm == null)
                    {
                        return;
                    }

                    if (sbForm.ParentSBWindow == null)
                    {
                        return;
                    }

                    parentWindow = sbForm.ParentSBWindow as Window;
                }

                var resizeGrip = SBUISupport.FindChildByType(parentWindow, typeof(ResizeGrip)) as ResizeGrip;
                if (resizeGrip == null)
                {
                    return;
                }

                resizeGrip.Visibility = (Visibility)e.NewValue;
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception caught setting resize grip visiblity.", exception);
            }
        }

        /// <summary>
        /// The on set drawable changed.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void OnSetDrawableChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            // if (SBPlus.Current.CommandLineArguments.ContainsKey(GenericConstants.CL_START_APPLICATION))
            if ((bool)args.NewValue)
            {
                SBGUIAttribute.SetSBDrawable(target, GenericConstants.TRUE_SB_STRING);
            }
            else
            {
                SBGUIAttribute.SetSBDrawable(target, GenericConstants.FALSE_SB_STRING);
            }
        }

        /// <summary>
        /// The send control x.
        /// </summary>
        private static void SendControlX()
        {
            var field2 = SBFocusManager.FocusedControl as ISBField;
            // ReSharper disable ConvertIfStatementToConditionalTernaryExpression
            if (field2 != null)
            {
                SendResponse(
                    new GuiInputEvent(
                        field2.SBValue, 
                        SBCommands.SBCtrlxCommand.SBEvent, 
                        SBCommands.SBCtrlxCommand.SBKeyValue, 
                        field2.CursorPosition + 1).ResponseString);
            }
            else
            {
                SendResponse(
                    new GuiInputEvent(string.Empty, SBCommands.SBCtrlxCommand.SBEvent, SBCommands.SBCtrlxCommand.SBKeyValue, 1)
                        .ResponseString);
            }

            // ReSharper restore ConvertIfStatementToConditionalTernaryExpression
        }

        /// <summary>
        /// The send control x command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private static void SendControlXCommandExecuted(object parameter)
        {
            var keyEventArgs = parameter as KeyEventArgs;
            if (keyEventArgs == null || keyEventArgs.Handled)
            {
                return;
            }

            SendControlX();
            keyEventArgs.Handled = true;
        }

        /// <summary>
        /// The show hide application insight command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private static void ShowHideApplicationInsightCommandExecuted(object parameter)
        {
            var keyEventArgs = parameter as KeyEventArgs;
            if (keyEventArgs == null || keyEventArgs.Handled)
            {
                return;
            }

            DebugWindowManager.FlipDebugConsole();
            keyEventArgs.Handled = true;
        }

        /// <summary>
        /// The show hide options command executed.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private static void ShowHideOptionsCommandExecuted(object parameter)
        {
            var keyEventArgs = parameter as KeyEventArgs;
            if (keyEventArgs == null || keyEventArgs.Handled)
            {
                return;
            }

            // If there is an application definition which is defined then recognize the DisplayOptionsMenu, otherwise just switch it.
            if (SBPlus.Current != null && SBPlus.Current.ApplicationDefinition != null
                && SBPlus.Current.ApplicationDefinition.DisplayOptionsMenu)
            {
                Current.OptionsVisibility = Current.OptionsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                Current.OptionsVisibility = Current.OptionsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }

            keyEventArgs.Handled = true;
        }

        /// <summary>
        /// The handle connected.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleConnected(object sender, ConnectedEventArgs e)
        {
            this.SetIsConnected(e.Connected);
        }

        /// <summary>
        /// The set is connected.
        /// </summary>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        private void SetIsConnected(bool newValue)
        {
            JobManager.RunInUIThread(DispatcherPriority.Input, () => this.IsConnected = newValue);
        }

        #endregion
    }

    /// <summary>
    ///     The relay command.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        /// <summary>
        /// The can execute.
        /// </summary>
        private readonly Predicate<object> canExecute;

        /// <summary>
        /// The execute.
        /// </summary>
        private readonly Action<object> execute;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">
        /// The execute.
        /// </param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">
        /// The execute.
        /// </param>
        /// <param name="canExecute">
        /// The can execute.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// execute
        /// </exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     The can execute changed.
        /// </summary>
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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The can execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        #endregion
    }
}