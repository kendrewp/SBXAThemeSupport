using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using SBXA.Shared;

namespace SBXAThemeSupport.Models
{
    public class NestedAttributeCollection : ObservableCollection<NestedAttribute>
    {
        public static NestedAttributeCollection BuildFromSBString(SBString sbString)
        {
            var nac = new NestedAttributeCollection();
            var index = 0;
            foreach (var attr in sbString)
            {
                nac.Insert(index, new NestedAttribute((index + 1).ToString(CultureInfo.InvariantCulture), attr));
                index++;
            }
            return nac;
        }

        public bool ContainsIndex(string index)
        {
            return this.Any(item => item.Index.Equals(index));
        }

        public NestedAttribute GetItemWithIndex(string index)
        {
            return this.FirstOrDefault(item => item.Index.Equals(index));
        }
    }

    public class NestedAttribute : ObservableEntity
    {
        private string _Index;
        private string _Data;
        private bool _IsNested;
        private SBString _Source;

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

        public NestedAttribute(string attributeNumber, SBString data)
        {
            Index = attributeNumber;
            Source = data;
        }
    }
}
