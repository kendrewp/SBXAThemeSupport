// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessAnalysis.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for ProcessAnalysis.xaml
    /// </summary>
    public partial class ProcessAnalysis : UserControl
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessAnalysis" /> class.
        /// </summary>
        public ProcessAnalysis()
        {
            this.InitializeComponent();
            CreateRevisionDefinitionCommand = new RelayCommand(
                this.ExecutedCreateRevisionDefinitionCommand, 
                this.CanExecuteCreateRevisionDefinitionCommand);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the create revision definition command.
        /// </summary>
        public static ICommand CreateRevisionDefinitionCommand { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// The can execute create revision definition command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CanExecuteCreateRevisionDefinitionCommand(object parameter)
        {
            return DebugViewModel.Instance.IsConnected;
        }

        /// <summary>
        /// The executed create revision definition command.
        /// </summary>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        private void ExecutedCreateRevisionDefinitionCommand(object parameter)
        {
            var createRevisionWindow = new CreateRevisionWindow();
            createRevisionWindow.StartItem = parameter as TreeItem;
            createRevisionWindow.Owner = DebugWindowManager.DebugConsoleWindow;
            createRevisionWindow.Show();
        }

        /// <summary>
        /// The handle loading text on loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void HandleLoadingTextOnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var blinkAnimation = this.TryFindResource("FlashingText") as Storyboard;
                if (blinkAnimation != null)
                {
                    blinkAnimation.Begin();
                }
            }
            catch (Exception exception)
            {
            }
        }

        #endregion
    }
}