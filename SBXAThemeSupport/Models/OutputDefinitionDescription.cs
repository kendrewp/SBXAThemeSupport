namespace SBXAThemeSupport.Models
{
    using SBXA.Shared;

    /// <summary>
    /// This class represents a output process.
    /// </summary>
    public class OutputDefinitionDescription : ScreenProcessDescription
    {

        public OutputDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, expression, definition)
        {
        }
    }
}