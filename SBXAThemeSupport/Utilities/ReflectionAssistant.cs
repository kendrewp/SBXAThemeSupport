// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionAssistant.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="ReflectionAssistant.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport.Utilities
{
    using System;
    using System.Reflection;

    /// <summary>
    ///     The reflection assistant.
    /// </summary>
    public class ReflectionAssistant
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get member info.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="methodName">
        /// The method name.
        /// </param>
        /// <param name="paramTypes">
        /// The param types.
        /// </param>
        /// <returns>
        /// The <see cref="MemberInfo"/>.
        /// </returns>
        public static MemberInfo GetMemberInfo(Type type, string methodName, Type[] paramTypes)
        {
            return type.GetMethod(
                methodName, 
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy, 
                null, 
                paramTypes, 
                null);
        }

        #endregion
    }
}