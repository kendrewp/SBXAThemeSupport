// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLoader.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System;
    using System.Windows.Input;

    using SBXA.UI.Client;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for SBStringViewer.xaml
    /// </summary>
    public partial class SBStringViewer
    {
        #region Static Fields

        public static readonly RoutedUICommand DrillDownCommand = new RoutedUICommand(
            "DrillDownCommand", 
            "DrillDownCommand", 
            typeof(SBStringViewer));

        private static readonly CommandBinding DrillDownCommandBinding = new CommandBinding(DrillDownCommand);

        private static readonly CommandBinding RefreshCommonCommandBinding = new CommandBinding(CommonViewer.RefreshCommonCommand);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="SBStringViewer" /> class.
        /// </summary>
        static SBStringViewer()
        {
            DrillDownCommandBinding.Executed += ExecutedDrillDownCommand;
            RefreshCommonCommandBinding.Executed += ExecutedRefreshCommonCommand;
            RefreshCommonCommandBinding.CanExecute += CanExecuteRefreshCommonCommand;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SBStringViewer" /> class.
        /// </summary>
        public SBStringViewer()
        {
            this.InitializeComponent();
            this.CommandBindings.Add(DrillDownCommandBinding);
            this.CommandBindings.Add(RefreshCommonCommandBinding);
        }

        #endregion

        #region Methods

        private static void CanExecuteRefreshCommonCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DebugViewModel.Instance.IsDebugEnabled;
        }

        private static void ExecutedDrillDownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var item = e.Parameter as NestedAttribute;
                if (item == null)
                {
                    return;
                }

                var variable = item.Index;

                DebugWindowManager.ShowSBString(variable, item.Source);
                if (string.IsNullOrEmpty(item.Data))
                {
                    DebugViewModel.GetCommonVariable(variable);
                }
            }
            catch (Exception exception)
            {
                SBPlusClient.LogError("Exception trying to show a nested string.", exception);
            }
        }

        private static void ExecutedRefreshCommonCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var which = e.Parameter as string;
            if (which == null)
            {
                var attr = e.Parameter as NestedAttribute;
                if (attr == null)
                {
                    return;
                }

                which = attr.Variable;
            }

            if (string.IsNullOrEmpty(which))
            {
                return;
            }

            DebugViewModel.GetCommonVariable(which);
        }

        #endregion
    }
}