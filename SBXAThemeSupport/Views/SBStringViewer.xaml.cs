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
using SBXAThemeSupport.DebugAssistant;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.Views
{
    /// <summary>
    /// Interaction logic for SBStringViewer.xaml
    /// </summary>
    public partial class SBStringViewer : UserControl
    {
        public static readonly RoutedUICommand DrillDownCommand = new RoutedUICommand("DrillDownCommand", "DrillDownCommand", typeof(SBStringViewer));
        public static CommandBinding DrillDownCommandBinding = new CommandBinding(DrillDownCommand);

        static SBStringViewer()
        {
            DrillDownCommandBinding.Executed += ExecutedDrillDownCommand;
        }

        static void ExecutedDrillDownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var item = e.Parameter as NestedAttribute;
            if (item == null) return;

            ShowSBString(item.Source);
        }

        public SBStringViewer()
        {
            InitializeComponent();
            CommandBindings.Add(DrillDownCommandBinding);
        }

        private static void ShowSBString(SBString data)
        {
            var sbStringViewWindow = new SBStringViewerWindow();
            sbStringViewWindow.DataContext = NestedAttributeCollection.BuildFromSBString(data);
            sbStringViewWindow.Owner = DebugWindowManager.DebugConsoleWindow;
            sbStringViewWindow.Show();
        }
    }
}
