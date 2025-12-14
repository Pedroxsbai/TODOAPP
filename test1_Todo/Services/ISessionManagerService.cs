using Microsoft.AspNetCore.Http;
using System;
namespace test1_Todo.Services
{
    public interface ISessionManagerService 
    {
         public void SetString(string Key, string value, HttpContext context); // pour les simples types qui necessit pas la serialisation
        public string GetString(string Key, HttpContext context); 
         
         public void AddObject(string Key, object obj, HttpContext context); // pour les objets complexes qui necessit la serialisation
        public T GetObject<T>(string Key, HttpContext context); // T est un type generique 
    }
}
