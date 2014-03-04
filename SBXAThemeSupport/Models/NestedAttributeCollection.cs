// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedAttributeCollection.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;
    using SBXAThemeSupport.ViewModels;

    /// <summary>
    ///     The nested attribute collection.
    /// </summary>
    public class NestedAttributeCollection : ObservableCollection<NestedAttribute>
    {
        #region Fields

        private SBString source;

        private string variable;

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the source.
        /// </summary>
        public SBString Source
        {
            get
            {
                return this.source;
            }

            set
            {
                this.source = value;
                this.UpdateCollection(this.source);
            }
        }

        /// <summary>
        ///     Gets or sets the Variable property. This property will raise a <see cref="ViewModel.PropertyChanged" /> event.
        /// </summary>
        public string Variable
        {
            get
            {
                return this.variable;
            }

            set
            {
                if (this.variable == value)
                {
                    return;
                }

                this.variable = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(this.Variable));
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The build from sb string.
        /// </summary>
        /// <param name="variable">
        /// The variable.
        /// </param>
        /// <param name="sbString">
        /// The sb string.
        /// </param>
        /// <returns>
        /// The <see cref="NestedAttributeCollection"/>.
        /// </returns>
        public static NestedAttributeCollection BuildFromSBString(string variable, SBString sbString)
        {
            if (sbString.Dcount() == 1)
            {
                sbString = sbString.Extract(1); // if only a single attribute, raise it so that the colleciton is built correctly.
            }

            var nac = new NestedAttributeCollection { Variable = variable, Source = sbString };
            return nac;
        }

        /// <summary>
        /// The contains index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ContainsIndex(string index)
        {
            return this.Any(item => item.Index.Equals(index));
        }

        /// <summary>
        /// The get item with index.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="NestedAttribute"/>.
        /// </returns>
        public NestedAttribute GetItemWithIndex(string index)
        {
            return this.FirstOrDefault(item => item.Index.Equals(index));
        }

        #endregion

        #region Methods

        private void UpdateCollection(SBString sbString)
        {
            this.Clear();
            var index = 0;
            if (sbString.Dcount() == 1)
            {
                this.Insert(
                    index, 
                    new NestedAttribute(
                        DebugViewModel.BuildTitle(this.Variable, "1"), 
                        sbString, 
                        DebugViewModel.BuildTitle(this.Variable, "1")));
            }
            else
            {
                foreach (var attr in sbString)
                {
                    this.Insert(
                        index, 
                        new NestedAttribute(
                            DebugViewModel.BuildTitle(this.Variable, (index + 1).ToString(CultureInfo.InvariantCulture)), 
                            attr, 
                            DebugViewModel.BuildTitle(this.Variable, (index + 1).ToString(CultureInfo.InvariantCulture))));
                    index++;
                }
            }
        }

        #endregion
    }

    /// <summary>
    ///     The nested attribute.
    /// </summary>
    public class NestedAttribute : ObservableEntity
    {
        #region Fields

        private string data;

        private string index;

        private bool isNested;

        private SBString source;

        private string variable;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedAttribute"/> class.
        /// </summary>
        /// <param name="attributeNumber">
        /// The attribute number.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="variable">
        /// The variable.
        /// </param>
        public NestedAttribute(string attributeNumber, SBString data, string variable)
        {
            this.Index = attributeNumber;
            this.Source = data;
            this.Variable = variable;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the data.
        /// </summary>
        public string Data
        {
            get
            {
                return this.data;
            }

            set
            {
                if (this.Data == value)
                {
                    return;
                }

                this.data = value;
                this.RaisePropertyChanged("Data");
            }
        }

        /// <summary>
        ///     Gets or sets the index.
        /// </summary>
        public string Index
        {
            get
            {
                return this.index;
            }

            set
            {
                if (this.index == value)
                {
                    return;
                }

                this.index = value;
                this.RaisePropertyChanged("Index");
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether is nested.
        /// </summary>
        public bool IsNested
        {
            get
            {
                return this.isNested;
            }

            set
            {
                if (this.isNested == value)
                {
                    return;
                }

                this.isNested = value;
                this.RaisePropertyChanged("IsNested");
            }
        }

        /// <summary>
        ///     Gets or sets the source.
        /// </summary>
        public SBString Source
        {
            get
            {
                return this.source;
            }

            set
            {
                if (this.source == value)
                {
                    return;
                }

                this.source = value;

                this.Data = this.source.GetStandardString();
                if (this.source.Dcount() > 1)
                {
                    this.IsNested = true;
                }

                this.RaisePropertyChanged("Source");
                //                RaisePropertyChanged("IsNested");
                string oindex = this.Index;
                this.Index = string.Empty;
                this.Index = oindex;
                //               RaisePropertyChanged("Index");
            }
        }

        /// <summary>
        ///     Gets or sets the variable.
        /// </summary>
        public string Variable
        {
            get
            {
                return this.variable;
            }

            set
            {
                this.variable = value;
            }
        }

        #endregion
    }
}