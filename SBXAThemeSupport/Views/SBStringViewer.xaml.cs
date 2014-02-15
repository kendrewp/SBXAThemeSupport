using System;
using System.Windows.Input;
using SBXA.Shared;
using SBXA.UI.Client;
using SBXAThemeSupport.DebugAssistant;
using SBXAThemeSupport.Models;
using SBXAThemeSupport.DebugAssistant.ViewModels;

namespace SBXAThemeSupport.Views
{
    /// <summary>
    /// Interaction logic for SBStringViewer.xaml
    /// </summary>
    public partial class SBStringViewer
    {
        public static readonly RoutedUICommand DrillDownCommand = new RoutedUICommand("DrillDownCommand", "DrillDownCommand", typeof(SBStringViewer));
        public static CommandBinding DrillDownCommandBinding = new CommandBinding(DrillDownCommand);
        public static CommandBinding RefreshCommonCommandBinding = new CommandBinding(CommonViewer.RefreshCommonCommand);

        static SBStringViewer()
        {
            DrillDownCommandBinding.Executed += ExecutedDrillDownCommand;
            RefreshCommonCommandBinding.Executed += ExecutedRefreshCommonCommand;
            RefreshCommonCommandBinding.CanExecute += CanExecuteRefreshCommonCommand;
        }

        static void CanExecuteRefreshCommonCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DebugViewModel.Instance.IsDebugEnabled;
        }


        private static void ExecutedRefreshCommonCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var which = e.Parameter as string;
            if (which == null)
            {
                var attr = e.Parameter as NestedAttribute;
                if (attr == null) return;
                which = attr.Variable;
            }
            if (string.IsNullOrEmpty(which)) return;
            DebugViewModel.GetCommonVariable(which);
            

        }

        static void ExecutedDrillDownCommand(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {

                var item = e.Parameter as NestedAttribute;
                if (item == null) return;


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

        public SBStringViewer()
        {
            InitializeComponent();
            CommandBindings.Add(DrillDownCommandBinding);
            CommandBindings.Add(RefreshCommonCommandBinding);
        }
    }
}
