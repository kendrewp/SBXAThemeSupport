namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class contains all the info required to display a File Update definition.
    /// </summary>
    public class FileUpdateDefinitionDescription : DefinitionDescription
    {
        public FileUpdateDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression)
        {
        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            if (!this.IsError)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "IO", FileName = this.FileName, Item = this.Name, Parameters = RevisionDefinitionViewModel.Data });
            }
            base.AddChildrenToCollection(collection);
        }
    }
}