namespace SBXAThemeSupport.Views
{
    using System.Windows;
    using System.Windows.Controls;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for RevisionDefinition.xaml
    /// </summary>
    public partial class RevisionDefinition : UserControl
    {
        public RevisionDefinition()
        {
            this.InitializeComponent();
        }

        #region StartItem Property

        public static readonly DependencyProperty StartItemProperty =
            DependencyProperty.Register(
                "StartItem",
                typeof(TreeItem),
                typeof(RevisionDefinition),
                new PropertyMetadata(OnStartItemChanged)
                );

        /// <summary>
        ///     Gets or sets the StartItemProperty. This is a DependencyProperty.
        /// </summary>
        public TreeItem StartItem
        {
            get
            {
                return ((TreeItem)this.GetValue(StartItemProperty));
            }
            set
            {
                this.SetValue(StartItemProperty, value);
            }
        }

        private static void OnStartItemChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var revisionDefinition = target as RevisionDefinition;
            if (revisionDefinition != null)
            {
                var revisionDefinitionViewModel = revisionDefinition.DataContext as RevisionDefinitionViewModel;
                if (revisionDefinitionViewModel == null)
                {
                    return;
                }

                revisionDefinitionViewModel.CreateRevisionDefinition(args.NewValue as TreeItem);
            }
        }

        #endregion StartItem Property
    }
}