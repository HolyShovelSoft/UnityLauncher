//using System;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//
//namespace UnityLauncher.Core
//{
//    public class SettingHolderConverter : JsonConverter
//    {
//        private Type targetObjectType;
//
//
//        public override bool CanWrite => false;
//
//        public override bool CanRead => targetObjectType == typeof(SettingHolder);
//
//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            throw new NotSupportedException();
//        }
//
//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            targetObjectType = objectType;
//            var jToken = JToken.ReadFrom(reader);
//            var a = jToken["ValueType"];
//            var type = a.ToObject<Type>();
//            if (type != null)
//            {
//                var targetType = typeof(SettingHolder<>).MakeGenericType(type);
//                var result = jToken.ToObject(targetType);
//                return result;
//            }
//            return null;
//        }
//
//        public override bool CanConvert(Type objectType)
//        {
//            return typeof(SettingHolder) == objectType;
//        }
//        
//    }
//}