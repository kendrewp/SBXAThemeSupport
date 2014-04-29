namespace SBXAThemeSupport.Models
{
    using System.Diagnostics;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     This class represents a basic program.
    /// </summary>
    public class BasicProgramDescription : DefinitionDescription
    {
        public BasicProgramDescription(string fileName, string name)
            : base(fileName, name)
        {
            Debug.WriteLine("[BasicProgramDescription.BasicProgramDescription(17)] " + fileName + ", " + name);
        }

        public string ObjectFileLocation { get; set; }

        public string ObjectLocation { get; set; }

        public bool Parsed { get; set; }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            if (!this.IsError)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "IO", FileName = this.FileName, Item = this.Name, Parameters = RevisionDefinitionViewModel.Data });
                RevisionDefinitionViewModel.AddItemToDefinition(collection, new RevisionDefinitionItem() { Action = "FB", FileName = this.FileName, Item = string.Empty, Parameters = string.Empty });
            }
            base.AddChildrenToCollection(collection);
        }
    }
}