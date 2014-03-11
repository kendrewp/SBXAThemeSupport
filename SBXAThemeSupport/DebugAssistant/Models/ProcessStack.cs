// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugWindowManager.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="DebugWindowManager.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.DebugAssistant.Models
{
    public class ProcessStack : ObservableStack<ProcessDescription>
    {
        public override void Clear()
        {
            foreach (var processDescription in this)
            {
                processDescription.Clear();
            }
            base.Clear();
        }
    }

    public class ProcessDescription : ObservableEntity
    {
        private bool _IsCurrent;
        public string Name { get; private set; }

        public ProcessStack Children { get; private set; }

        public ProcessDescription HistoryProcessDescription { get; set; }

        public bool IsCurrent
        {
            get { return _IsCurrent; }
            set
            {
                _IsCurrent = value;
                RaisePropertyChanged("IsCurrent");
            }
        }

        public ProcessDescription(string name)
        {
            Name = name;
            Children = new ProcessStack();
        }

        public void Clear()
        {
            foreach (var item in Children)
            {
                item.Clear();
            }
            Children.Clear();
        }

        public void ClearHistoryReferences()
        {
            HistoryProcessDescription = null;
            foreach (var item in Children)
            {
                item.ClearHistoryReferences();
            }
        }

    }
}
