// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStack.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ProcessStack.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="ProcessStack.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SBXAThemeSupport.DebugAssistant.Models
{
    using SBXAThemeSupport.Models;

    /// <summary>
    /// This class if a specialized version of ObservableStack/>.
    /// </summary>
    public class ProcessStack : ObservableStack<ProcessDescription>
    {
        /// <summary>
        /// Clears this instance.
        /// </summary>
        public override void Clear()
        {
            foreach (var processDescription in this)
            {
                processDescription.Clear();
            }

            base.Clear();
        }
    }

    /// <summary>
    /// This class contains the description of the process.
    /// </summary>
    public class ProcessDescription : ObservableEntity
    {
        private bool isCurrent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessDescription"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ProcessDescription(string name)
        {
            this.Name = name;
            this.Children = new ProcessStack();
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public ProcessStack Children { get; private set; }

        /// <summary>
        /// Gets or sets the history process description.
        /// </summary>
        /// <value>
        /// The history process description.
        /// </value>
        public ProcessDescription HistoryProcessDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is current].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is current]; otherwise, <c>false</c>.
        /// </value>
        public bool IsCurrent
        {
            get
            {
                return this.isCurrent;
            }

            set
            {
                this.isCurrent = value;
                this.RaisePropertyChanged("IsCurrent");
            }
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            foreach (var item in this.Children)
            {
                item.Clear();
            }

            this.Children.Clear();
        }

        /// <summary>
        /// Clears the history references.
        /// </summary>
        public void ClearHistoryReferences()
        {
            this.HistoryProcessDescription = null;
            foreach (var item in this.Children)
            {
                item.ClearHistoryReferences();
            }
        }
    }
}
