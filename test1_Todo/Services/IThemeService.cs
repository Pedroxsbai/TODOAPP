using Microsoft.AspNetCore.Http;

namespace test1_Todo.Services
{
    // Responsabilite : Definir le contrat pour la gestion du theme (light/dark)
    
    public interface IThemeService
    {
        string GetCurrentTheme(HttpContext context);
        string ToggleTheme(HttpContext context);
        void SetTheme(HttpContext context, string theme);
    }
}
