using System.IO;
using Newtonsoft.Json;
using ProtoBuf.Meta;

namespace Laobian.Common.Base
{
    /// <summary>
    /// Helpers related to serialization
    /// </summary>
    public class SerializeHelper
    {
        private static readonly RuntimeTypeModel Serializer; // custom ProtoBuf instance

        static SerializeHelper()
        {
            Serializer = TypeModel.Create();
            Serializer.UseImplicitZeroDefaults = false;
        }

        /// <summary>
        /// Serialize object to JSON string
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="obj">The given object</param>
        /// <param name="indented">Indicates whether JSON should be intended, true means yes, otherwise no.</param>
        /// <returns>JSON string</returns>
        public static string ToJson<T>(T obj, bool indented = false)
        {
            return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None);
        }

        /// <summary>
        /// Deserialize to object from JSON string
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="json">The given string</param>
        /// <returns>Deserialized object</returns>
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Serialize object to Google ProtoBuf binary
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="obj">The given object</param>
        /// <returns>ProtoBuf binary stream</returns>
        public static Stream ToProtobuf<T>(T obj)
        {
            var ms = new MemoryStream();
            Serializer.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <summary>
        /// Deserialize to object from Google ProtoBuf binary
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="stream">The given ProtoBuf binary stream</param>
        /// <returns>Deserialized object</returns>
        public static T FromProtobuf<T>(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return (T)Serializer.Deserialize(stream, null, typeof(T));
        }
    }
}
