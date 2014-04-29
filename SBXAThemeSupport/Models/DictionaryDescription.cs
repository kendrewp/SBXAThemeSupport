namespace SBXAThemeSupport.Models
{
    public class DictionaryDescription : DefinitionDescription
    {
        public DictionaryDescription(string fileName)
            : base(fileName, fileName)
        {
            
        }

        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            base.AddChildrenToCollection(collection);
            // There is not dictionary only the fields, etc. in the dict so do not add it the name of the dictionary.

            // now add all the fields.

        }
    }
}