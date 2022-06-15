using UnityEngine;

namespace Utility
{
    public static class MathExtension
    {
        public static Vector3 MultiplyByValue(this Vector3 self, Vector3 multiplicator)
        {
            return new Vector3(
                self.x * multiplicator.x,
                self.y * multiplicator.y, 
                self.z * multiplicator.z);
        }
        
        public static Vector3 DivideByValue(this Vector3 self, Vector3 divider)
        {
            return new Vector3(
                self.x / divider.x,
                self.y / divider.y, 
                self.z / divider.z);
        }
        
        public static bool Between(this float num, int lower, int upper, bool lowerInclusive = false)
        {
            return lowerInclusive
                ? lower <= num && num < upper
                : lower < num && num < upper;
        }
    }
}