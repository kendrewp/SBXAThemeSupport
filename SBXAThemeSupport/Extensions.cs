// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Ruf Informatik AG">
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
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Xml.Serialization;

    using Microsoft.Practices.ServiceLocation;
    using Microsoft.Practices.Unity;

    using SBXA.Shared;
    using SBXA.UI.Client;
    using SBXA.UI.WPFControls;

    /// <summary>
    ///     The extensions.
    /// </summary>
    public static class Extensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add object to tag.
        /// </summary>
        /// <param name="gridData">
        /// The grid data.
        /// </param>
        /// <param name="newInstance">
        /// The new instance.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="isOverwrite">
        /// The is overwrite.
        /// </param>
        /// <typeparam name="T">
        /// object to tag
        /// </typeparam>
        public static void AddObjectToTag<T>(this ITag gridData, T newInstance, string key = null, bool isOverwrite = false)
        {
            if (gridData == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = typeof(T).FullName;
            }

            if (gridData.Tag == null)
            {
                gridData.Tag = new Dictionary<string, T>();
            }

            var data = (Dictionary<string, T>)gridData.Tag;
            if (isOverwrite && data.ContainsKey(key))
            {
                data[key] = newInstance;
            }
            else
            {
                ((Dictionary<string, T>)gridData.Tag).Add(key, newInstance);
            }
        }

        /// <summary>
        /// The add range.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="newItems">
        /// The new items.
        /// </param>
        /// <typeparam name="T">
        /// to tag
        /// </typeparam>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> newItems)
        {
            if (list == null)
            {
                return;
            }

            foreach (var newItem in newItems)
            {
                list.Add(newItem);
            }
        }

        /// <summary>
        /// The add range.
        /// </summary>
        /// <param name="list">
        /// The list.
        /// </param>
        /// <param name="newItems">
        /// The new items.
        /// </param>
        public static void AddRange(this IList list, IEnumerable newItems)
        {
            if (list == null)
            {
                return;
            }

            foreach (var newItem in newItems)
            {
                list.Add(newItem);
            }
        }

        /// <summary>
        /// The build up.
        /// </summary>
        /// <param name="serviceLocator">
        /// The service locator.
        /// </param>
        /// <param name="instance">
        /// The instance.
        /// </param>
        public static void BuildUp(this IServiceLocator serviceLocator, object instance)
        {
            if (instance == null)
            {
                return;
            }

            var unityContainer = ServiceLocator.Current.GetInstance<IUnityContainer>();
            if (unityContainer != null)
            {
                unityContainer.BuildUp(instance.GetType(), instance);
            }
        }

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
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="throwException">
        /// if set to <c>true</c> [throw exception].
        /// </param>
        /// <returns>
        /// The object that is being extended.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// queue
        /// </exception>
        /// <exception cref="System.Exception">
        /// Not able to peek element from the queue
        /// </exception>
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
            var startTime = DateTime.Now;
            SBPlusClient.LogInformation("DoEvents Starting");

            if (Application.Current == null || Application.Current.Dispatcher == null)
            {
                SBPlusClient.LogInformation("DoEvents Application.Current or Application.Current.Dispatcher is null");
                return;
            }

            if (IsUiThreadSuspended())
            {
                SBPlusClient.LogInformation("DoEvents ui thread is suspended so go out here");
                return;
            }

            var frame = new DispatcherFrame(true);
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                (SendOrPostCallback)delegate(object arg)
                    {
                        var f = arg as DispatcherFrame;
                        if (f == null)
                        {
                            SBPlusClient.LogInformation("DoEvents BehinInvoke - DispatcherFrame is null");
                            return;
                        }

                        f.Continue = false;
                    }, 
                frame);

            if (Application.Current.Dispatcher.CheckAccess())
            {
                try
                {
                    SBPlusClient.LogInformation("DoEvents Dispatcher.CheckAccess");

                    Dispatcher.PushFrame(frame);
                }
                catch (Exception exception)
                {
                    SBPlusClient.LogError("A problem in DoEvents, trying to push the frame.", exception);
                }
            }
            else
            {
                SBPlusClient.LogInformation("DoEvents Dispatcher.CheckAccess is false so call BeginInvoke");

                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Dispatcher.PushFrame(frame)));
            }

            SBPlusClient.LogInformation("DoEvents end. Milliseconds " + (DateTime.Now - startTime).TotalMilliseconds);
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
        /// The for each.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="T">
        /// IEnumerable
        /// </typeparam>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// not null exeption
        /// </exception>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (var item in source)
            {
                action(item);
            }

            return source;
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
        public static string GetDateInternal(this DateTime dateTime)
        {
            return SBConv.IConv(dateTime.ToString("dd.MM.yyyy"), "D4.");
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
            return !dateTime.HasValue ? string.Empty : dateTime.Value.GetDateInternal();
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
        /// The get object from tag.
        /// </summary>
        /// <param name="gridData">
        /// The grid data.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// param
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        public static T GetObjectFromTag<T>(this ITag gridData, string key = null)
        {
            if (gridData == null || gridData.Tag == null)
            {
                return default(T);
            }

            if (string.IsNullOrEmpty(key))
            {
                key = typeof(T).FullName;
            }

            var data = gridData.Tag as Dictionary<string, T>;
            if (data == null)
            {
                return default(T);
            }

            T entry;
            data.TryGetValue(key, out entry);
            return entry;
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
        public static string GetTimeInternal(this DateTime dateTime)
        {
            return SBConv.IConv(dateTime.ToString("HH:mm:ss"), "MTS");
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
            return !dateTime.HasValue ? string.Empty : dateTime.Value.GetTimeInternal();
        }

        /// <summary>
        /// The is nan.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsNan(this double number)
        {
            return double.IsNaN(number);
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
        ///     The is ui thread suspended.
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsUiThreadSuspended()
        {
            if (Application.Current == null || Application.Current.Dispatcher == null)
            {
                return false;
            }

            bool isSuspended = Application.Current.Dispatcher.Thread.ThreadState == ThreadState.Suspended;
            if (isSuspended)
            {
                SBPlusClient.LogInformation("Ui Thread is suspended");
            }

            return isSuspended;
        }

        /// <summary>
        /// The no exception action.
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <returns>
        /// The <see cref="Exception"/>.
        /// </returns>
        public static Exception NoExceptionAction(this object obj, Action action)
        {
            if (action == null)
            {
                return new ArgumentNullException("action");
            }

            try
            {
                action();
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }

        /// <summary>
        /// Peeks the specified queue.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object that this method is extending.
        /// </typeparam>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <param name="throwException">
        /// if set to <c>true</c> [throw exception].
        /// </param>
        /// <returns>
        /// The object that is being extended.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// queue
        /// </exception>
        /// <exception cref="System.Exception">
        /// Not able to peek element from the queue
        /// </exception>
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
        /// The remove object from tag.
        /// </summary>
        /// <param name="gridData">
        /// The grid data.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <typeparam name="T">
        /// param
        /// </typeparam>
        public static void RemoveObjectFromTag<T>(this ITag gridData, string key = null)
        {
            if (gridData == null || gridData.Tag == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = typeof(T).FullName;
            }

            var data = (Dictionary<string, T>)gridData.Tag;
            if (data.ContainsKey(key))
            {
                data.Remove(key);
            }
        }

        /// <summary>
        /// resolves the name to handle
        /// </summary>
        /// <param name="sbPlus">
        /// param
        /// </param>
        /// <param name="handeOrSbName">
        /// handler or sbname
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ResolveServerTokensByRuf(this SBPlus sbPlus, string handeOrSbName)
        {
            const char Delimitter = '*';

            if (string.IsNullOrEmpty(handeOrSbName))
            {
                return handeOrSbName;
            }

            if (handeOrSbName.ToUpper() == "@FORM")
            {
                return sbPlus.CurrentForm.SBObjectHandle;
            }

            if (handeOrSbName.StartsWith("@"))
            {
                // example @FORM*RTF1
                string[] nameParts = handeOrSbName.Split(new[] { Delimitter });
                if (nameParts.Length > 1 && nameParts[0].ToUpper() == "@FORM")
                {
                    string formHandle = sbPlus.ResolveServerTokensByRuf("@FORM");
                    List<string> parts = nameParts.ToList();
                    parts.RemoveAt(0);
                    return string.Format("{0}{1}{2}", formHandle, Delimitter, string.Join(Delimitter.ToString(), parts));
                }

                string newHandle = sbPlus.ResolveServerTokens(handeOrSbName);
                if (!string.IsNullOrEmpty(newHandle) && newHandle != "0")
                {
                    return newHandle;
                }
            }

            return handeOrSbName;
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
        /// The to attributes.
        /// </summary>
        /// <param name="sbStringValue">
        /// The sb string value.
        /// </param>
        /// <param name="valPos">
        /// The val pos.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public static string[] ToAttributes(this string sbStringValue, int valPos = 1)
        {
            var sbData = new SbData(sbStringValue);
            return sbData.ToAttributesArray(valPos);
        }

        /// <summary>
        /// The to bool.
        /// </summary>
        /// <param name="stringValue">
        /// The string value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ToBool(this string stringValue)
        {
            bool result;
            if (stringValue == null)
            {
                return false;
            }

            if (stringValue == "1")
            {
                return true;
            }

            if (stringValue == "0")
            {
                return false;
            }

            if (stringValue.ToUpper().Trim() == "TRUE")
            {
                return true;
            }

            if (stringValue.ToUpper().Trim() == "FALSE")
            {
                return false;
            }

            if (bool.TryParse(stringValue, out result))
            {
                return result;
            }

            return false;
        }

        /// <summary>
        /// The to bool if empty true.
        /// </summary>
        /// <param name="stringValue">
        /// The string value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ToBoolIfEmptyTrue(this string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            return stringValue.ToBool();
        }

        /// <summary>
        /// To the double.
        /// </summary>
        /// <param name="stringValue">
        /// The string value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// The object as a <see cref="double"/>.
        /// </returns>
        /// <exception cref="System.InvalidCastException">
        /// If it is not possible the exception is thrown.
        /// </exception>
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
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <returns>
        /// A <see cref="StringBuilder"/> with the nested details of the exception.
        /// </returns>
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
        /// <param name="stringValue">
        /// The string value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <returns>
        /// The object as a <see cref="int"/>.
        /// </returns>
        /// <exception cref="System.InvalidCastException">
        /// If it is not possible the exception is thrown.
        /// </exception>
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

        /// <summary>
        /// The to sb param.
        /// </summary>
        /// <param name="stringToConvert">
        /// The string to convert.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToSbParam(this string stringToConvert)
        {
            if (string.IsNullOrEmpty(stringToConvert))
            {
                return stringToConvert;
            }

            stringToConvert = stringToConvert.Replace("/", "@YYSLASH@");
            while (stringToConvert.Contains(' '))
            {
                stringToConvert = stringToConvert.Replace(" ", "@YYSPACE@");
            }

            return stringToConvert;
        }

        /// <summary>
        /// The to unsecure string.
        /// </summary>
        /// <param name="secureString">
        /// The secure string.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string ToUnsecureString(this SecureString secureString)
        {
            string result;
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                result = Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }

            return result;
        }

        /// <summary>
        /// The to values.
        /// </summary>
        /// <param name="sbStringValue">
        /// The sb string value.
        /// </param>
        /// <param name="attPos">
        /// The att pos.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public static string[] ToValues(this string sbStringValue, int attPos = 1)
        {
            var sbData = new SbData(sbStringValue);
            return sbData.ToValuesArray(attPos);
        }

        #endregion
    }
}