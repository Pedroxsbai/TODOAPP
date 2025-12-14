# 📚 Documentation Architecture - Todo Application

## 📋 Table des Matières
1. [Vue d'ensemble](#vue-densemble)
2. [Modifications apportées](#modifications-apportées)
3. [Principes SOLID appliqués](#principes-solid-appliqués)
4. [Design Patterns utilisés](#design-patterns-utilisés)
5. [Structure du projet](#structure-du-projet)
6. [Flux de données](#flux-de-données)

---

## 🎯 Vue d'ensemble

Cette application ASP.NET Core MVC implémente un système de gestion de tâches (Todo) avec authentification, gestion de sessions, et logging des actions utilisateurs. L'architecture suit les bonnes pratiques de développement logiciel avec une attention particulière aux principes SOLID et aux design patterns modernes.

---

## 🔧 Modifications apportées

### 1. **Système de Logging (Refactoring SRP)**

#### Fichiers créés :
- `Services/ILoggingService.cs` - Interface du service de logging
- `Services/FileLoggingService.cs` - Implémentation concrète pour le logging fichier

#### Fichiers modifiés :
- `Filtres/LoggingFilter.cs` - Refactorisé pour déléguer le logging au service
- `Program.cs` - Enregistrement du service de logging dans le conteneur DI
- `Controllers/AuthController.cs` - Utilisation de ServiceFilter au lieu d'attribut direct
- `Controllers/TodoController.cs` - Utilisation de ServiceFilter + suppression de ToggleTheme (SRP)

### 2. **Séparation des responsabilités des Controllers (SRP)**

#### Fichiers créés :
- `Controllers/ThemeController.cs` - Controller dédié à la gestion du thème UI

#### Fichiers modifiés :
- `Controllers/TodoController.cs` - Suppression de la méthode `ToggleTheme()` (violation SRP)
- `Views/Shared/_Layout.cshtml` - Mise à jour du lien vers `ThemeController.Toggle`

### 3. **Services de gestion de session**

#### Fichiers existants dans le projet :
- `Services/ISessionManagerService.cs` - Interface pour la gestion des sessions
- `Services/SessionManagerService.cs` - Implémentation de la gestion des sessions

### 4. **Filtres d'action**

#### Fichiers de filtres :
- `Filtres/LoggingFilter.cs` - Intercepte et enregistre les actions utilisateurs
- `Filtres/AuthFilres.cs` - Vérifie l'authentification avant l'accès aux ressources protégées
- `Filtres/ThemeFilter.cs` - Gestion du thème (light/dark mode)

---

## 🏛️ Principes SOLID appliqués

### **S - Single Responsibility Principle (SRP)** ✅

#### **Violation #1 : LoggingFilter**

**Problème initial :**
Le `LoggingFilter` avait deux responsabilités :
1. Intercepter les requêtes MVC et collecter les informations d'action
2. Écrire les logs dans le système de fichiers

**Solution appliquée :**
```
AVANT (violation SRP):
LoggingFilter → Collecte les données + Écrit dans le fichier

APRÈS (respect SRP):
LoggingFilter → Collecte les données → Délègue à ILoggingService
                                      ↓
                              FileLoggingService → Écrit dans le fichier
```

**Fichiers concernés :**
- `Filtres/LoggingFilter.cs` - Responsabilité : Interception MVC uniquement
- `Services/ILoggingService.cs` - Responsabilité : Contrat de logging
- `Services/FileLoggingService.cs` - Responsabilité : Écriture fichier uniquement

---

#### **Violation #2 : TodoController**

**Problème initial :**
Le `TodoController` gérait deux domaines distincts :
1. Gestion des tâches (Todo) - **Logique métier**
2. Gestion du thème UI (Toggle Light/Dark) - **Préférence utilisateur**

**Solution appliquée :**
```
AVANT (violation SRP):
TodoController
├── Index()           ← Gestion todos ✅
├── Add()             ← Gestion todos ✅
└── ToggleTheme()     ← Gestion UI ❌ (responsabilité différente !)

APRÈS (respect SRP):
TodoController              ThemeController
├── Index()                 └── Toggle()
└── Add()
```

**Fichiers concernés :**
- `Controllers/TodoController.cs` - Responsabilité : Gestion des tâches uniquement
- `Controllers/ThemeController.cs` - Responsabilité : Gestion du thème uniquement

**Bénéfices du SRP :**
- ✅ Chaque classe a une seule raison de changer
- ✅ Code plus facile à comprendre et maintenir
- ✅ Meilleure organisation et découplage
- ✅ Testabilité améliorée

---

#### **Violation #3 : ThemeController (Refactoring Approfondi)** 🔥

**Problème identifié :**
Même après avoir séparé `ToggleTheme()` dans son propre controller, une violation SRP subsiste:
- Le `ThemeController` manipule directement les cookies (logique de persistance)
- Le controller gère à la fois le routing HTTP ET la logique métier du thème

**Détection de la violation :**
```csharp
// ❌ VIOLATION SRP - Controller mélange routing et logique métier
public class ThemeController : Controller
{
    public IActionResult Toggle()
    {
        // RESPONSABILITÉ #1: Logique métier du thème
        var currentTheme = Request.Cookies["theme"] ?? "light";
        var newTheme = currentTheme == "light" ? "dark" : "light";
        
        // RESPONSABILITÉ #2: Persistance (manipulation cookies)
        Response.Cookies.Append("theme", newTheme, new CookieOptions {...});
        
        // RESPONSABILITÉ #3: Routing HTTP
        return Redirect(Request.Headers["Referer"].ToString() ?? "/");
    }
}
```

**Solution appliquée : Service Layer Pattern**

```
AVANT (SRP partiel):
ThemeController → Lit cookies + Toggle logic + Écrit cookies + Redirect

APRÈS (SRP complet):
ThemeController → IThemeService.ToggleTheme() → Redirect
                         ↓
                  ThemeService → Lit/Écrit cookies + Logique métier
```

**Fichiers créés :**
- `Services/IThemeService.cs` - Interface pour la gestion du thème
- `Services/ThemeService.cs` - Implémentation de la gestion du thème

**Code refactorisé :**

```csharp
// ✅ RESPECT SRP - Controller ne fait QUE du routing
public class ThemeController : Controller
{
    private readonly IThemeService _themeService;
    
    public ThemeController(IThemeService themeService)
    {
        _themeService = themeService;
    }
    
    public IActionResult Toggle()
    {
        // Délégation complète au service
        _themeService.ToggleTheme(HttpContext);
        
        // Controller = routing uniquement
        string referer = Request.Headers["Referer"].ToString();
        return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
    }
}
```

**ThemeService - Encapsulation complète de la logique :**

```csharp
public class ThemeService : IThemeService
{
    private const string ThemeCookieName = "theme";
    private const string DefaultTheme = "light";
    private const int CookieExpirationDays = 30;

    public string GetCurrentTheme(HttpContext context)
    {
        return context.Request.Cookies[ThemeCookieName] ?? DefaultTheme;
    }

    public string ToggleTheme(HttpContext context)
    {
        string currentTheme = GetCurrentTheme(context);
        string newTheme = currentTheme == "light" ? "dark" : "light";
        SetTheme(context, newTheme);
        return newTheme;
    }

    public void SetTheme(HttpContext context, string theme)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(CookieExpirationDays),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        };
        context.Response.Cookies.Append(ThemeCookieName, theme, cookieOptions);
    }
}
```

**Fichiers modifiés :**
- `Controllers/ThemeController.cs` - Refactorisé pour utiliser IThemeService
- `Filtres/ThemeFilter.cs` - Utilise IThemeService au lieu de lire directement le cookie
- `Program.cs` - Enregistrement de IThemeService et ThemeFilter dans le conteneur DI
- `Controllers/AuthController.cs` - Utilisation de ServiceFilter pour ThemeFilter
- `Controllers/TodoController.cs` - Utilisation de ServiceFilter pour ThemeFilter

**Bénéfices du refactoring :**
- ✅ **SRP complet** : Chaque classe a UNE seule responsabilité
- ✅ **Testabilité** : Service facilement mockable pour les tests
- ✅ **Réutilisabilité** : Service utilisable partout (controllers, filtres, views)
- ✅ **Maintenance** : Configuration centralisée (nom cookie, expiration, etc.)
- ✅ **Sécurité** : Options de cookies (Secure, HttpOnly, SameSite) centralisées

**Séparation claire des responsabilités :**

| Classe | Responsabilité Unique |
|--------|----------------------|
| `ThemeController` | Routing HTTP uniquement |
| `ThemeService` | Logique métier du thème + persistance cookies |
| `ThemeFilter` | Injection du thème dans ViewBag pour les vues |

---

### **O - Open/Closed Principle (OCP)** ✅

**Application :**
Le système est ouvert à l'extension mais fermé à la modification grâce à l'interface `ILoggingService`.

**Exemple :**
Pour ajouter un nouveau type de logging (ex: base de données), il suffit de créer une nouvelle classe :

```csharp
public class DatabaseLoggingService : ILoggingService
{
    public void LogAction(string userName, string controllerName, string actionName)
    {
        // Logique pour enregistrer dans une base de données
    }
}
```

Puis modifier l'enregistrement dans `Program.cs` :
```csharp
// Changement d'une seule ligne, aucune modification du filtre
builder.Services.AddSingleton<ILoggingService, DatabaseLoggingService>();
```

**Aucune modification nécessaire dans :**
- `LoggingFilter.cs`
- Les controllers
- Les autres services

---

### **L - Liskov Substitution Principle (LSP)** ✅

**Application :**
Toute implémentation de `ILoggingService` peut remplacer `FileLoggingService` sans briser l'application.

**Exemple :**
```csharp
// Ces deux implémentations sont interchangeables
ILoggingService service1 = new FileLoggingService();
ILoggingService service2 = new DatabaseLoggingService();
ILoggingService service3 = new CloudLoggingService();

// Le LoggingFilter fonctionne avec n'importe laquelle
```

---

### **I - Interface Segregation Principle (ISP)** ✅

**Application :**
Les interfaces sont petites et ciblées :

```csharp
// Interface minimale - une seule méthode avec une responsabilité claire
public interface ILoggingService
{
    void LogAction(string userName, string controllerName, string actionName);
}

// Interface pour la gestion de session - responsabilités spécifiques
public interface ISessionManagerService
{
    void Add(string Key, object obj, HttpContext context);
}
```

**Bénéfices :**
- Pas de méthodes inutilisées forcées sur les implémentations
- Interfaces faciles à implémenter et à comprendre

---

### **D - Dependency Inversion Principle (DIP)** ✅

**Application :**
Les classes de haut niveau (`LoggingFilter`) ne dépendent pas des classes de bas niveau (`FileLoggingService`), mais des abstractions (`ILoggingService`).

**Illustration :**
```csharp
// ❌ MAUVAIS - Dépendance directe (couplage fort)
public class LoggingFilter : ActionFilterAttribute
{
    private readonly FileLoggingService _logger = new FileLoggingService();
}

// ✅ BON - Dépendance sur l'abstraction (couplage faible)
public class LoggingFilter : ActionFilterAttribute
{
    private readonly ILoggingService _loggingService;
    
    public LoggingFilter(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }
}
```

**Configuration dans Program.cs :**
```csharp
// Injection de dépendances - le conteneur gère les dépendances
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
builder.Services.AddScoped<LoggingFilter>();
```

---

## 🎨 Design Patterns utilisés

### 1. **Dependency Injection (DI) Pattern** 🔥

**Définition :**
Un pattern qui permet d'injecter les dépendances d'une classe depuis l'extérieur plutôt que de les créer en interne.

**Utilisation dans le projet :**

#### A. Dans les Controllers
```csharp
public class AuthController : Controller
{
    ISessionManagerService session;
    
    // DI via constructeur
    public AuthController(ISessionManagerService session)
    {
        this.session = session;
    }
}
```

#### B. Dans les Filtres
```csharp
public class LoggingFilter : ActionFilterAttribute
{
    private readonly ILoggingService _loggingService;
    
    // DI via constructeur
    public LoggingFilter(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }
}
```

#### C. Configuration dans Program.cs
```csharp
// Enregistrement des services dans le conteneur DI
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
builder.Services.AddScoped<LoggingFilter>();
```

**Bénéfices :**
- ✅ Testabilité (on peut injecter des mocks)
- ✅ Découplage (pas de `new` dans les classes)
- ✅ Flexibilité (changement facile d'implémentation)
- ✅ Gestion automatique du cycle de vie des objets

---

### 2. **Service Layer Pattern** 🔥

**Définition :**
Encapsulation de la logique métier dans des services réutilisables.

**Utilisation dans le projet :**

#### A. SessionManagerService
```csharp
public class SessionManagerService : ISessionManagerService
{
    public void Add(string Key, Object obj, HttpContext context) 
    {
        // Logique de sérialisation et stockage en session
        string chaine = JsonSerializer.Serialize(obj);
        context.Session.SetString(Key, chaine);
    }
}
```

**Responsabilité :** Gérer la complexité de la sérialisation JSON et du stockage en session.

#### B. FileLoggingService
```csharp
public class FileLoggingService : ILoggingService
{
    public void LogAction(string userName, string controllerName, string actionName)
    {
        // Logique de logging dans un fichier
        // Gestion de la concurrence, création de dossiers, etc.
    }
}
```

**Responsabilité :** Encapsuler toute la logique de logging fichier.

**Bénéfices :**
- ✅ Réutilisabilité du code
- ✅ Séparation des préoccupations
- ✅ Facilite les tests unitaires
- ✅ Logique métier centralisée

---

### 3. **Strategy Pattern** 🔥

**Définition :**
Permet de définir une famille d'algorithmes, de les encapsuler et de les rendre interchangeables.

**Utilisation implicite avec ILoggingService :**

```csharp
// L'interface définit la stratégie
public interface ILoggingService
{
    void LogAction(string userName, string controllerName, string actionName);
}

// Stratégie 1: Logging dans un fichier
public class FileLoggingService : ILoggingService { ... }

// Stratégie 2 (potentielle): Logging dans une base de données
public class DatabaseLoggingService : ILoggingService { ... }

// Stratégie 3 (potentielle): Logging vers le cloud
public class CloudLoggingService : ILoggingService { ... }
```

**Le client (LoggingFilter) utilise la stratégie sans connaître l'implémentation :**
```csharp
public class LoggingFilter : ActionFilterAttribute
{
    private readonly ILoggingService _loggingService; // N'importe quelle stratégie
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Utilise la stratégie configurée
        _loggingService.LogAction(userName, controllerName, actionName);
    }
}
```

**Bénéfices :**
- ✅ Algorithmes interchangeables à runtime ou au démarrage
- ✅ Ajout de nouvelles stratégies sans modifier le code existant
- ✅ Respect du principe Open/Closed

---

### 4. **Filter Pattern (ASP.NET Core)** 🔥

**Définition :**
Pattern permettant d'intercepter les requêtes HTTP avant/après leur traitement.

**Utilisation dans le projet :**

#### A. LoggingFilter
```csharp
public class LoggingFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Code exécuté AVANT l'action du controller
        _loggingService.LogAction(userName, controllerName, actionName);
    }
}
```

#### B. AuthFilres
```csharp
public class AuthFilres : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Vérification de l'authentification
        if (context.HttpContext.Session.GetString("IsConnected") != "True")
        {
            context.Result = new RedirectToActionResult("Inscription", "Auth", null);
        }
    }
}
```

**Application sur les controllers :**
```csharp
[ServiceFilter(typeof(LoggingFilter))]  // Avec DI
[AuthFilres]                             // Sans DI (pas de dépendances)
[ThemeFilter]                            // Gestion du thème
public class TodoController : Controller
{
    // Toutes les actions passent par ces filtres
}
```

**Bénéfices :**
- ✅ Cross-cutting concerns (logging, auth, etc.)
- ✅ Code réutilisable
- ✅ Séparation des préoccupations
- ✅ Application déclarative avec attributs

---

### 5. **Repository Pattern (implicite avec Session)** 🔥

**Définition :**
Abstraction de l'accès aux données.

**Utilisation avec SessionManagerService :**

Le `SessionManagerService` agit comme un repository pour les données en session :

```csharp
// Au lieu d'accéder directement à la session partout
HttpContext.Session.SetString("key", JsonSerializer.Serialize(obj));

// On utilise le service (abstraction)
sessionService.Add("key", obj, HttpContext);
```

**Bénéfices :**
- ✅ Abstraction du stockage (session, cache, DB, etc.)
- ✅ Logique de sérialisation centralisée
- ✅ Facilite le changement de stockage

---

### 6. **Mapper Pattern** 🔥

**Définition :**
Transforme un objet d'un type vers un autre type.

**Utilisation dans le projet :**

#### AuthMapper
```csharp
public class AuthMapper
{
    public static Auth GetAuthFromAuthVM(AuthVM VM)
    {
        return new Auth 
        { 
            Nom = VM.Nom,
            Email = VM.Email,
            Password = VM.Password
        };
    }
}
```

#### TodoMapper
```csharp
public class TodoMapper
{
    public static Todo GetTodoFromAddTodoVM(todoAddVM vm)
    {
        // Transformation ViewModel → Model
    }
}
```

**Bénéfices :**
- ✅ Séparation View/Model/ViewModel
- ✅ Transformations centralisées
- ✅ Code réutilisable
- ✅ Évite la duplication de logique de mapping

---

### 7. **Singleton Pattern (via DI)** 🔥

**Utilisation :**
```csharp
// Le FileLoggingService est enregistré comme Singleton
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
```

**Raison :**
- Une seule instance pour toute l'application
- Nécessaire pour la gestion thread-safe de l'écriture fichier (`lock`)
- Performance optimale (pas de création/destruction constante)

**Thread Safety :**
```csharp
public class FileLoggingService : ILoggingService
{
    private static readonly object _lockObject = new object();
    
    private void WriteToFile(string logEntry)
    {
        lock (_lockObject)  // Protection contre les accès concurrents
        {
            File.AppendAllText(logPath, logEntry + Environment.NewLine);
        }
    }
}
```

---

## 🏗️ Structure du projet

```
test1_Todo/
│
├── Controllers/
│   ├── AuthController.cs          # Gestion authentification
│   ├── TodoController.cs          # Gestion des tâches (SRP - todos uniquement)
│   └── ThemeController.cs         # Gestion du thème UI (SRP - séparé de Todo)
│
├── Filtres/
│   ├── AuthFilres.cs              # Filtre d'authentification
│   ├── LoggingFilter.cs           # Filtre de logging (refactorisé - SRP)
│   └── ThemeFilter.cs             # Filtre de thème
│
├── Services/
│   ├── ISessionManagerService.cs  # Interface gestion session
│   ├── SessionManagerService.cs   # Implémentation session
│   ├── ILoggingService.cs         # Interface logging (nouveau - SRP)
│   └── FileLoggingService.cs      # Implémentation logging fichier (nouveau - SRP)
│
├── Models/
│   ├── Auth.cs                    # Modèle authentification
│   ├── Todo.cs                    # Modèle tâche
│   └── ErrorViewModel.cs          # Modèle erreur
│
├── ViewModels/
│   ├── AuthVM.cs                  # ViewModel authentification
│   └── TodoAddVM.cs               # ViewModel ajout tâche
│
├── Mappers/
│   ├── AuthMapper.cs              # Mapper Auth
│   └── TodoMapper.cs              # Mapper Todo
│
├── Views/
│   ├── Auth/
│   │   └── Inscription.cshtml
│   ├── Todo/
│   │   ├── Index.cshtml
│   │   └── Add.cshtml
│   └── Shared/
│
├── Logs/                          # Dossier de logs (créé automatiquement)
│   └── actions.log                # Fichier de logs des actions
│
└── Program.cs                     # Configuration DI et middleware
```

---

## 🔄 Flux de données

### 1. **Flux d'authentification**

```
Utilisateur → Inscription.cshtml
           ↓
AuthController.Inscription(AuthVM vm)
           ↓
AuthMapper.GetAuthFromAuthVM()  ← Mapper Pattern
           ↓
SessionManagerService.SetString()  ← Service Layer Pattern
           ↓
HttpContext.Session
           ↓
RedirectToAction("Index", "Todo")
```

### 2. **Flux de logging**

```
Requête HTTP → TodoController.Index()
                      ↓
              [ServiceFilter(typeof(LoggingFilter))]  ← Filter Pattern
                      ↓
              LoggingFilter.OnActionExecuting()
                      ↓
              ILoggingService.LogAction()  ← DI Pattern + Strategy Pattern
                      ↓
              FileLoggingService.LogAction()  ← SRP
                      ↓
              WriteToFile() [Thread-Safe]  ← Singleton Pattern
                      ↓
              Logs/actions.log
```

### 3. **Flux de protection des routes**

```
Requête HTTP → TodoController.Index()
                      ↓
              [AuthFilres]  ← Filter Pattern
                      ↓
              AuthFilres.OnActionExecuting()
                      ↓
         Session.GetString("IsConnected") == "True" ?
                      ↓
              OUI → Continue vers l'action
                      ↓
              NON → RedirectToAction("Inscription", "Auth")
```

---

## 📊 Durées de vie des services (DI Lifetimes)

```csharp
// SINGLETON - Une seule instance pour toute l'application
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
// ✅ Parfait pour le logging fichier (thread-safe avec lock)

// SCOPED - Une instance par requête HTTP
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
builder.Services.AddScoped<LoggingFilter>();
// ✅ Parfait pour les services qui interagissent avec HttpContext

// TRANSIENT - Une nouvelle instance à chaque injection
// (Non utilisé dans ce projet)
```

---

## 🎯 Résumé des bonnes pratiques appliquées

### ✅ Architecture propre
- Séparation des responsabilités (SRP)
- Découplage via interfaces (DIP)
- Extension sans modification (OCP)

### ✅ Design Patterns
- Dependency Injection
- Service Layer
- Strategy Pattern
- Filter Pattern
- Mapper Pattern
- Singleton Pattern
- Repository Pattern (implicite)

### ✅ Principes SOLID
- **S**ingle Responsibility
- **O**pen/Closed
- **L**iskov Substitution
- **I**nterface Segregation
- **D**ependency Inversion

### ✅ Avantages obtenus
- 🧪 **Testabilité** : Toutes les dépendances peuvent être mockées
- 🔧 **Maintenabilité** : Code organisé et facile à comprendre
- 🚀 **Évolutivité** : Facile d'ajouter de nouvelles fonctionnalités
- 🔄 **Réutilisabilité** : Services et filtres réutilisables
- 🛡️ **Robustesse** : Thread-safety, gestion d'erreurs

---

## 📖 Références

- [ASP.NET Core Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Design Patterns](https://refactoring.guru/design-patterns)
- [ASP.NET Core Filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)

---

**Date de création :** 13 Décembre 2025  
**Version :** 1.0  
**Auteur :** Architecture refactorisée selon les principes SOLID
