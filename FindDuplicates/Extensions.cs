using System;

namespace FindDuplicates
{
    public static class Extensions
    {
        public static string Replace(this string s, string[] separators, string newVal)
        {
            string[] temp = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(newVal, temp);
        }
    }
}