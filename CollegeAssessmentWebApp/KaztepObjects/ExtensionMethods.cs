using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq;
using System.Reflection;

namespace CollegeAssessmentWebApp
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Remove a number of characters from the end of a string
        /// </summary>
        public static string TrimChars(this string str, int count)
        {
            return str.Remove(str.Length - count);
        }

        /// <summary>
        /// Swaps object in list to new position
        /// </summary>
        public static void MoveItem(this List<object> list, int index, int moveIndex)
        {
            var temp = list[index];
            list.RemoveAt(index);
            list.Insert(moveIndex, temp);
        } 
    }
}