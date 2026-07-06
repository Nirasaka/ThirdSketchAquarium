using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace TriLibCore.Dae.Schema
{
    public partial class COLLADA
    {
        //private static Regex regex = new Regex(@"\s+");

        private static readonly char[] SplitChars = { ' ', '\t', '\r', '\n' };

        public static string ConvertFromArray<T>(IList<T> array)
        {
            if (array == null)
                return null;

            StringBuilder text = new StringBuilder();
            if (typeof(T) == typeof(double))
            {
                // If type is double, then use a plain ToString with no exponent
                for (int i = 0; i < array.Count; i++)
                {
                    object value1 = array[i];
                    double value = (double)value1;
                    text.Append(
                        value.ToString(
                            "0.000000",
                            NumberFormatInfo.InvariantInfo));
                    if ((i + 1) < array.Count)
                        text.Append(" ");
                }
            }
            else
            {
                for (int i = 0; i < array.Count; i++)
                {
                    text.Append(Convert.ToString(array[i], NumberFormatInfo.InvariantInfo));
                    if ((i + 1) < array.Count)
                        text.Append(" ");
                }
            }
            return text.ToString();
        }

        internal static string[] ConvertStringArray(string arrayStr)
        {
            string[] elements = arrayStr.Trim().Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            string[] ret = new string[elements.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = elements[i];
            return ret;
        }

        internal static int[] ConvertIntArray(string arrayStr)
        {
            var ret = new int[CountTokens(arrayStr)];
            var index = 0;
            var i = 0;
            var length = arrayStr.Length;
            while (i < length)
            {
                while (i < length && IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
                if (i >= length)
                {
                    break;
                }
                var start = i;
                while (i < length && !IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
                ret[index++] = (int)ParseLongToken(arrayStr, start, i);
            }
            return ret;
        }


        internal static long[] ConvertLongArray(string arrayStr)
        {
            var ret = new long[CountTokens(arrayStr)];
            var index = 0;
            var i = 0;
            var length = arrayStr.Length;
            while (i < length)
            {
                while (i < length && IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
                if (i >= length)
                {
                    break;
                }
                var start = i;
                while (i < length && !IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
                ret[index++] = ParseLongToken(arrayStr, start, i);
            }
            return ret;
        }

        internal static double[] ConvertDoubleArray(string arrayStr)
        {
            var ret = new double[CountTokens(arrayStr)];
            try
            {
                var index = 0;
                var i = 0;
                var length = arrayStr.Length;
                while (i < length)
                {
                    while (i < length && IsSplitChar(arrayStr[i]))
                    {
                        i++;
                    }
                    if (i >= length)
                    {
                        break;
                    }
                    var start = i;
                    while (i < length && !IsSplitChar(arrayStr[i]))
                    {
                        i++;
                    }
                    ret[index++] = double.Parse(arrayStr.Substring(start, i - start), CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            return ret;
        }

        private static bool IsSplitChar(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        private static int CountTokens(string arrayStr)
        {
            var count = 0;
            var i = 0;
            var length = arrayStr.Length;
            while (i < length)
            {
                while (i < length && IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
                if (i >= length)
                {
                    break;
                }
                count++;
                while (i < length && !IsSplitChar(arrayStr[i]))
                {
                    i++;
                }
            }
            return count;
        }

        private static long ParseLongToken(string arrayStr, int start, int end)
        {
            var i = start;
            var negative = false;
            if (i < end && (arrayStr[i] == '-' || arrayStr[i] == '+'))
            {
                negative = arrayStr[i] == '-';
                i++;
            }
            long value = 0;
            for (; i < end; i++)
            {
                value = value * 10 + (arrayStr[i] - '0');
            }
            return negative ? -value : value;
        }

        internal static bool[] ConvertBoolArray(string arrayStr)
        {
            string[] elements = arrayStr.Trim().Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            bool[] ret = new bool[elements.Length];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bool.Parse(elements[i]);
            return ret;
        }

        public static COLLADA Load(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var result = Load(stream);
            return result;
        }

        public static COLLADA Load(Stream stream)
        {
            var str = new StreamReader(stream);
            var xSerializer = new XmlSerializer(typeof(COLLADA));
            return (COLLADA)xSerializer.Deserialize(str);
        }
    }
}