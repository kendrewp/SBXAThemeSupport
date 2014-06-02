// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableStack.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// This stack implements INotifyCollectionChanged and INotifyPropertyChanged so that objects that are interested can
    ///     listen to change events.
    /// </summary>
    /// <typeparam name="T">
    /// The <see cref="Type"/> of objects that this collection will contain.
    /// </typeparam>
    public class ObservableStack<T> : Stack<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObservableStack{T}" /> class.
        /// </summary>
        public ObservableStack()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public ObservableStack(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                base.Push(item);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        public ObservableStack(List<T> list)
        {
            foreach (var item in list)
            {
                base.Push(item);
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Explicit Interface Events

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                this.PropertyChanged += value;
            }

            remove
            {
                this.PropertyChanged -= value;
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public new virtual void Clear()
        {
            base.Clear();
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///     Pops this instance.
        /// </summary>
        /// <returns>
        ///     The <see cref="T" />.
        /// </returns>
        public new virtual T Pop()
        {
            var item = this.Peek();

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, this.Count - 1));
            item = base.Pop();

            return item;
        }

        /// <summary>
        /// Pushes the specified item.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public new virtual void Push(T item)
        {
            base.Push(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.RaiseCollectionChanged(e);
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(e);
        }

        /// <summary>
        /// Raises the collection changed.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var onCollectionChanged = this.CollectionChanged;
            if (onCollectionChanged != null)
            {
                onCollectionChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="e">
        /// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            var onPropertyChanged = this.PropertyChanged;
            if (onPropertyChanged != null)
            {
                onPropertyChanged(this, e);
            }
        }

        #endregion
    }
}