using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using test1_Todo.Models;

namespace test1_Todo.Services
{
    public class SessionManagerService : ISessionManagerService
    {
        public void SetString(string Key, string value, HttpContext context) 
        {
            context.Session.SetString(Key, value);
        }

        public string GetString(string Key, HttpContext context)
        {
            return context.Session.GetString(Key);
        }

        public void AddObject(string Key, Object obj, HttpContext context) 
        {
            // enregistrement dans la session
            // pour enregistrer une list dans la session il faut la serialiser et la rendre sous forme d'un json 
            string chaine = JsonSerializer.Serialize(obj);
            context.Session.SetString(Key, chaine);
        }

        public T GetObject<T>(string Key, HttpContext context)
        {
            var json = context.Session.GetString(Key);
            return json == null ? default(T) : JsonSerializer.Deserialize<T>(json);
        }
    }
}
