// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStack.xaml.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

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

        private static readonly CommandBinding ClearStackCommandBinding = new CommandBinding(ClearStackCommand);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="ProcessStack" /> class.
        /// </summary>
        static ProcessStack()
        {
            ClearStackCommandBinding.Executed += ExecutedClearStackCommand;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessStack" /> class.
        /// </summary>
        public ProcessStack()
        {
            this.InitializeComponent();

            this.CommandBindings.Add(ClearStackCommandBinding);
        }

        #endregion

        #region Methods

        private static void ExecutedClearStackCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DebugViewModel.Instance.ProcessHistoryStack.Clear();
        }

        #endregion
    }
}