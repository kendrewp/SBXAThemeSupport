using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using SBXA.Shared;
using SBXAThemeSupport.DebugAssistant.ViewModels;
using SBXAThemeSupport.ViewModels;

namespace SBXAThemeSupport.Models
{
    public class NestedAttributeCollection : ObservableCollection<NestedAttribute>
    {
        public static NestedAttributeCollection BuildFromSBString(string variable, SBString sbString)
        {
            if (sbString.Dcount() == 1) sbString = sbString.Extract(1); // if only a single attribute, raise it so that the colleciton is built correctly.
            var nac = new NestedAttributeCollection {Variable = variable, Source = sbString};
            return nac;
        }

        private void UpdateCollection(SBString sbString)
        {
            Clear();
            var index = 0;
            if (sbString.Dcount() == 1)
            {
                    Insert(index, new NestedAttribute(DebugViewModel.BuildTitle(Variable, "1"), sbString, DebugViewModel.BuildTitle(Variable, "1")));
            }
            else
            {
                foreach (var attr in sbString)
                {
                    Insert(index, new NestedAttribute(DebugViewModel.BuildTitle(Variable, (index + 1).ToString(CultureInfo.InvariantCulture)), attr, DebugViewModel.BuildTitle(Variable, (index + 1).ToString(CultureInfo.InvariantCulture))));
                    index++;
                }
            }
        }
    

        private SBString _Source;

        public SBString Source
        {
            get { return _Source; }
            set
            {
                _Source = value;
                UpdateCollection(_Source);

            }
        }

        public bool ContainsIndex(string index)
        {
            return this.Any(item => item.Index.Equals(index));
        }

        public NestedAttribute GetItemWithIndex(string index)
        {
            return this.FirstOrDefault(item => item.Index.Equals(index));
        }

        #region Variable Property

        private string _Variable;

        /// <summary>
        /// Gets or sets the Variable property. This property will raise a <see cref="ViewModel.PropertyChanged"/> event.
        /// </summary>
        public string Variable
        {
            get { return _Variable; }
            set
            {
                if (_Variable == value) return;
                _Variable = value;
                base.OnPropertyChanged(new PropertyChangedEventArgs(Variable));
            }
        }

        #endregion Variable Property
    
    }

    public class NestedAttribute : ObservableEntity
    {
        private string _Index;
        private string _Data;
        private bool _IsNested;
        private SBString _Source;
        private string _Variable;

        public string Index
        {
            get { return _Index; }
            set
            {
                if (_Index == value) return;
                _Index = value;
                RaisePropertyChanged("Index");
            }
        }

        public string Data
        {
            get { return _Data; }
            set
            {
                if (Data == value) return;
                _Data = value;
                RaisePropertyChanged("Data");
            }
        }

        public bool IsNested
        {
            get { return _IsNested; }
            set
            {
                if (_IsNested == value) return;
                _IsNested = value;
                RaisePropertyChanged("IsNested");
            }
        }

        public SBString Source
        {
            get { return _Source; }
            set
            {
                if (_Source == value) return;
                _Source = value;

                Data = _Source.GetStandardString();
                if (_Source.Dcount() > 1) IsNested = true;

                RaisePropertyChanged("Source");
//                RaisePropertyChanged("IsNested");
                string oindex = Index;
                Index = "";
                Index = oindex;
 //               RaisePropertyChanged("Index");
            }
        }

        public string Variable
        {
            get { return _Variable; }
            set { _Variable = value; }
        }

        public NestedAttribute(string attributeNumber, SBString data, string variable)
        {
            Index = attributeNumber;
            Source = data;
            Variable = variable;
        }
    }
}
