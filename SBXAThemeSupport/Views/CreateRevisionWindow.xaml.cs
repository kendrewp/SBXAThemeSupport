namespace SBXAThemeSupport.Views
{
    using System.Windows;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    /// Interaction logic for CreateRevisionWindow.xaml
    /// </summary>
    public partial class CreateRevisionWindow : Window
    {
        public CreateRevisionWindow()
        {
            InitializeComponent();
        }

        #region StartItem Property

        public static readonly DependencyProperty StartItemProperty =
            DependencyProperty.Register(
                "StartItem",
                typeof(TreeItem),
                typeof(CreateRevisionWindow));

        /// <summary>
        /// Gets or sets the StartItemProperty. This is a DependencyProperty.
        /// </summary>
        public TreeItem StartItem
        {
            get
            {
                return ((TreeItem)GetValue(StartItemProperty));
            }
            set
            {
                SetValue(StartItemProperty, value);
            }
        }

        #endregion StartItem Property


/*
        public void StartItem(TreeItem startItem)
        {
            var revisionDefinitionViewModel = DataContext as RevisionDefinitionViewModel;
            if (revisionDefinitionViewModel == null) return;

            revisionDefinitionViewModel.RevisionDefinitionItemCollection.Add(new RevisionDefinitionItem() {Action = "1", FileName = "fileName", Item = startItem.Name, Parameters = "params"});
        }
*/
    }
}
