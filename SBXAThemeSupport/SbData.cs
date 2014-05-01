// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SbData.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text;

    using SBXA.Shared;

    /// <summary>
    ///     The sb data.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(SbDataConverter))]
    public class SbData
    {
        #region Fields

        private readonly List<string[]> dataLineValues = new List<string[]>();

        private readonly char subValueMark;

        private readonly char valueMark;

        private char attributeMark;

        private int maxLineLength;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SbData"/> class.
        /// </summary>
        /// <param name="attributeMark">
        /// The attribute mark.
        /// </param>
        /// <param name="valueMark">
        /// The value mark.
        /// </param>
        /// <param name="subValueMark">
        /// The sub value mark.
        /// </param>
        public SbData(char attributeMark, char valueMark, char subValueMark)
        {
            this.attributeMark = attributeMark;
            this.valueMark = valueMark;
            this.subValueMark = subValueMark;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SbData"/> class.
        /// </summary>
        /// <param name="sbAllData">
        /// The sb all data.
        /// </param>
        public SbData(string sbAllData)
            : this(sbAllData, Delimiters.AttributeMark, Delimiters.ValueMark, Delimiters.SubValueMark)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SbData"/> class.
        /// </summary>
        /// <param name="sbString">
        /// The sb string.
        /// </param>
        public SbData(SBString sbString)
            : this(sbString.GetRawString())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SbData"/> class.
        /// </summary>
        /// <param name="sbAllData">
        /// The sb all data.
        /// </param>
        /// <param name="attributeMark">
        /// The attribute mark.
        /// </param>
        /// <param name="valueMark">
        /// The value mark.
        /// </param>
        /// <param name="subvalueMark">
        /// The subvalue mark.
        /// </param>
        public SbData(string sbAllData, char attributeMark, char valueMark, char subvalueMark)
            : this(attributeMark, valueMark, subvalueMark)
        {
            if (string.IsNullOrEmpty(sbAllData))
            {
                return;
            }

            string[] lines = sbAllData.Split(attributeMark);

            foreach (var line in lines)
            {
                string[] lineValues = line.Split(this.valueMark);
                this.dataLineValues.Add(lineValues);

                if (this.maxLineLength < lineValues.Length)
                {
                    this.maxLineLength = lineValues.Length;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return this.dataLineValues.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(this.GetRawString());
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The new.
        /// </summary>
        /// <returns>
        ///     The <see cref="SbData" />.
        /// </returns>
        public static SbData New()
        {
            return new SbData(Delimiters.AttributeMark, Delimiters.ValueMark, Delimiters.SubValueMark);
        }

        /// <summary>
        /// The count sub value mark.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int CountSubValueMark(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return 0;
            }

            return data.Split(new[] { this.subValueMark }).Length;
        }

        /// <summary>
        /// The extract.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Extract(int attr)
        {
            return this.Extract(attr, 1);
        }

        /// <summary>
        /// The extract.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// this is 1 based collection because of sb+
        ///     or
        ///     val is lesser than 1, is not allowed
        /// </exception>
        public string Extract(int attr, int val)
        {
            if (attr <= 0)
            {
                throw new IndexOutOfRangeException("this is 1 based collection because of sb+");
            }

            if (val <= 0)
            {
                throw new IndexOutOfRangeException("val < 1 is not allowed");
            }

            attr--;
            val--;
            if (attr >= this.dataLineValues.Count)
            {
                return string.Empty;
            }

            string[] lineValues = this.dataLineValues[attr];
            if (val >= lineValues.Length)
            {
                return string.Empty;
            }

            if (lineValues[val] == null)
            {
                lineValues[val] = string.Empty;
            }

            return lineValues[val];
        }

        /// <summary>
        /// The extract.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="subVal">
        /// The sub val.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// this is 1 based collection because of sb+
        ///     or
        ///     val is lesser than 1 is not allowed
        ///     or
        ///     Subvalue position must be bigger than 0
        /// </exception>
        public string Extract(int attr, int val, int? subVal)
        {
            if (attr <= 0)
            {
                throw new IndexOutOfRangeException("this is 1 based collection because of sb+");
            }

            if (val <= 0)
            {
                throw new IndexOutOfRangeException("val < 1 is not allowed");
            }

            if (subVal.HasValue)
            {
                if (subVal.Value <= 0)
                {
                    throw new IndexOutOfRangeException("Subvalue position must be bigger than 0");
                }
            }

            attr--;
            val--;
            if (subVal.HasValue)
            {
                subVal = subVal.Value - 1;
            }

            if (attr >= this.dataLineValues.Count)
            {
                return string.Empty;
            }

            string[] lineValues = this.dataLineValues[attr];
            if (val >= lineValues.Length)
            {
                return string.Empty;
            }

            // Handle Subvalue
            if (subVal.HasValue)
            {
                if (string.IsNullOrEmpty(lineValues[val]))
                {
                    lineValues[val] = string.Empty;
                }

                string[] subValues = lineValues[val].Split(this.subValueMark);
                if (subValues.Length <= subVal)
                {
                    return string.Empty;
                }

                return subValues[subVal.Value];
            }
            else
            {
                return lineValues[val];
            }
        }

        /// <summary>
        /// The extract array.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] ExtractArray(int attr)
        {
            if (this.dataLineValues.Count < attr)
            {
                return new string[] { };
            }

            return this.dataLineValues[attr - 1];
        }

        /// <summary>
        /// The extract array.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] ExtractArray(int attr, int val)
        {
            string[] attVals = this.ExtractArray(attr);
            val--;
            if (val >= attVals.Length)
            {
                return new string[] { };
            }

            return attVals[val].Split(Delimiters.SubValueMark);
        }

        /// <summary>
        /// The extract to sb data.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <returns>
        /// The <see cref="SbData"/>.
        /// </returns>
        public SbData ExtractToSbData(int attr)
        {
            return new SbData(this.ExtractToSbString(attr));
        }

        /// <summary>
        /// The extract to sb string.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ExtractToSbString(int attr)
        {
            if (attr > this.dataLineValues.Count)
            {
                return string.Empty;
            }

            attr--;
            return string.Join(this.valueMark.ToString(), this.dataLineValues[attr]);
        }

        /// <summary>
        ///     The get raw string.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetRawString()
        {
            var myString = new StringBuilder();
            bool isFirstEmpty = false;

            foreach (var dataLineValue in this.dataLineValues)
            {
                string stringToAppend = string.Join(this.valueMark.ToString(), dataLineValue);

                if (string.IsNullOrEmpty(stringToAppend) && myString.Length == 0)
                {
                    stringToAppend = this.valueMark.ToString();
                    isFirstEmpty = true;
                }

                if (myString.Length == 0)
                {
                    myString.Append(stringToAppend);
                    continue;
                }

                myString.Append(Delimiters.AttributeMark + stringToAppend);
            }

            if (isFirstEmpty)
            {
                myString.Remove(0, this.valueMark.ToString().Length);
            }

            return myString.ToString();
        }

        /// <summary>
        ///     The get sb string.
        /// </summary>
        /// <returns>
        ///     The <see cref="SBString" />.
        /// </returns>
        public SBString GetSBString()
        {
            return new SBString(this.GetRawString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, bool newValue)
        {
            this.SBInsert(attr, newValue ? "1" : "0");
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, long newValue)
        {
            SBInsert(attr, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, double newValue)
        {
            SBInsert(attr, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int newValue)
        {
            SBInsert(attr, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, string newValue)
        {
            SBInsert(attr, 1, newValue);
        }

        /// <summary>
        /// Fügt den Wert zum bestimmten Attribut hinzu, array wir VM getrennt abgelegt
        /// </summary>
        /// <param name="attr">
        /// The attribute.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, string[] newValue)
        {
            if (newValue == null)
            {
                this.SBInsert(attr, string.Empty);
                return;
            }

            for (int i = 0; i < newValue.Length; i++)
            {
                SBInsert(attr, i + 1, newValue[i]);
            }
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, int newValue)
        {
            SBInsert(attr, val, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, decimal newValue)
        {
            SBInsert(attr, val, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, uint newValue)
        {
            SBInsert(attr, val, newValue.ToString());
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, string newValue)
        {
            this.SBInsert(attr, val, null, newValue);
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, bool newValue)
        {
            this.SBInsert(attr, val, newValue ? "1" : "0");
        }

        /// <summary>
        /// The newValue will be added separated by SVM
        /// </summary>
        /// <param name="attr">
        /// The attribute.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        public void SBInsert(int attr, int val, string[] newValue)
        {
            for (int i = 0; i < newValue.Length; i++)
            {
                this.SBInsert(attr, val, i + 1, newValue[i]);
            }
        }

        /// <summary>
        /// The sb insert.
        /// </summary>
        /// <param name="attr">
        /// The attr.
        /// </param>
        /// <param name="val">
        /// The val.
        /// </param>
        /// <param name="subVal">
        /// The sub val.
        /// </param>
        /// <param name="newValue">
        /// The new value.
        /// </param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// this is 1 based collection because of sb+
        ///     or
        ///     val is lesser than  1 is not allowed
        ///     or
        ///     Subvalue position must be bigger than 0
        /// </exception>
        public void SBInsert(int attr, int val, int? subVal, string newValue)
        {
            if (attr <= 0)
            {
                throw new IndexOutOfRangeException("this is 1 based collection because of sb+");
            }

            if (val <= 0)
            {
                throw new IndexOutOfRangeException("val < 1 is not allowed");
            }

            if (subVal.HasValue)
            {
                if (subVal.Value <= 0)
                {
                    throw new IndexOutOfRangeException("Subvalue position must be bigger than 0");
                }
            }

            attr--;
            val--;
            if (subVal.HasValue)
            {
                subVal = subVal.Value - 1;
            }

            if (attr >= this.dataLineValues.Count)
            {
                while (attr > this.dataLineValues.Count)
                {
                    this.dataLineValues.Add(new[] { string.Empty });
                }

                this.InsertNewLine(attr, val, newValue);
                return;
            }

            string[] lineValues = this.dataLineValues[attr];
            if (lineValues.Length <= val)
            {
                Array.Resize(ref lineValues, val + 1);
            }

            // handel subvalue
            if (subVal.HasValue)
            {
                if (string.IsNullOrEmpty(lineValues[val]))
                {
                    lineValues[val] = string.Empty;
                }

                string[] subValues = lineValues[val].Split(this.subValueMark);
                if (subValues.Length <= subVal)
                {
                    Array.Resize(ref subValues, subVal.Value + 1);
                }

                subValues[subVal.Value] = newValue;
                lineValues[val] = string.Join(this.subValueMark.ToString(), subValues);
            }
            else
            {
                lineValues[val] = newValue;
            }

            this.dataLineValues[attr] = lineValues;
        }

        /// <summary>
        /// Fügt alle werte die mit Attribute Mark geliefert wurde zu einem Array
        /// </summary>
        /// <param name="valPos">
        /// The value position.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] ToAttributesArray(int valPos)
        {
            var attributeValues = new List<string>(this.dataLineValues.Count);
            for (int i = 1; i <= this.Count; i++)
            {
                attributeValues.Add(this.Extract(i, valPos));
            }

            return attributeValues.ToArray();
        }

        /// <summary>
        ///     The to string.
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public override string ToString()
        {
            return this.GetRawString();
        }

        /// <summary>
        /// The to values array.
        /// </summary>
        /// <param name="attPos">
        /// The att pos.
        /// </param>
        /// <returns>
        /// The <see cref="string[]"/>.
        /// </returns>
        public string[] ToValuesArray(int attPos)
        {
            return this.ExtractArray(attPos);
        }

        #endregion

        #region Methods

        private void InsertNewLine(int attr, int val, string newValue)
        {
            if (this.maxLineLength <= val)
            {
                this.maxLineLength = val + 1;
            }

            var lineValues = new string[this.maxLineLength];
            this.dataLineValues.Add(lineValues);
            lineValues[val] = newValue;
        }

        #endregion

        //public IEnumerator<string[]> GetEnumerator()
        //{
        //    for (int i = 0; i < _dataLineValues.Count; i++)
        //    {
        //        if (_dataLineValues[i].Length < maxLineLength)
        //        {
        //            string[] dataLine = _dataLineValues[i];
        //            Array.Resize(ref dataLine, maxLineLength);
        //        }
        //        yield return _dataLineValues[i];
        //    }
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return this.GetEnumerator();
        //}
    }
}