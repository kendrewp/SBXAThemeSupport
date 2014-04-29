using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using SBXAThemeSupport.DebugAssistant.ViewModels;

namespace SBXAThemeSupport.Views
{
    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.Models;

    /// <summary>
    /// Interaction logic for ProcessAnalysis.xaml
    /// </summary>
    public partial class ProcessAnalysis : UserControl
    {
        public static ICommand CreateRevisionDefinitionCommand { get; private set; }

        public ProcessAnalysis()
        {
            this.InitializeComponent();
            CreateRevisionDefinitionCommand = new RelayCommand(this.ExecutedCreateRevisionDefinitionCommand, this.CanExecuteCreateRevisionDefinitionCommand);

        }

        private void HandleLoadingTextOnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Storyboard blinkAnimation = this.TryFindResource("FlashingText") as Storyboard;
                if (blinkAnimation != null)
                {
                    blinkAnimation.Begin();
                }
            }
            catch (Exception exception)
            {
            }
        }

        private bool CanExecuteCreateRevisionDefinitionCommand(object parameter)
        {
            return DebugViewModel.Instance.IsConnected;
        }

        private void ExecutedCreateRevisionDefinitionCommand(object parameter)
        {
            var createRevisionWindow = new CreateRevisionWindow();
            createRevisionWindow.StartItem = parameter as TreeItem;
            createRevisionWindow.Owner = DebugWindowManager.DebugConsoleWindow;
            createRevisionWindow.Show();
        }
    }
}
