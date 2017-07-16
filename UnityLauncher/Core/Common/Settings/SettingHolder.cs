using Newtonsoft.Json;

namespace UnityLauncher.Core
{
    public class SettingHolder<T>
    {
        [JsonProperty]
        public T Value { get; set; }
    }
}