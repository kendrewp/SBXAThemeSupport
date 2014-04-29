﻿using SBXA.Shared;

namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    /// This class represents a paragraph.
    /// </summary>
    public class ParagraphDescription : DefinitionDescription
    {
        public ParagraphDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression)
        {
        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "IO", FileName = this.FileName, Item = this.Name, Parameters = RevisionDefinitionViewModel.Data});
            base.AddChildrenToCollection(collection);
        }

    }
}