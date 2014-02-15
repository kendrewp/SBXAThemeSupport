using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SBXAThemeSupport.DebugAssistant.ViewModels;

namespace SBXAThemeSupport.Views
{
    /// <summary>
    /// Interaction logic for ProcessStack.xaml
    /// </summary>
    public partial class ProcessStack : UserControl
    {
        public static readonly RoutedUICommand ClearStackCommand = new RoutedUICommand("ClearStackCommand", "ClearStackCommand", typeof(ProcessStack));
        public static CommandBinding ClearStackCommandBinding = new CommandBinding(ClearStackCommand);

        static ProcessStack()
        {
            ClearStackCommandBinding.Executed += ExecutedClearStackCommand;
        }

        public ProcessStack()
        {
            InitializeComponent();

            CommandBindings.Add(ClearStackCommandBinding);
        }

        static void ExecutedClearStackCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DebugViewModel.Instance.ProcessHistoryStack.Clear();
        }

    }
}
