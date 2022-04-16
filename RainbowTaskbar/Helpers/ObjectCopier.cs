using System.IO;
using System.Runtime.Serialization;

namespace RainbowTaskbar.Helpers;

public static class ObjectCopier {
    /// <summary>
    ///     Perform a deep copy of the object via serialization.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>A deep copy of the object.</returns>
    public static T DeepClone<T>(this T obj) {
        using (var stream = new MemoryStream()) {
            var dcs = new DataContractSerializer(typeof(T));
            dcs.WriteObject(stream, obj);
            stream.Position = 0;
            return (T) dcs.ReadObject(stream);
        }
    }
}