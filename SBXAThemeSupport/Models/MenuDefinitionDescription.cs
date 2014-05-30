// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuDefinitionDescription.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Models
{
    using System;

    using SBXA.Shared;

    using SBXAThemeSupport.DebugAssistant.ViewModels;

    /// <summary>
    ///     The menu definition description.
    /// </summary>
    public class MenuDefinitionDescription : DefinitionDescription
    {
        #region Constants

        private const int MenuOptionType = 8;

        private const int MenuType = 2;

        private const int Options = 6;

        private const int ProcessName = 7;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDefinitionDescription"/> class.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="definition">
        /// The definition.
        /// </param>
        public MenuDefinitionDescription(string fileName, string name, string expression, SBString definition)
            : base(fileName, name, SourceDefinition.Definition, expression)
        {
            this.ParseDefinition(definition);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add children to collection.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        public override void AddChildrenToCollection(RevisionDefinitionItemCollection collection)
        {
            if (!this.IsError)
            {
                RevisionDefinitionViewModel.AddItemToDefinition(
                    collection, 
                    new RevisionDefinitionItem()
                        {
                            Action = "IO", 
                            FileName = this.FileName, 
                            Item = this.Name, 
                            Parameters = RevisionDefinitionViewModel.Data
                        });
            }

            base.AddChildrenToCollection(collection);
        }

        #endregion

        #region Methods

        private void ParseDefinition(SBString definition)
        {
            /*
            Type                                               2
            1: vertical box
            2: horiz boxtype.
            3: user painted
            4: as 3 + select no       
            5: horiz box with linked sub-menus (for GUI)
             * 
             * Option Description                                 6.M     C
             * Name                                               7.M     D
             * Type (M/P/T/>/H/C/X)                               8.M     D

             * 
             * Process before menu display                       13.1
             * Process after display                             13.2
             * Process after select option                       13.3
             * Process after execution of option                 13.4
             * Process after escaping this menu                  13.5
             * Process before returning to SYSMENU or LOGIN      13.6
             * Process to determine box cords                    13.7
             * Subroutine to call at start                       13.8

            */
            switch (definition.Extract(MenuType).Value)
            {
                case "1":
                    this.ProcessMenuOptions(definition);
                    break;
                case "2":
                    this.ProcessMenuOptions(definition);
                    break;
                case "3":
                    break;
                case "4":
                    break;
                case "5":
                    break;
            }
        }

        private void ProcessMenuOptions(SBString definition)
        {
            try
            {
                var noOptions = definition.Extract(MenuOptionType).Dcount();
                for (int optNo = 1; optNo <= noOptions; optNo++)
                {
                    var desc = definition.Extract(Options, optNo).Value;
                    var type = definition.Extract(MenuOptionType, optNo).Value;
                    var process = definition.Extract(ProcessName, optNo).Value;
                    if (type.Equals("P") && !string.IsNullOrEmpty(process))
                    {
                        DebugViewModel.Instance.ProcessAnalysisViewModel.LoadProcessFromExpression(
                            SourceDefinition.Menu, 
                            SourceDefinition.Process, 
                            process, 
                            this, 
                            desc);
                    }
                }
            }
            catch (Exception exception)
            {
                this.IsError = true;
                CustomLogger.LogException(exception, "There was a problem process the menu '" + this.Name + "'");
            }
        }

        #endregion
    }
}