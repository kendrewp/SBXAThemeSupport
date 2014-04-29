using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBXAThemeSupport.Models
{
    using System.Collections.ObjectModel;

    public class RevisionDefinitionItem
    {
        public string Action { get; set; }
        public string FileName { get; set; }
        public string Item { get; set; }
        public string Parameters { get; set; }
    }

    public class RevisionDefinitionItemCollection : ObservableCollection<RevisionDefinitionItem>
    {
        public bool ContainsItem(string fileName, string itemName)
        {
            foreach (var item in this)
            {
                if (item.FileName.Equals(fileName) && item.Item.Equals(itemName))
                {
                    return true;
                }
            }
            return false;
        }
    }

}
