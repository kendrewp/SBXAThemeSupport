// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UiViewModel.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.ViewModels
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Windows.Input;

    /// <summary>
    ///     The ui view model.
    /// </summary>
    public class UiViewModel : ViewModel
    {
        #region Static Fields

        private static readonly UiViewModel Instance = new UiViewModel();

        #endregion

        #region Fields

        private readonly Hashtable disabledKeys = new Hashtable();

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public static UiViewModel Current
        {
            get
            {
                return Instance;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The disabled key.
        /// </summary>
        /// <param name="disabledKey">
        /// The the key that will be used to maanage the disabled field.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        public void DisabledKey(string disabledKey, string fieldName)
        {
            var currentDisabledKeys = new StringCollection();

            if (this.disabledKeys.ContainsKey(fieldName))
            {
                currentDisabledKeys = this.disabledKeys[fieldName] as StringCollection;
                this.disabledKeys.Remove(fieldName);
            }

            if (currentDisabledKeys != null && !currentDisabledKeys.Contains(disabledKey))
            {
                currentDisabledKeys.Add(disabledKey);
            }

            this.disabledKeys.Add(fieldName, currentDisabledKeys);
        }

        /// <summary>
        /// The enabled key.
        /// </summary>
        /// <param name="disabledKey">
        /// The disabled key.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        public void EnabledKey(string disabledKey, string fieldName)
        {
            var currentDisabledKeys = new StringCollection();

            if (this.disabledKeys.ContainsKey(fieldName))
            {
                currentDisabledKeys = this.disabledKeys[fieldName] as StringCollection;
                this.disabledKeys.Remove(fieldName);
            }

            if (currentDisabledKeys != null && currentDisabledKeys.Contains(disabledKey))
            {
                currentDisabledKeys.Remove(disabledKey);
            }

            this.disabledKeys.Add(fieldName, currentDisabledKeys);
        }

        /// <summary>
        /// The is key disabled.
        /// </summary>
        /// <param name="keyEventArgs">
        /// The key event args.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsKeyDisabled(KeyEventArgs keyEventArgs, string fieldName)
        {
            if (this.disabledKeys.ContainsKey(fieldName))
            {
                var currentDisabledKeys = this.disabledKeys[fieldName] as StringCollection;
                if (currentDisabledKeys != null && currentDisabledKeys.Contains(keyEventArgs.Key.ToString()))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}