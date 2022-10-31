using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CbLib
{
    public static class Extensions
    {
        #region Conversion methods

        public static string ConvertToBase64String(this string textToConvert)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(textToConvert);
            return Convert.ToBase64String(plainTextBytes);

        }

        public static string ConvertFromBase64String(this string textToConvert)
        {
            var base64EncodedBytes = Convert.FromBase64String(textToConvert);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }


        public static DateTime ConvertUnixTimeStringToDate(this string unixTimeText, bool isSeconds = false)
        {
            var asDateTime = DateTime.MinValue;
            long unixTimeNum = 0;

            // remove any character that is not a number (including white space)
            unixTimeText = Regex.Replace(unixTimeText, "[^0-9]", "");

            // Check if the string is empty
            if (!unixTimeText.IsEmpty())
            {
                // Check if the string can be converted to a long
                if (long.TryParse(unixTimeText, out unixTimeNum))
                {
                    // Convert from milliseconds by default
                    asDateTime = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeNum).DateTime;

                    // If set to, convert from seconds
                    if (isSeconds)
                    {
                        asDateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeNum).DateTime;
                    }
                }
            }

            return asDateTime;
        }

        #endregion


        #region Returns bool

        /// <summary>
        /// Returns true if the string is empty
        /// </summary>
        /// <param name="stringToCheck"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string stringToCheck)
        {
            return String.IsNullOrEmpty(stringToCheck);
        }


        /// <summary>
        /// Returns true if the file has a .xlsx extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsExcel(this FileInfo file)
        {
            return file.Extension == ".xlsx";
        }

        /// <summary>
        /// Checks if a List<string> contains an element that is the contained in the given string. Case-insensitive.
        /// </summary>
        /// <param name="searchList"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public static bool ListContains(this List<string> searchList, string searchString)
        {
            searchString = searchString.ToUpperInvariant();

            foreach (var item in searchList)
            {
                var itemUc = item.ToUpperInvariant();


                if (itemUc.Contains(searchString))
                {
                    return true;
                }
                else if (searchString.Contains(itemUc))
                {
                    return true;
                }
            }

            return false;
        }


        #endregion


        #region TryParse
        /// <summary>
        /// Tries to convert the string to a DateTime. If it can't, returns DateTime.MinValue
        /// </summary>
        /// <param name="dateText"></param>
        /// <returns></returns>
        public static DateTime TryParseToDate(this string dateText)
        {
            DateTime date = DateTime.MinValue;
            DateTime.TryParse(dateText, out date);

            return date;
        }

        public static int TryParseToInt(this string numberText)
        {
            int result = 0;
            var onlyNumbersText = numberText.RemoveNonNumericCharcters();

            int.TryParse(onlyNumbersText, out result);

            return result;
        }

        public static double TryParseToDouble(this string numberText)
        {
            double result = 0;
            var onlyNumbersText = numberText.RemoveNonNumericCharcters(true);

            double.TryParse(onlyNumbersText, out result);

            return result;
        }

        /// <summary>
        /// Tries to convert the string to a string representation of a bool. If it can't, returns an empty string.
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <returns></returns>
        public static string TryParseToBoolString(this string textToConvert)
        {
            string boolText = "";
            List<string> trueText = new()
            {
                "true",
                "t",
                "yes",
                "y",
                "1"
            };

            List<string> falseText = new()
            {
                "false",
                "f",
                "no",
                "n",
                "0"
            };

            if (trueText.ListContains(textToConvert))
            {
                boolText = bool.TrueString;
            }
            else if (falseText.ListContains(textToConvert))
            {
                boolText = bool.FalseString;
            }

            return boolText;
        }


        /// <summary>
        /// Tries to parse the string to a dictionary by using the given delimiters and setting each item as a key or value.
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <param name="delimiters"></param>
        /// <returns></returns>
        public static Dictionary<string, string> TryParseToDictionary(this string textToConvert, char[] delimiters)
        {
            var splitText = textToConvert.Split(delimiters);
            var splitDict = new Dictionary<string, string>();

            var key = "";
            var value = "";

            for (int i = 0; i <= splitText.Length; i++)
            {
                value = splitText[i];

                if (i % 2 == 0 || i == 0)
                {
                    key = splitText[i];
                    value = "";
                }

                splitDict.TrySet(key, value);
            }

            return splitDict;
        }

        #endregion


        #region TryGet

        /// <summary>
        /// Try to get a string value from a dictionary by the Key. If the key is not found, return an empty string.
        /// Optional parameter "emptyIsZero" will return a zero if the string is empty.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="emptyIsZero"></param>
        /// <returns></returns>
        public static string TryGetStringValue(this Dictionary<string, string> dict, string key, bool emptyIsZero = false)
        {
            // Set default of return value to empty string unless 0 is specified
            string value = "";
            if (emptyIsZero) { value = "0"; }

            dict.TryGetValue(key, out value);



            return value;
        }

        #endregion


        #region TrySet

        /// <summary>
        /// Try to set the value of a given Key in the dictionary if it exists. 
        /// If it does not, adds the key and then sets the value.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void TrySet(this Dictionary<string, string> dict, string key, string value)
        {
            if (!key.IsEmpty())
            {
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, "");
                }

                dict[key] = value;
            }

        }

        #endregion


        #region Collection Extensions
        public static List<T> Clone<T>(this List<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static List<List<T>> ChunkBy<T>(this List<T> values, int chunkSize)
        {
            var chunkedList = new List<List<T>>();

            if (chunkSize == 0)
            {
                chunkedList.Add(values);
            }
            else
            {
                chunkedList =
                    values
                        .Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / chunkSize)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();
            }

            return chunkedList;
        }


        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }

        /// <summary>
        /// Takes all values in List<string> and converts them to uppercase
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<string> AllToUpper(this List<string> values)
        {
            return values.Select(item => item.ToUpper()).ToList();
        }


        /// <summary>
        /// Takes all values in Hashset<string> and converts them to uppercase
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static HashSet<string> AllToUpper(this HashSet<string> values)
        {
            return values.Select(item => item.ToUpper()).ToHashSet();
        }


        /// <summary>
        /// Takes the results of a groupjoin and splits them between those that have at least one match and those that don't
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="groupedItems"></param>
        /// <returns></returns>
        public static Tuple<List<Tuple<T1, List<T2>>>, List<T1>> SplitGroupJoin<T1, T2>(this List<Tuple<T1, List<T2>>> groupedItems)
        {
            var hasMatches = new List<Tuple<T1, List<T2>>>();
            var noMatches = new List<T1>();

            foreach (var group in groupedItems)
            {
                if (group.Item2.Count == 0 || group.Item2 == null)
                {
                    noMatches.Add(group.Item1);
                }
                else
                {
                    hasMatches.Add(group);
                }
            }

            return new Tuple<List<Tuple<T1, List<T2>>>, List<T1>>(hasMatches, noMatches);
        }

        #endregion


        #region Create String Keys

        /// <summary>
        /// Replaces all spaces in a string with a specified delimiter and converts the string to uppercase
        /// </summary>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string AsKey(this string value, string delimiter = "_")
        {
            return Regex.Replace(value.ToUpper(), " ", delimiter);
        }


        /// <summary>
        /// Takes all values in a List<string>, converts them to uppercase and joins them on a given delimiter to create a key
        /// </summary>
        /// <param name="values"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string AsKey(this List<string> values, string delimiter = "_")
        {
            string outputKey = String.Join(delimiter, values.AllToUpper());

            return Regex.Replace(outputKey, " ", delimiter);
        }

        /// <summary>
        /// Takes all values in a Hashset<string>, converts them to uppercase and joins them on a given delimiter to create a key
        /// </summary>
        /// <param name="values"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string AsKey(this HashSet<string> values, string delimiter = "_")
        {
            return values.ToList().AsKey(delimiter);
        }

        #endregion


        #region Date Extensions




        /// <summary>
        /// Returns the earliest of two given dates
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static DateTime FindEarliest(this DateTime date1, DateTime date2)
        {
            int result = DateTime.Compare(date1, date2);


            // if the result is 1, date2 is the earliest date
            if (result == 1)
            {
                return date2;
            }

            // if result is 1, date1 is earliest date
            // if result is 0, date1 and date2 are the same, so it doesn't matter which is returned
            return date1;
        }


        /// <summary>
        /// Returns the latest of two given dates
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static DateTime FindLatest(this DateTime date1, DateTime date2)
        {
            int result = DateTime.Compare(date1, date2);

            // if the result is -1, date1 is less than date2, so return date2
            if (result == -1)
            {
                return date2;
            }


            // if the result is 1, date1 is greater than date 2
            // if the result is 0, date1 and date2 are the same, so it doesn't matter which is returned
            return date1;
        }

        #endregion


        public static string GenerateTempFileName(this FileInfo originalFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalFile.Name).AppendRandomId();
            var extension = originalFile.Extension;
            var tempFileName = $"{fileName}.{extension}";

            return tempFileName;
        }

        public static string AppendRandomId(this string text, char delimiter = '_')
        {
            var randomId = Guid.NewGuid().ToString();

            return $"{text}{delimiter}{randomId}";
        }


        public static string FormatPhoneNumber(this string phone)
        {
            var updatedPhoneNumber = phone;
            var dashIndexes = new List<int>();
            var numbersInString = Regex.Matches(phone, @"\d");

            switch (numbersInString.Count)
            {
                case 7:
                    dashIndexes.Add(2);
                    break;
                case 10:
                    dashIndexes = new() { 2, 5 };
                    break;
                case 11:
                    dashIndexes = new() { 0, 3, 6 };
                    break;
                default:
                    return updatedPhoneNumber;
            }

            var sb = new StringBuilder();

            for (int i = 0; i <= numbersInString.Count - 1; i++)
            {
                sb.Append(numbersInString[i].ToString());

                if (dashIndexes.Contains(i))
                {
                    sb.Append("-");
                }
            }

            updatedPhoneNumber = sb.ToString();

            return updatedPhoneNumber;
        }

        public static string RemoveNonNumericCharcters(this string text, bool keepDecimal = false)
        {
            var nonNumericRegex = new Regex(@"[^\d]");

            if (keepDecimal)
            {
                nonNumericRegex = new Regex(@"[^0-9.]");
            }
            

            return nonNumericRegex.Replace(text, "");
        }

        public static void OpenFileUsingDefaultApplication(this string filePath)
        {
            var file = new FileInfo(filePath);

            file.OpenFileUsingDefaultApplication();
        }

        public static void OpenFileUsingDefaultApplication(this FileInfo file)
        {
            if (file.Exists)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(file.FullName);
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
            }
        }
    }
}
