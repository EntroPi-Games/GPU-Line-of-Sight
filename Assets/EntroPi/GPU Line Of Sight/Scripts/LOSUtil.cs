using UnityEngine;

namespace LOS
{
    public class Util
    {
        public static bool Verify(bool comparison, string message)
        {
            Debug.Assert(comparison, "Verify Failed: " + message);

            return comparison;
        }

        public static bool Verify(bool comparison)
        {
            Debug.Assert(comparison, "Verify Failed!");

            return comparison;
        }
    }
}