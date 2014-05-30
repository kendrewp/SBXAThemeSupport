﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class is the basis for SB/XA definitions, definition, field, etc.
    /// </summary>
    public class DefinitionDescription : TreeItem
    {
        #region Fields

        private DateTime endTime;

        private bool isCurrent;

        private int serverEndMilliseconds;

        private int serverStartMilliseconds;

        private DateTime startTime;

        private readonly ObservableCollection<SBExpression> screenExpressions = new ObservableCollection<SBExpression>();

        #endregion

        #region Constructors and Destructors

        public DefinitionDescription()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public DefinitionDescription(string fileName, string name)
            : base(name)
        {
            this.ChildProcesses = new ProcessStack();
            this.ProcessCollection = new ObservableCollection<ProcessCall>();
            this.DictionaryExpressions = new ObservableCollection<SBExpression>();
            this.Expressions = new ObservableCollection<SBExpression>();

            this.FileName = fileName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="hookType">The type of description, process or expression</param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        public DefinitionDescription(string fileName, string name, SourceDefinition hookType, string expression)
            : this(fileName, name)
        {
            this.HookType = hookType;

            if (!expression.Equals(name))
            {
                this.SourceExpression = expression;
            }
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        ///     Gets or sets the hook type.
        /// </summary>
        public SourceDefinition HookType { get; set; }


        /// <summary>
        ///     Gets the child processes.
        /// </summary>
        public ProcessStack ChildProcesses { get; private set; }

        /// <summary>
        ///     Gets or sets the dictionary expressions.
        /// </summary>
        public ObservableCollection<SBExpression> DictionaryExpressions { get; set; }

        /// <summary>
        ///     Gets or sets the end time.
        /// </summary>
        /// <value>
        ///     The end time.
        /// </value>
        public DateTime EndTime
        {
            get
            {
                return this.endTime;
            }

            set
            {
                this.endTime = value;
                this.RaisePropertyChanged("EndTime");
            }
        }

        /// <summary>
        ///     Gets or sets the expressions.
        /// </summary>
        public ObservableCollection<SBExpression> Expressions { get; set; }

        /// <summary>
        ///     Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Gets or sets the history process description.
        /// </summary>
        public DefinitionDescription HistoryProcessDescription { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is current.
        /// </summary>
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
        ///     Gets or sets a value indicating whether is error.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        ///     Gets the process collection.
        /// </summary>
        public virtual ObservableCollection<ProcessCall> ProcessCollection { get; private set; }

        /// <summary>
        ///     Gets or sets the screen expressions.
        /// </summary>
        public virtual ObservableCollection<SBExpression> ScreenExpressions
        {
            get
            {
                return this.screenExpressions;
            }
        }

        /// <summary>
        ///     Gets or sets the server end milliseconds.
        /// </summary>
        /// <value>
        ///     The server end milliseconds.
        /// </value>
        public int ServerEndMilliseconds
        {
            get
            {
                return this.serverEndMilliseconds;
            }

            set
            {
                this.serverEndMilliseconds = value;
                this.RaisePropertyChanged("ServerEndMilliseconds");
            }
        }

        /// <summary>
        ///     Gets or sets the server start milliseconds.
        /// </summary>
        /// <value>
        ///     The server start milliseconds.
        /// </value>
        public int ServerStartMilliseconds
        {
            get
            {
                return this.serverStartMilliseconds;
            }

            set
            {
                this.serverStartMilliseconds = value;
                this.RaisePropertyChanged("ServerStartMilliseconds");
            }
        }

        /// <summary>
        ///     Gets the source expression.
        /// </summary>
        public virtual string SourceExpression { get; protected set; }

        /// <summary>
        ///     Gets or sets the start time.
        /// </summary>
        /// <value>
        ///     The start time.
        /// </value>
        public DateTime StartTime
        {
            get
            {
                return this.startTime;
            }

            set
            {
                this.startTime = value;
                this.RaisePropertyChanged("StartTime");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            foreach (var item in this.ProcessCollection)
            {
                item.ProcessDescription.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in this.DictionaryExpressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in this.ScreenExpressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            foreach (var sbExpression in this.Expressions)
            {
                sbExpression.AddChildrenToCollection(collection);
            }

            base.AddChildrenToCollection(collection);

            this.AddSelf(collection);
        }

        /// <summary>
        ///     The clear.
        /// </summary>
        public void Clear()
        {
            foreach (var item in this.ChildProcesses)
            {
                item.Clear();
            }

            this.ChildProcesses.Clear();
        }

        /// <summary>
        ///     The clear history references.
        /// </summary>
        public void ClearHistoryReferences()
        {
            this.HistoryProcessDescription = null;
            foreach (var item in this.ChildProcesses)
            {
                item.ClearHistoryReferences();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add self.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        protected virtual void AddSelf(RevisionDefinitionItemCollection collection)
        {
            if (!this.IsError)
            {
                // do this last so it will only happen if it has not already happened.
                var parameters = RevisionDefinitionViewModel.Data;
                var fname = this.FileName;
                if (this.FileName.StartsWith("DICT "))
                {
                    parameters = RevisionDefinitionViewModel.Dict;
                    fname = this.FileName.Substring(5);
                }

                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection, 
                    new RevisionDefinitionItem { Action = "IO", FileName = fname, Item = this.Name, Parameters = parameters });
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                foreach (var item in this.ChildProcesses)
                {
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }

                foreach (var item in this.ProcessCollection)
                {
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    ///     The process collection.
    /// </summary>
    public class ProcessCollection : Collection<DefinitionDescription>, IDisposable
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Finalizes an instance of the <see cref="ProcessCollection" /> class.
        /// </summary>
        ~ProcessCollection()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        #endregion

        #region Public Indexers

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="DefinitionDescription"/>.
        /// </returns>
        public DefinitionDescription this[string key]
        {
            get
            {
                return this.FirstOrDefault(processDescription => processDescription.Name.Equals(key));
            }
        }

        public DefinitionDescription this[string fileName, string itemName, Type type]
        {
            get
            {
                return this.FirstOrDefault(processDescription => (processDescription.Name.Equals(itemName) && processDescription.FileName.Equals(fileName)
                     && processDescription.GetType() == type));
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The contains key.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsKey(string fileName, string processName)
        {
            return
                this.Any(
                    processDescription => (processDescription.FileName.Equals(fileName) && processDescription.Name.Equals(processName)));
        }

        /// <summary>
        /// The contains key.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="processName">
        /// The process name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsKey(string fileName, string processName, Type type)
        {
            if (type == typeof(BasicProgramDescription))
            {
                // do not check against the file name as 1) it may not be there yet and 2) it does not matter as there can only be one program with the name cataloged (at least locally).
                return this.Any(processDescription => (processDescription.Name.Equals(processName) && processDescription.GetType() == type));
            }

            return
                this.Any(
                    processDescription =>
                    (processDescription.Name.Equals(processName) && processDescription.FileName.Equals(fileName)
                     && processDescription.GetType() == type));
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
                    if (item != null)
                    {
                        ((IDisposable)item).Dispose();
                    }
                }

                this.Clear();
            }
        }

        #endregion
    }
}