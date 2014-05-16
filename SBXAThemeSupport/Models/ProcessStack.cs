// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStack.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    ///     The process stack.
    /// </summary>
    public class ProcessStack : ObservableCollection<DefinitionDescription>, IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Finalizes an instance of the <see cref="ProcessStack" /> class.
        /// </summary>
        ~ProcessStack()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The clear.
        /// </summary>
        public new void Clear()
        {
            foreach (var processDescription in this)
            {
                processDescription.Clear();
            }

            base.Clear();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Returns the last object on the stack.
        /// </summary>
        /// <returns>The instance of <see cref="DefinitionDescription" /> that was added to the stack last.</returns>
        public DefinitionDescription Peek()
        {
            var obj = this[this.Count - 1];
            return obj;
        }

        /// <summary>
        ///     Pops the last item on the stack.
        /// </summary>
        /// <returns>The instance of <see cref="DefinitionDescription" /> that was popped.</returns>
        public DefinitionDescription Pop()
        {
            var obj = this[this.Count - 1];
            this.Remove(obj);
            return obj;
        }

        /// <summary>
        /// Pushes the specified object on the stack.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        public void Push(DefinitionDescription obj)
        {
            this.Insert(this.Count, obj);
        }

        #endregion

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 

        // The bulk of the clean-up code is implemented in Dispose(bool)
        #region Methods

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                foreach (var item in this)
                {
                    if (item is IDisposable)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
            }
        }

        #endregion
    }
}