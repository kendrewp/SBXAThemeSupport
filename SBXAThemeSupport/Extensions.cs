using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using SBXA.Shared;

namespace SBXAThemeSupport
{
    public static class Extensions
    {

        public static T Peek<T>(this ConcurrentQueue<T> queue, bool throwException = false)
        {
            if (queue == null)
            {
                if (throwException) throw new ArgumentNullException("queue");
                return default(T);
            }

            T targetItem;
            if(queue.TryPeek(out targetItem))
            {
                return targetItem;
            }
            if(throwException) throw new Exception("Not able to peek element from the queue");
            return default(T);
        }

        public static T Dequeue<T>(this ConcurrentQueue<T> queue, bool throwException = false)
        {
            if (queue == null)
            {
                if (throwException) throw new ArgumentNullException("queue");
                return default(T);
            }

            T targetItem;
            if (queue.TryDequeue(out targetItem))
            {
                return targetItem;
            }
            if (throwException) throw new Exception("Not able to peek element from the queue");
            return default(T);
        }

        public static StringBuilder ToExceptionDetails(this Exception exception)
        {
            var stringBuilder = new StringBuilder();
            if (exception == null) return stringBuilder;

            stringBuilder.AppendLine(exception.GetType().FullName);
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);
            if (exception.InnerException != null)
            {
                stringBuilder.AppendLine(exception.InnerException.ToExceptionDetails().ToString());
            }
            return stringBuilder;

        }

        /// <summary>
        /// Deserialize the object and execute the Method OnLoaded() when exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(string xmlData)
        {
            using (var inputStream = new StringReader(xmlData))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var target = (T)xmlSerializer.Deserialize(inputStream);
                var onLoaded = target.GetType().GetMethod("OnLoaded",
                                                          BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                                                          BindingFlags.Instance);
                if (onLoaded != null) onLoaded.Invoke(target, null);
                return target;
            }
        }

        public static string SerializeToXml<T>(this T objectToSerialize)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                xmlSerializer.Serialize(memoryStream, objectToSerialize);
                memoryStream.Position = 0;

                var textReader = new StreamReader(memoryStream);
                return textReader.ReadToEnd();
            }
        }

        public static void DoEvents(this FrameworkElement frameworkElement)
        {
            DoEvents();
        }

        public static void DoEvents()
        {
            var frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke
                (
                    DispatcherPriority.Background,
                    (SendOrPostCallback)delegate(object arg)
                                            {
                                                var f = arg as DispatcherFrame;
                                                if (f != null) f.Continue = false;
                                            },
                    frame
                );
            Dispatcher.PushFrame(frame);
        }

        public static string GetDateInternal(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return string.Empty;
            return SBConv.IConv(dateTime.Value.ToString("dd.MM.yyyy"), "D4.");
        }

        public static string GetTimeInternal(this DateTime? dateTime)
        {
            if (!dateTime.HasValue) return string.Empty;
            return SBConv.IConv(dateTime.Value.ToString("HH:mm:ss"), "MTS");
        }

        public static DateTime GetDateTime(this string internalDate, string internalTime, DateTime defaultValue)
        {
            var date = GetDateTimeNullable(internalDate, internalTime);
            if (date.HasValue) return date.Value;
            return defaultValue;
        }

        public static DateTime? GetDateTimeNullable(this string internalDate, string internalTime)
        {
            if (string.IsNullOrEmpty(internalDate)) return null;
            if (internalDate.Contains(".")) return DateTime.Parse(internalDate, CultureInfo.GetCultureInfo("de"));

            internalDate = SBConv.OConv(internalDate, "D4.");
            if (!string.IsNullOrEmpty(internalTime))
            {
                internalTime = SBConv.OConv(internalTime, "MTS");
                internalDate = internalDate + " " + internalTime;
            }

            return DateTime.Parse(internalDate, CultureInfo.GetCultureInfo("de"));
        }

        public static T FindAncestor<T>(this object dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null) return null;
            var target = (DependencyObject)dependencyObject;
            do
            {
                DependencyObject visualParent = target is Visual ? VisualTreeHelper.GetParent(target) : null;
                if (visualParent != null)
                {
                    target = visualParent;
                }
                else
                {
                    DependencyObject logicalParent = LogicalTreeHelper.GetParent(target);
                    if (logicalParent != null)
                    {
                        target = logicalParent;
                    }
                    else
                    {
                        return null;
                    }
                }
            } while (!(target is T));
            return (T)target;
        }

        public static T FindVisualChildByName<T>(this DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                var controlName = child.GetValue(FrameworkElement.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        public static int ToInt(this string stringValue, int? defaultValue = null)
        {
            int result;
            if (int.TryParse(stringValue, out result))
                return result;

            if (defaultValue.HasValue)
            {
                return defaultValue.Value;
            }
            throw new InvalidCastException();
        }

        public static double ToDouble(this string stringValue, double? defaultValue = null)
        {
            double result;
            if (double.TryParse(stringValue, out result))
                return result;

            if (defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            throw new InvalidCastException();
        }

        public static void Show(this ContextMenu ctx, double positionX, double positionY, UIElement relativeTo)
        {
            ctx.Placement = PlacementMode.Relative;
            ctx.PlacementTarget = relativeTo;
            ctx.PlacementRectangle = new Rect(positionX, positionY, 0, 0);
            ctx.IsOpen = true;
        }

        public static bool IsNumeric(this string toParse)
        {
            double result;
            return !string.IsNullOrEmpty(toParse) && double.TryParse(toParse, out result);
        }
    }
}