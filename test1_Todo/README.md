# 📝 Todo Application – ASP.NET Core MVC

<div align="center">

**Application de gestion de tâches avec architecture SOLID**

</div>

---

## 📖 À propos du projet

Ce projet est une application Todo développée avec ASP.NET Core MVC.  
L'objectif n'était pas seulement de "faire marcher" l'application, mais surtout de structurer le code correctement, appliquer les principes SOLID, et comprendre pourquoi on fait certaines choses (services, filtres, DI…).

### ✨ L'application inclut :

- ✅ authentification basée sur les sessions
- ✅ gestion du thème dark / light
- ✅ logging des actions utilisateurs
- ✅ une architecture claire et maintenable

---

## 🎯 Objectif pédagogique du projet

Ce projet m'a permis de :

- 🧠 comprendre concrètement les principes SOLID
- 📚 apprendre à séparer les responsabilités
- 💉 utiliser correctement Dependency Injection
- 🎭 distinguer Controller / Service / Filter
- 📈 améliorer la lisibilité et l'évolutivité du code

> Le code a évolué progressivement : certaines parties ont été refactorisées après réflexion, ce qui m'a aidé à mieux comprendre les bonnes pratiques.

---

## 🔹 Principes SOLID appliqués

### 1️⃣ Single Responsibility Principle (SRP)

> Une classe doit avoir une seule responsabilité et une seule raison de changer.

#### 🔸 Exemple : Logging

Au début, le filtre de logging faisait trop de choses :

```
❌ LoggingFilter (ancienne version)
- Interception des requêtes
- Récupération des infos (user, controller, action)
- Gestion des fichiers
- Écriture du log
- Gestion du multi-threading
```

Après refactorisation :

```
✅ LoggingFilter
- Intercepte l'action
- Récupère les infos
- Délègue le log

✅ FileLoggingService
- Gère uniquement l'écriture du log
```

👉 Chaque classe a maintenant une responsabilité claire.

#### 🔸 Exemple : Gestion du thème

Même logique pour le thème :

```
ThemeController
→ gère uniquement la requête HTTP

ThemeService
→ contient la logique métier (toggle + cookies)

ThemeFilter
→ injecte automatiquement le thème dans les vues
```

**Résultat :**
- ✅ code plus lisible
- ✅ plus facile à modifier
- ✅ responsabilités bien séparées

---

### 2️⃣ Open / Closed Principle (OCP)

> Le code est ouvert à l'extension, mais fermé à la modification.

Exemple avec le logging :

```csharp
public class FileLoggingService : ILoggingService
{
    public void LogAction(string userName, string controller, string action)
    {  

    }
}
```

Aucun changement dans le filtre, seulement dans Program.cs :

```csharp
builder.Services.AddSingleton<ILoggingService, DatabaseLoggingService>();
```

---

### 3️⃣ Liskov Substitution Principle (LSP)

Toutes les implémentations de ILoggingService sont interchangeables :

```csharp
ILoggingService logger = new FileLoggingService();
ILoggingService logger = new DatabaseLoggingService();
```

Le LoggingFilter fonctionne sans savoir laquelle est utilisée.

---

### 4️⃣ Interface Segregation Principle (ISP)

Les interfaces sont simples et ciblées :

```csharp
public interface ILoggingService
{
    void LogAction(string userName, string controllerName, string actionName);
}
```

Pas de méthodes inutiles, chaque interface a un but précis.

---

### 5️⃣ Dependency Inversion Principle (DIP)

Les classes dépendent des interfaces, pas des implémentations concrètes.

❌ **Mauvais :**

```csharp
new FileLoggingService();
```

✅ **Bon :**

```csharp
public LoggingFilter(ILoggingService loggingService)
```

Cela rend le code :
- ✅ testable
- ✅ flexible
- ✅ moins couplé

---

## 🛠️ Bonnes pratiques utilisées

### 🔹 Dependency Injection (DI)

Toutes les dépendances sont déclarées dans Program.cs :

```csharp
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
```

**Choix des durées de vie :**
- **Singleton** → logging (une seule instance, thread-safe)
- **Scoped** → services liés à la requête HTTP

---

### 🔹 Service Layer Pattern

La logique métier est déplacée dans des services :

- **SessionManagerService** → gestion des sessions (JSON)
- **FileLoggingService** → logging thread-safe
- **ThemeService** → gestion du thème et des cookies

Les controllers restent simples et lisibles.

---

### 🔹 Filter Pattern

Utilisation des filtres pour les préoccupations transversales :

```csharp
[ServiceFilter(typeof(LoggingFilter))]
[ServiceFilter(typeof(ThemeFilter))]
[AuthFilres]
public class TodoController : Controller
```

Cela évite la duplication de code dans chaque action.

---

### 🔹 Thread Safety (Logging)

Le logging utilise un verrou statique :

```csharp
lock (_lockObject)
{
    File.AppendAllText(...);
}
```

Cela évite les conflits d'écriture lorsque plusieurs requêtes arrivent en même temps.

---

### 🔹 Sécurité des cookies

```csharp
HttpOnly = true
```

- ✅ protège contre l'accès JavaScript
- ✅ limite les risques XSS

---

## 📁 Structure du projet

```
Controllers/   → gestion HTTP
Services/      → logique métier
Filtres/       → logging, thème, auth
Models/        → entités
ViewModels/    → données pour les vues
Mappers/       → conversion VM ↔ Model
Logs/          → fichiers de log
```

Chaque dossier a un rôle clair.

---

## 🚀 Fonctionnalités principales

- ✅ Authentification par session
- ✅ Gestion des tâches Todo
- ✅ Dark / Light mode avec persistance
- ✅ Logging automatique des actions utilisateurs
- ✅ Architecture claire et maintenable

---

## 🎓 Ce que j'ai appris avec ce projet

- ✅ appliquer SOLID dans un vrai projet
- ✅ comprendre quand utiliser un service ou un filtre
- ✅ refactoriser un code existant
- ✅ écrire un code plus propre et plus professionnel
- ✅ penser en termes de responsabilités, pas seulement de fonctionnalités

---

## 🧠 Principe clé du projet

<div align="center">

> **Un bon code n'est pas seulement un code qui marche,**  
> **mais un code qui peut évoluer sans tout casser.**

</div>