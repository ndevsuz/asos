namespace Easy.Extensions
{
    public static class StringExtensions
    {
        public static string PathToName(this string str) =>
            str.Split("\\").Last().Split(".").First();

        public static string NameToPlural(this string str) =>
            str.EndsWith("y") ? $"{str.Substring(0, str.Length - 1)}ies" : $"{str}s";

        //public static string NameToPlural(this string str) =>
        //    str.EndsWith("y") ? str.Replace("y", "ies") : $"{str}s";

        public static string ToLowFirstLetter(this string str) =>
            String.Concat(str.First().ToString().ToLower(), str.Substring(1));

        public static string AddCSharpExtension(this string str) =>
            $"{str}.cs";
    }
}
