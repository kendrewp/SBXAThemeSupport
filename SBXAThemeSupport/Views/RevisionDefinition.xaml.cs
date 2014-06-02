// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevisionDefinition.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
        #region Static Fields

        /// <summary>
        /// The start item property.
        /// </summary>
        public static readonly DependencyProperty StartItemProperty = DependencyProperty.Register(
            "StartItem", 
            typeof(TreeItem), 
            typeof(RevisionDefinition), 
            new PropertyMetadata(OnStartItemChanged));

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RevisionDefinition" /> class.
        /// </summary>
        public RevisionDefinition()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the StartItemProperty. This is a DependencyProperty.
        /// </summary>
        public TreeItem StartItem
        {
            get
            {
                return (TreeItem)this.GetValue(StartItemProperty);
            }

            set
            {
                this.SetValue(StartItemProperty, value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on start item changed.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
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

        #endregion
    }
}