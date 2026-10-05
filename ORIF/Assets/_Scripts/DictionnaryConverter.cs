using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DictionaryConverter<TKey, TValue> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Dictionary<TKey, TValue>);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dictionary = (Dictionary<TKey, TValue>)value;
        writer.WriteStartObject();
        foreach (var kvp in dictionary)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            serializer.Serialize(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var dictionary = new Dictionary<TKey, TValue>();
        var jObject = JObject.Load(reader);

        foreach (var property in jObject.Properties())
        {
            var key = ConvertKey(property.Name);
            var value = DeserializeValue(property.Value, serializer);
            dictionary.Add(key, value);
        }

        return dictionary;
    }

    private TKey ConvertKey(string propertyName)
    {
        return (TKey)Convert.ChangeType(propertyName, typeof(TKey));
    }

    private TValue DeserializeValue(JToken token, JsonSerializer serializer)
    {
        return token.ToObject<TValue>(serializer);
    }
}

