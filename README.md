# Todo Application - ASP.NET Core MVC

<div align="center">


</div>

---

## A propos du projet

Ce projet est une application Todo developpee avec ASP.NET Core MVC.  
L'objectif n'etait pas seulement de "faire marcher" l'application, mais surtout de structurer le code correctement, appliquer les principes SOLID, et comprendre pourquoi on fait certaines choses (services, filtres, DI...).

### L'application inclut :

- authentification basee sur les sessions
- gestion du theme dark / light
- logging des actions utilisateurs
- une architecture claire et maintenable

---

## Objectif pedagogique du projet

Ce projet m'a permis de :

- comprendre concretement les principes SOLID
- apprendre a separer les responsabilites
- utiliser correctement Dependency Injection
- distinguer Controller / Service / Filter
- ameliorer la lisibilite et l'evolutivite du code

> Le code a evolue progressivement : certaines parties ont ete refactorisees apres reflexion, ce qui m'a aide a mieux comprendre les bonnes pratiques.

---

## Principes SOLID appliques

### 1. Single Responsibility Principle (SRP)

> Une classe doit avoir une seule responsabilite et une seule raison de changer.

#### Exemple : Logging

Au debut, le filtre de logging faisait trop de choses :

```
LoggingFilter (ancienne version)
- Interception des requetes
- Recuperation des infos (user, controller, action)
- Gestion des fichiers
- Ecriture du log
- Gestion du multi-threading
```

Apres refactorisation :

```
LoggingFilter
- Intercepte l'action
- Recupere les infos
- Delegue le log

FileLoggingService
- Gere uniquement l'ecriture du log
```

Chaque classe a maintenant une responsabilite claire.

#### Exemple : Gestion du theme

Meme logique pour le theme :

```
ThemeController
-> gere uniquement la requete HTTP

ThemeService
-> contient la logique metier (toggle + cookies)

ThemeFilter
-> injecte automatiquement le theme dans les vues
```

**Resultat :**
- code plus lisible
- plus facile a modifier
- responsabilites bien separees

---

### 2. Open / Closed Principle (OCP)

> Le code est ouvert a l'extension, mais ferme a la modification.

L'architecture permet d'ajouter de nouvelles implementations de logging (par exemple pour une base de donnees) sans modifier le code existant des filtres. Il suffirait de creer une nouvelle classe implementant `ILoggingService` et de changer l'injection dans `Program.cs`.

---

### 3. Liskov Substitution Principle (LSP)

`FileLoggingService` respecte le contrat de l'interface `ILoggingService`. Il peut etre utilise partout ou l'interface est attendue sans casser le fonctionnement de l'application.

Le LoggingFilter ne connait pas l'implementation concrete, il utilise simplement l'interface.

---

### 4. Interface Segregation Principle (ISP)

Les interfaces sont simples et ciblees :

```csharp
public interface ILoggingService
{
    void LogAction(string userName, string controllerName, string actionName);
}
```

Pas de methodes inutiles, chaque interface a un but precis.

---

### 5. Dependency Inversion Principle (DIP)

Les classes dependent des interfaces, pas des implementations concretes.

**Mauvais :**

```csharp
new FileLoggingService();
```

**Bon :**

```csharp
public LoggingFilter(ILoggingService loggingService)
```

Cela rend le code :
- testable
- flexible
- moins couple

---

## Bonnes pratiques utilisees

### Dependency Injection (DI)

Toutes les dependances sont declarees dans Program.cs :

```csharp
builder.Services.AddSingleton<ILoggingService, FileLoggingService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
```

**Choix des durees de vie :**
- **Singleton** -> logging (une seule instance, thread-safe)
- **Scoped** -> services lies a la requete HTTP

---

### Service Layer Pattern

La logique metier est deplacee dans des services :

- **SessionManagerService** -> gestion des sessions (JSON)
- **FileLoggingService** -> logging thread-safe
- **ThemeService** -> gestion du theme et des cookies

Les controllers restent simples et lisibles.

---

### Filter Pattern

Utilisation des filtres pour les preoccupations transversales :

```csharp
[ServiceFilter(typeof(LoggingFilter))]
[ServiceFilter(typeof(ThemeFilter))]
[AuthFilres]
public class TodoController : Controller
```

Cela evite la duplication de code dans chaque action.

---

### Thread Safety (Logging)

Le logging utilise un verrou statique :

```csharp
lock (_lockObject)
{
    File.AppendAllText(...);
}
```

Cela evite les conflits d'ecriture lorsque plusieurs requetes arrivent en meme temps.

---

### Securite des cookies

```csharp
HttpOnly = true
```

- protege contre l'acces JavaScript
- limite les risques XSS

---

## Structure du projet

```
Controllers/   -> gestion HTTP
Services/      -> logique metier
Filtres/       -> logging, theme, auth
Models/        -> entites
ViewModels/    -> donnees pour les vues
Mappers/       -> conversion VM <-> Model
Logs/          -> fichiers de log
```

Chaque dossier a un role clair.

---

## Fonctionnalites principales

- Authentification par session
- Gestion des taches Todo
- Dark / Light mode avec persistance
- Logging automatique des actions utilisateurs
- Dockerisation complete de l'application (Dockerfile + docker-compose)
- Architecture claire et maintenable

---

## Ce que j'ai appris avec ce projet

- appliquer SOLID dans un vrai projet
- comprendre quand utiliser un service ou un filtre
- refactoriser un code existant
- ecrire un code plus propre et plus professionnel
- penser en termes de responsabilites, pas seulement de fonctionnalites
- creer un conteneur et deployer l'application sur Docker

---

## Principe cle du projet

<div align="center">

> **Un bon code n'est pas seulement un code qui marche,**  
> **mais un code qui peut evoluer sans tout casser.**

</div>
