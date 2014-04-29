// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugConsoleWindow.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="DebugConsoleWindow.xaml.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="DebugConsoleWindow.xaml.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System;
    using System.Windows;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     Interaction logic for DebugConsoleWindow.xaml
    /// </summary>
    public partial class DebugConsoleWindow : Window
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugConsoleWindow" /> class.
        /// </summary>
        public DebugConsoleWindow()
        {
            this.InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            DebugViewModel.Instance.CheckIsConnected();
        }

        #endregion
    }
}