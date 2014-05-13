// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStack.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ProcessStack.xaml.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="ProcessStack.xaml.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for ProcessStack.xaml
    /// </summary>
    public partial class ProcessStack : UserControl
    {
        #region Static Fields

        public static readonly RoutedUICommand ClearStackCommand = new RoutedUICommand(
            "ClearStackCommand", 
            "ClearStackCommand", 
            typeof(ProcessStack));

        public static readonly RoutedUICommand CopyNodeTextCommand = new RoutedUICommand(
            "CopyNodeTextCommand",
            "CopyNodeTextCommand",
            typeof(ProcessStack));

        private static readonly CommandBinding ClearStackCommandBinding = new CommandBinding(ClearStackCommand);

        private static readonly CommandBinding CopyNodeTextCommandBinding = new CommandBinding(CopyNodeTextCommand);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="ProcessStack" /> class.
        /// </summary>
        static ProcessStack()
        {
            ClearStackCommandBinding.Executed += ExecutedClearStackCommand;
            CopyNodeTextCommandBinding.Executed += ExecutedCopyNodeTextCommand;
            CopyNodeTextCommandBinding.CanExecute += CanExecuteCopyNodeTextCommand;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessStack" /> class.
        /// </summary>
        public ProcessStack()
        {
            this.InitializeComponent();

            this.CommandBindings.Add(ClearStackCommandBinding);
            this.CommandBindings.Add(CopyNodeTextCommandBinding);
        }

        #endregion

        #region Methods

        private static void CanExecuteCopyNodeTextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static void ExecutedCopyNodeTextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var treeItem = e.Parameter as TreeItem;
                if (treeItem == null || string.IsNullOrEmpty(treeItem.Name))
                {
                    return;
                }

                System.Windows.Clipboard.Clear();
                System.Windows.Clipboard.SetText(treeItem.Name);
            }
            catch (Exception exception)
            {
                CustomLogger.LogException(exception, "A problem occurred when trying to copy the text of a trace node.");
            }
        }

        private static void ExecutedClearStackCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DebugViewModel.Instance.ClearHistoryStack();
        }

        #endregion
    }
}