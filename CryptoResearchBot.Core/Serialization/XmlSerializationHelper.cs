using System.Xml;
using System.Xml.Serialization;

namespace CryptoResearchBot.Core.Serialization
{
    public static class XmlSerializationHelper
    {
        public static void SerializeToXml<T>(T obj, string fileName)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new XmlTextWriter(fileName, null))
            {
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, obj);
            }
        }

        public static T DeserializeFromXml<T>(string fileName)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new XmlTextReader(fileName))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
