using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace ECommerceApp.Helpers
{
    public static class SessionHelper
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetObjectAsJsonCookie(this HttpResponse response, string key, object value, int? expireTime = null)
        {
            CookieOptions option = new CookieOptions();
            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddDays(7);

            var json = JsonConvert.SerializeObject(value);
            response.Cookies.Append(key, json, option);
        }

        public static T GetObjectFromJsonCookie<T>(this HttpRequest request, string key)
        {
            request.Cookies.TryGetValue(key, out string value);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
