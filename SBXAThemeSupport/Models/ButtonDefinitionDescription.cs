namespace SBXAThemeSupport.Models
{
    using SBXAThemeSupport.DebugAssistant.ViewModels;

    public class ButtonDefinitionDescription : DefinitionDescription
    {

        public ButtonDefinitionDescription(string fileName, string name, SourceDefinition hookType, string expression)
            : base(fileName, name, hookType, expression)
        {
        }


    }
}
