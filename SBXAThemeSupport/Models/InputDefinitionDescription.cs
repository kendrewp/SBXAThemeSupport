namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    /// <summary>
    /// This class represents a intput definition.
    /// </summary>
    public class InputDefinitionDescription : ScreenProcessDescription
    {

        public InputDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression, definition)
        {
        }

    }
}