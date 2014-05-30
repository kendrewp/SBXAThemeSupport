using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBXAThemeSupport.Models
{
    public class ScreenFieldDefinition : FieldDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenFieldDefinition"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public ScreenFieldDefinition(string fileName, string name)
            : base(fileName, name)
        {
        }

    }
}
