using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.Text;

public class ObjectSaveLoad
{
    public static void JsonSave<T>(T objectContainer, string filename)
    {
        var serializer = JsonConvert.SerializeObject(objectContainer);
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            byte[] info = new UTF8Encoding(true).GetBytes(serializer);
            stream.Write(info, 0, info.Length);
        }
    }

    public static T JsonLoad<T>(string filename)
    {
        using (var stream = new StreamReader(filename, Encoding.UTF8))
        {
            string text = stream.ReadToEnd();
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
