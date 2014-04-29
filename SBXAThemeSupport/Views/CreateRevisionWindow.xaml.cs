// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateRevisionWindow.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.Views
{
    using System.Windows;

    using SBXAThemeSupport.Models;

    /// <summary>
    ///     Interaction logic for CreateRevisionWindow.xaml
    /// </summary>
    public partial class CreateRevisionWindow : Window
    {
        #region Static Fields

        public static readonly DependencyProperty StartItemProperty = DependencyProperty.Register(
            "StartItem",
            typeof(TreeItem),
            typeof(CreateRevisionWindow));

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateRevisionWindow" /> class.
        /// </summary>
        public CreateRevisionWindow()
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