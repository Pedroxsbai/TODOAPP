using Microsoft.AspNetCore.Http;
using System;

namespace test1_Todo.Services
{

    public class ThemeService : IThemeService
    {
        private const string ThemeCookieName = "theme"; // nom du cookie pour stocker le theme ( cnst bcs on evite les fautes de frapep)
        private const string DefaultTheme = "light";
        private const int CookieExpirationDays = 30;

 
        public string GetCurrentTheme(HttpContext context)
        {
            
            return context.Request.Cookies[ThemeCookieName] ?? DefaultTheme; // Lit le cookie "theme", si n existe pas retourne "light"
        }
        public string ToggleTheme(HttpContext context)
        {
            string currentTheme = GetCurrentTheme(context);
            
            string newTheme = currentTheme == "light" ? "dark" : "light"; // si theme = light then curenttheme = dark wal3aks sa7i7 
            
            SetTheme(context, newTheme); // on sauvgarde ke nouveau theme dans le cookie

            return newTheme;
        }

        public void SetTheme(HttpContext context, string theme)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(CookieExpirationDays), // Expire dans 30 jours
                HttpOnly = true,  // securite Xss ( bon pratique)
            };
            
            context.Response.Cookies.Append(ThemeCookieName, theme, cookieOptions);
        }
    }
}
