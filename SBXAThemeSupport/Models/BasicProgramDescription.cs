namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    /// This class represents a basic program.
    /// </summary>
    public class BasicProgramDescription : DefinitionDescription
    {
        public string ObjectLocation { get; set; }
        public string ObjectFileLocation { get; set; }
        public bool Parsed { get; set; }

        public BasicProgramDescription(string fileName, string name)
            : base(fileName, name)
        {
        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            if (!IsError)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "IO", FileName = this.FileName, Item = this.Name, Parameters = RevisionDefinitionViewModel.Data });
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "FB", FileName = this.FileName, Item = string.Empty, Parameters = string.Empty });
            }
            base.AddChildrenToCollection(collection);
        }

    }
}