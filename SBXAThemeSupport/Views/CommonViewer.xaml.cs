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
using SBXA.Shared;
using SBXAThemeSupport.DebugAssistant.ViewModels;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.Views
{
    /// <summary>
    /// Interaction logic for CommonViewer.xaml
    /// </summary>
    public partial class CommonViewer : UserControl
    {
        public static readonly RoutedUICommand RefreshCommonCommand = new RoutedUICommand("RefreshCommonCommand", "RefreshCommonCommand", typeof(CommonViewer));
        public static CommandBinding RefreshCommonCommandBinding = new CommandBinding(RefreshCommonCommand);

        static CommonViewer()
        {
            RefreshCommonCommandBinding.Executed += ExecutedRefreshCommonCommand;
            RefreshCommonCommandBinding.CanExecute += CanExecuteRefreshCommonCommand;

            
        }

        static void CanExecuteRefreshCommonCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DebugViewModel.Instance.IsDebugEnabled;
        }


        static void ExecutedRefreshCommonCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var which = e.Parameter as string;
            if (!string.IsNullOrEmpty(which))
            {
            }
            else
            {
                var nestedAttribute = e.Parameter as NestedAttribute;
                if (nestedAttribute == null) return;

                which = nestedAttribute.Index;
            }
            if (string.IsNullOrEmpty(which)) return;

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

        public CommonViewer()
        {
            InitializeComponent();

            CommandBindings.Add(RefreshCommonCommandBinding);
        }

    }
}
