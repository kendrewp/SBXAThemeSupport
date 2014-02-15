using System.Windows;
using SBXAThemeSupport.DebugAssistant;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.Views
{
    /// <summary>
    /// Interaction logic for SBStringViewerWindow.xaml
    /// </summary>
    public partial class SBStringViewerWindow
    {
        public SBStringViewerWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var collection = DataContext as NestedAttributeCollection;
            if (collection == null) return;

            DebugWindowManager.RemoveWindow(collection.Variable);
            base.OnClosing(e);
        }
    }
}
