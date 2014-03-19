// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SBStringViewerWindow.xaml.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="SBStringViewerWindow.xaml.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Views
{
    using System.ComponentModel;

    using SBXAThemeSupport.DebugAssistant;
    using SBXAThemeSupport.Models;

    /// <summary>
    /// Interaction logic for SBStringViewerWindow.xaml
    /// </summary>
    public partial class SBStringViewerWindow
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SBStringViewerWindow" /> class.
        /// </summary>
        public SBStringViewerWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on closing.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnClosing(CancelEventArgs e)
        {
            var collection = this.DataContext as NestedAttributeCollection;
            if (collection == null)
            {
                return;
            }

            DebugWindowManager.RemoveWindow(collection.Variable);
            base.OnClosing(e);
        }

        #endregion
    }
}