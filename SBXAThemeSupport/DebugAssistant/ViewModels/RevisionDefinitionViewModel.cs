namespace SBXAThemeSupport.DebugAssistant.ViewModels
{
    using System;
    using System.Windows.Forms.VisualStyles;

    using SBXAThemeSupport.Models;

    using Xceed.Wpf.DataGrid;

    public class RevisionDefinitionViewModel
    {
        public const string Dict = "1";

        public const string Data = "3";

        public const string DictAndData = "2";

        private readonly RevisionDefinitionItemCollection revisionDefinitionItemCollection = new RevisionDefinitionItemCollection();
        
        public RevisionDefinitionItemCollection RevisionDefinitionItemCollection
        {
            get
            {
                return revisionDefinitionItemCollection;
            }
        }

        public void CreateRevisionDefinition(TreeItem startItem)
        {
            startItem.AddChildrenToCollection(RevisionDefinitionItemCollection);
        }

        internal static void AddItemToDefinition(RevisionDefinitionItemCollection collection, RevisionDefinitionItem item)
        {
            if (string.IsNullOrEmpty(item.FileName))
            {
                // do not add items that do not have a file name.
                return;
            }
            if (item.Action.Equals("IO") && (string.IsNullOrEmpty(item.FileName) || string.IsNullOrEmpty(item.Item)))
            {
                // do not add items that do not have a file name and item name if the action is IO.
                return;
            }
            if (item.FileName.Equals("DM") || item.FileName.Equals("DMUT") || item.FileName.Equals("DMSH") || item.FileName.Equals("DMGC") || item.FileName.Equals("DMGD") || item.FileName.Equals("DMGC"))
            {
                // do not include SB/XA program files.
                return;
            }
            if (!collection.ContainsItem(item.FileName, item.Item))
            {
                collection.Add(item);
            }
        }

    }
}
