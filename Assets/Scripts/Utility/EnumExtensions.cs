using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Utility
{
    public static class EnumExtensions
    {
        public static string FetchName<T>(this T enumType) where T : Enum
        {
            string fetchedName = Enum.GetName(typeof(T), enumType);
            if (fetchedName == null) return string.Empty;
        
            string returnString = fetchedName.DeepClone();
            for (int i = 0; i < fetchedName.Length; i++)
            {
                if (char.IsUpper(fetchedName, i) && i != 0)
                    returnString = returnString.Insert(i, " ");
            }

            return returnString;
        }
        
        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        
        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T) formatter.Deserialize(ms);
            }
        }
    }
}