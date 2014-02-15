using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SBXAThemeSupport.Models;

namespace SBXAThemeSupport.Views
{

    public class SBStringViewerCellTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _defaultTemplate;
        public DataTemplate DefaultTemplate
        {
            get { return _defaultTemplate; }
            set { _defaultTemplate = value; }
        }

        private DataTemplate _alternateTemplate;
        public DataTemplate AlternateTemplate
        {
            get { return _alternateTemplate; }
            set { _alternateTemplate = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataUnit = item as NestedAttribute;

            if (dataUnit == null) return DefaultTemplate;

            //lets see what template we need to select according to the specified property value
            if (dataUnit.IsNested)
            {
                return AlternateTemplate;
            }
            return DefaultTemplate;
        }
    }
}
