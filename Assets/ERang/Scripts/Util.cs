using System.Linq;

namespace ERang
{
    public static class Utils
    {
        public static int[] ParseIntArray(string intArray)
        {
            return intArray.Split(new[] { "," }, System.StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => int.Parse(s.Trim()))
                          .ToArray();
        }
    }
}