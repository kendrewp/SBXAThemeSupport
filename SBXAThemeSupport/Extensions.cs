// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLoader.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// <copyright file="AssemblyLoader.cs" company="Woolworths, Limited.">
//   Copyright © Woolworths, Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
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

    /// <summary>
    ///     The extensions.
    /// </summary>
    public static class Extensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The deep clone.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
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

        /// <summary>
        /// Dequeues the specified queue.
        /// </summary>
        /// <typeparam name="T">The type of the object that this method is extending.</typeparam>
        /// <param name="queue">The queue.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns>The object that is being extended.</returns>
        /// <exception cref="System.ArgumentNullException">queue</exception>
        /// <exception cref="System.Exception">Not able to peek element from the queue</exception>
        public static T Dequeue<T>(this ConcurrentQueue<T> queue, bool throwException = false)
        {
            if (queue == null)
            {
                if (throwException)
                {
                    throw new ArgumentNullException("queue");
                }

                return default(T);
            }

            T targetItem;
            if (queue.TryDequeue(out targetItem))
            {
                return targetItem;
            }

            if (throwException)
            {
                throw new Exception("Not able to peek element from the queue");
            }

            return default(T);
        }

        /// <summary>
        /// Deserialize the object and execute the Method OnLoaded() when exists
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <param name="xmlData">
        /// The XML data which contains the values for the object to be deserialized.
        /// </param>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T DeserializeFromXml<T>(string xmlData)
        {
            using (var inputStream = new StringReader(xmlData))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                var target = (T)xmlSerializer.Deserialize(inputStream);
                var onLoaded = target.GetType()
                    .GetMethod("OnLoaded", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
                if (onLoaded != null)
                {
                    onLoaded.Invoke(target, null);
                }

                return target;
            }
        }

        /// <summary>
        /// The do events.
        /// </summary>
        /// <param name="frameworkElement">
        /// The framework element.
        /// </param>
        public static void DoEvents(this FrameworkElement frameworkElement)
        {
            DoEvents();
        }

        /// <summary>
        ///     The do events.
        /// </summary>
        public static void DoEvents()
        {
            var frame = new DispatcherFrame(true);
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                (SendOrPostCallback)delegate(object arg)
                    {
                        var f = arg as DispatcherFrame;
                        if (f != null)
                        {
                            f.Continue = false;
                        }
                    }, 
                frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// The find ancestor.
        /// </summary>
        /// <param name="dependencyObject">
        /// The dependency object.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T FindAncestor<T>(this object dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

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
            }
            while (!(target is T));
            return (T)target;
        }

        /// <summary>
        /// The find visual child by name.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
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
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// The get date internal.
        /// </summary>
        /// <param name="dateTime">
        /// The date time.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetDateInternal(this DateTime? dateTime)
        {
            return !dateTime.HasValue ? string.Empty : SBConv.IConv(dateTime.Value.ToString("dd.MM.yyyy"), "D4.");
        }

        /// <summary>
        /// The get date time.
        /// </summary>
        /// <param name="internalDate">
        /// The internal date.
        /// </param>
        /// <param name="internalTime">
        /// The internal time.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime GetDateTime(this string internalDate, string internalTime, DateTime defaultValue)
        {
            var date = GetDateTimeNullable(internalDate, internalTime);
            return date.HasValue ? date.Value : defaultValue;
        }

        /// <summary>
        /// The get date time nullable.
        /// </summary>
        /// <param name="internalDate">
        /// The internal date.
        /// </param>
        /// <param name="internalTime">
        /// The internal time.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime? GetDateTimeNullable(this string internalDate, string internalTime)
        {
            if (string.IsNullOrEmpty(internalDate))
            {
                return null;
            }

            if (internalDate.Contains("."))
            {
                return DateTime.Parse(internalDate, CultureInfo.GetCultureInfo("de"));
            }

            internalDate = SBConv.OConv(internalDate, "D4.");
            if (!string.IsNullOrEmpty(internalTime))
            {
                internalTime = SBConv.OConv(internalTime, "MTS");
                internalDate = internalDate + " " + internalTime;
            }

            return DateTime.Parse(internalDate, CultureInfo.GetCultureInfo("de"));
        }

        /// <summary>
        /// The get time internal.
        /// </summary>
        /// <param name="dateTime">
        /// The date time.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetTimeInternal(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return string.Empty;
            }

            return SBConv.IConv(dateTime.Value.ToString("HH:mm:ss"), "MTS");
        }

        /// <summary>
        /// The is numeric.
        /// </summary>
        /// <param name="toParse">
        /// The to parse.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsNumeric(this string toParse)
        {
            double result;
            return !string.IsNullOrEmpty(toParse) && double.TryParse(toParse, out result);
        }

        /// <summary>
        /// Peeks the specified queue.
        /// </summary>
        /// <typeparam name="T">The type of the object that this method is extending.</typeparam>
        /// <param name="queue">The queue.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns>The object that is being extended.</returns>
        /// <exception cref="System.ArgumentNullException">queue</exception>
        /// <exception cref="System.Exception">Not able to peek element from the queue</exception>
        public static T Peek<T>(this ConcurrentQueue<T> queue, bool throwException = false)
        {
            if (queue == null)
            {
                if (throwException)
                {
                    throw new ArgumentNullException("queue");
                }

                return default(T);
            }

            T targetItem;
            if (queue.TryPeek(out targetItem))
            {
                return targetItem;
            }

            if (throwException)
            {
                throw new Exception("Not able to peek element from the queue");
            }

            return default(T);
        }

        /// <summary>
        /// The serialize to xml.
        /// </summary>
        /// <param name="objectToSerialize">
        /// The object to serialize.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
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

        /// <summary>
        /// The show.
        /// </summary>
        /// <param name="ctx">
        /// The ctx.
        /// </param>
        /// <param name="positionX">
        /// The position x.
        /// </param>
        /// <param name="positionY">
        /// The position y.
        /// </param>
        /// <param name="relativeTo">
        /// The relative to.
        /// </param>
        public static void Show(this ContextMenu ctx, double positionX, double positionY, UIElement relativeTo)
        {
            ctx.Placement = PlacementMode.Relative;
            ctx.PlacementTarget = relativeTo;
            ctx.PlacementRectangle = new Rect(positionX, positionY, 0, 0);
            ctx.IsOpen = true;
        }

        /// <summary>
        /// To the double.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The object as a <see cref="double"/>.</returns>
        /// <exception cref="System.InvalidCastException">If it is not possible the exception is thrown.</exception>
        public static double ToDouble(this string stringValue, double? defaultValue = null)
        {
            double result;
            if (double.TryParse(stringValue, out result))
            {
                return result;
            }

            if (defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// To the exception details.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>A <see cref="StringBuilder"/> with the nested details of the exception.</returns>
        public static StringBuilder ToExceptionDetails(this Exception exception)
        {
            var stringBuilder = new StringBuilder();
            if (exception == null)
            {
                return stringBuilder;
            }

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
        /// To the int.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The object as a <see cref="int"/>.</returns>
        /// <exception cref="System.InvalidCastException">If it is not possible the exception is thrown.</exception>
        public static int ToInt(this string stringValue, int? defaultValue = null)
        {
            int result;
            if (int.TryParse(stringValue, out result))
            {
                return result;
            }

            if (defaultValue.HasValue)
            {
                return defaultValue.Value;
            }

            throw new InvalidCastException();
        }

        #endregion
    }
}