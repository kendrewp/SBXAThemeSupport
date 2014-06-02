// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonViewer.xaml.cs" company="Ruf Informatik AG">
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
    using System.Windows.Controls;
    using System.Windows.Input;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for CommonViewer.xaml
    /// </summary>
    public partial class CommonViewer : UserControl
    {
        #region Static Fields

        /// <summary>
        /// The refresh common command.
        /// </summary>
        public static readonly RoutedUICommand RefreshCommonCommand = new RoutedUICommand(
            "RefreshCommonCommand", 
            "RefreshCommonCommand", 
            typeof(CommonViewer));

        /// <summary>
        /// The refresh common command binding.
        /// </summary>
        private static readonly CommandBinding RefreshCommonCommandBinding = new CommandBinding(RefreshCommonCommand);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="CommonViewer" /> class.
        /// </summary>
        static CommonViewer()
        {
            RefreshCommonCommandBinding.Executed += ExecutedRefreshCommonCommand;
            RefreshCommonCommandBinding.CanExecute += CanExecuteRefreshCommonCommand;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommonViewer" /> class.
        /// </summary>
        public CommonViewer()
        {
            this.InitializeComponent();

            this.CommandBindings.Add(RefreshCommonCommandBinding);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The can execute refresh common command.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void CanExecuteRefreshCommonCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DebugViewModel.Instance.IsDebugEnabled;
        }

        /// <summary>
        /// The executed refresh common command.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void ExecutedRefreshCommonCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var which = e.Parameter as string;
            if (!string.IsNullOrEmpty(which))
            {
            }
            else
            {
                var nestedAttribute = e.Parameter as NestedAttribute;
                if (nestedAttribute == null)
                {
                    return;
                }

                which = nestedAttribute.Index;
            }

            if (string.IsNullOrEmpty(which))
            {
                return;
            }

            switch (which)
            {
                case "SECTION1":
                    DebugViewModel.Instance.RefreshCollection(DebugViewModel.Instance.Section1Collection);
                    break;
                case "SECTION2":
                    break;
                case "SECTION3":
                    break;
                default:
                    DebugViewModel.GetCommonVariable(which);
                    break;
            }
        }

        #endregion
    }
}