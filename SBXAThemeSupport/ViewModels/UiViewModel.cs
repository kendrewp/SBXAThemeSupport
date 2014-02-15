using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using SBXA.UI.WPFControls;

namespace SBXAThemeSupport.ViewModels
{
    public class UiViewModel : ViewModel
    {
        private static readonly UiViewModel _Current = new UiViewModel();
        private readonly Hashtable _DisabledKeys = new Hashtable();

        public static UiViewModel Current
        {
            get { return _Current; }
        }

        public bool IsKeyDisabled(KeyEventArgs keyEventArgs, string fieldName)
        {
            if (_DisabledKeys.ContainsKey(fieldName))
            {
                var currentDisabledKeys = _DisabledKeys[fieldName] as StringCollection;
                if (currentDisabledKeys != null && currentDisabledKeys.Contains(keyEventArgs.Key.ToString()))
                {
                    return (true);
                }
            }

            return false;

        }

        public void DisabledKey(string disabledKey, string fieldName)
        {
            var currentDisabledKeys = new StringCollection();

            if (_DisabledKeys.ContainsKey(fieldName))
            {
                currentDisabledKeys = _DisabledKeys[fieldName] as StringCollection;
                _DisabledKeys.Remove(fieldName);
            }

            if (currentDisabledKeys != null && !currentDisabledKeys.Contains(disabledKey)) currentDisabledKeys.Add(disabledKey);

            _DisabledKeys.Add(fieldName, currentDisabledKeys);
        }

        public void EnabledKey(string disabledKey, string fieldName)
        {
            var currentDisabledKeys = new StringCollection();

            if (_DisabledKeys.ContainsKey(fieldName))
            {
                currentDisabledKeys = _DisabledKeys[fieldName] as StringCollection;
                _DisabledKeys.Remove(fieldName);
            }

            if (currentDisabledKeys != null && currentDisabledKeys.Contains(disabledKey)) currentDisabledKeys.Remove(disabledKey);

            _DisabledKeys.Add(fieldName, currentDisabledKeys);
        }

    }
}
