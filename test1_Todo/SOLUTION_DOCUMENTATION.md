# Session Authentication - Problem & Solution Documentation

## Problem Description

### The Issue
When a user completed the inscription form, they were being redirected back to the inscription page instead of accessing the `/todo/index` page, even though the session was being set in the controller.

### Root Cause
The problem was in the **SessionManagerService**. The `Add()` method was serializing **all values** to JSON, including simple strings:

```csharp
public void Add(string Key, Object obj, HttpContext context) 
{
    string chaine = JsonSerializer.Serialize(obj);
    context.Session.SetString(Key, chaine);
}
```

When setting `IsConnected` to `"True"`, the JSON serialization converted it to `"\"True\""` (with escaped quotes).

However, the `AuthFilres` filter was checking for the exact string `"True"`:

```csharp
var isConnected = context.HttpContext.Session.GetString("IsConnected");
if (string.IsNullOrEmpty(isConnected) || isConnected != "True")
{
    // Redirect to inscription
}
```

Since `"\"True\""` ≠ `"True"`, the authentication always failed.

---

## Solution Implemented

### Fix Applied
Changed the `AuthController` to use `HttpContext.Session.SetString()` directly for simple string values:

```csharp
// Before (INCORRECT):
session.Add("UserName", auth.Nom, HttpContext);
session.Add("IsConnected", "True", HttpContext);

// After (CORRECT):
HttpContext.Session.SetString("UserName", auth.Nom);
HttpContext.Session.SetString("IsConnected", "True");
```

### Additional Improvement
Updated the `AuthFilres` to be more explicit about null checks:

```csharp
var isConnected = context.HttpContext.Session.GetString("IsConnected");
if (string.IsNullOrEmpty(isConnected) || isConnected != "True")
{
    context.Result = new RedirectToActionResult("Inscription", "Auth", null);
}
```

---

## SOLID Principles Analysis & Recommendations

### Current Issues

#### 1. **Single Responsibility Principle (SRP) - VIOLATED**

**Problem Areas:**

**a) SessionManagerService has unclear responsibility**
- Current implementation serializes everything to JSON
- Should it handle simple strings differently than complex objects?
- Mixing concerns: serialization logic + session management

**b) AuthController violates SRP**
- Handles HTTP requests
- Manages session directly
- Contains authentication logic

**c) TodoController violates SRP**
- Handles HTTP requests  
- Manages TODO operations
- Manages session for TODO list storage

---

### Recommended Improvements

#### 1. **Refactor SessionManagerService (SRP Fix)**

Create separate methods for different data types:

```csharp
public interface ISessionManagerService
{
    // For simple string values
    void SetString(string key, string value, HttpContext context);
    string GetString(string key, HttpContext context);
    
    // For complex objects that need serialization
    void SetObject<T>(string key, T obj, HttpContext context);
    T GetObject<T>(string key, HttpContext context);
}

public class SessionManagerService : ISessionManagerService
{
    public void SetString(string key, string value, HttpContext context)
    {
        context.Session.SetString(key, value);
    }
    
    public string GetString(string key, HttpContext context)
    {
        return context.Session.GetString(key);
    }
    
    public void SetObject<T>(string key, T obj, HttpContext context)
    {
        string json = JsonSerializer.Serialize(obj);
        context.Session.SetString(key, json);
    }
    
    public T GetObject<T>(string key, HttpContext context)
    {
        var json = context.Session.GetString(key);
        return json == null ? default(T) : JsonSerializer.Deserialize<T>(json);
    }
}
```

#### 2. **Create an Authentication Service (SRP Fix)**

Extract authentication logic from the controller:

```csharp
public interface IAuthenticationService
{
    void Login(string userName, HttpContext context);
    void Logout(HttpContext context);
    bool IsAuthenticated(HttpContext context);
    string GetCurrentUserName(HttpContext context);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ISessionManagerService _sessionManager;
    
    public AuthenticationService(ISessionManagerService sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    public void Login(string userName, HttpContext context)
    {
        _sessionManager.SetString("UserName", userName, context);
        _sessionManager.SetString("IsConnected", "True", context);
    }
    
    public void Logout(HttpContext context)
    {
        context.Session.Clear();
    }
    
    public bool IsAuthenticated(HttpContext context)
    {
        var isConnected = _sessionManager.GetString("IsConnected", context);
        return !string.IsNullOrEmpty(isConnected) && isConnected == "True";
    }
    
    public string GetCurrentUserName(HttpContext context)
    {
        return _sessionManager.GetString("UserName", context);
    }
}
```

#### 3. **Create a Todo Repository/Service (SRP Fix)**

Separate TODO data management from the controller:

```csharp
public interface ITodoService
{
    List<Todo> GetAll(HttpContext context);
    void Add(Todo todo, HttpContext context);
}

public class TodoService : ITodoService
{
    private readonly ISessionManagerService _sessionManager;
    private const string TodoSessionKey = "todos";
    
    public TodoService(ISessionManagerService sessionManager)
    {
        _sessionManager = sessionManager;
    }
    
    public List<Todo> GetAll(HttpContext context)
    {
        return _sessionManager.GetObject<List<Todo>>(TodoSessionKey, context) 
               ?? new List<Todo>();
    }
    
    public void Add(Todo todo, HttpContext context)
    {
        var todos = GetAll(context);
        todos.Add(todo);
        _sessionManager.SetObject(TodoSessionKey, todos, context);
    }
}
```

#### 4. **Refactored Controllers (Clean & Focused)**

**AuthController:**
```csharp
public class AuthController : Controller
{
    private readonly IAuthenticationService _authService;
    
    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }
    
    public IActionResult Inscription()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Inscription(AuthVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        
        Auth auth = AuthMapper.GetAuthFromAuthVM(vm);
        _authService.Login(auth.Nom, HttpContext);
        
        return RedirectToAction("Index", "Todo");
    }
}
```

**TodoController:**
```csharp
public class TodoController : Controller
{
    private readonly ITodoService _todoService;
    
    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }
    
    [AuthFilres]
    public IActionResult Index()
    {
        var todos = _todoService.GetAll(HttpContext);
        return View(todos);
    }
    
    public IActionResult Add()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Add(todoAddVM vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        
        Todo todo = TodoMapper.GetTodoFromAddTodoVM(vm);
        _todoService.Add(todo, HttpContext);
        
        return RedirectToAction(nameof(Index));
    }
}
```

#### 5. **Update Dependency Injection in Program.cs**

```csharp
// Add these service registrations
builder.Services.AddScoped<ISessionManagerService, SessionManagerService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITodoService, TodoService>();
```

---

## Other SOLID Principles

### Open/Closed Principle (OCP)
✅ **Current Status:** Good
- The filter system (`AuthFilres`) allows extending authentication without modifying existing code
- Can create new filters without changing existing ones

### Liskov Substitution Principle (LSP)
✅ **Current Status:** Good
- Interfaces are properly used for dependency injection

### Interface Segregation Principle (ISP)
⚠️ **Improvement Needed:**
- Create smaller, focused interfaces instead of large ones
- Current `ISessionManagerService` should be split (as shown above)

### Dependency Inversion Principle (DIP)
✅ **Current Status:** Good
- Controllers depend on interfaces, not concrete implementations
- Dependency injection is properly configured

---

## Implementation Updates

### 1. SessionManagerService Refactoring (SRP Compliance) ✅

**Date:** 2025-12-12

**Problem:** The original `SessionManagerService` had a single `Add()` method that serialized everything to JSON, which was incorrect for simple string values.

**Solution Implemented:**

Updated `ISessionManagerService` interface:
```csharp
public interface ISessionManagerService 
{
     // For simple string values (no serialization)
     public void SetString(string Key, string value, HttpContext context);
     public string GetString(string Key, HttpContext context);
     
     // For complex objects that need JSON serialization
     public void AddObject(string Key, object obj, HttpContext context);
     public T GetObject<T>(string Key, HttpContext context);
}
```

**Benefits:**
- ✅ Respects Single Responsibility Principle - clear separation between string and object handling
- ✅ No more serialization issues with simple string values
- ✅ Type-safe generic methods for complex objects
- ✅ Better API clarity - method names indicate their purpose

**Files Modified:**
- `Services/ISessionManagerService.cs` - Added new method signatures
- `Services/SessionManagerService.cs` - Implemented all four methods
- `Controllers/AuthController.cs` - Updated to use `session.SetString()`
- `Controllers/TodoController.cs` - Updated to use `session.GetObject<T>()` and `session.AddObject()`

---

### 2. Todo List Display Implementation ✅

**Date:** 2025-12-12

**Feature:** Display todos in a table on the `/todo/index` page.

**Changes Made:**

**Controller Update (`TodoController.cs`):**
```csharp
[AuthFilres]
public IActionResult Index()
{
    // Get todos from session or return empty list
    List<Todo> todos = session.GetObject<List<Todo>>("todos", HttpContext) ?? new List<Todo>();
    return View(todos);
}
```

**View Update (`Views/Todo/Index.cshtml`):**
- Added `@model List<test1_Todo.Models.Todo>` directive
- Created a Bootstrap table to display todos with columns: Libelle, Description, Date Limite, Status
- Added empty state message when no todos exist
- Added "Add New Todo" button for easy access

**SRP Compliance:**
- Controller's responsibility: retrieve data from session service and pass to view
- View's responsibility: display the data in a user-friendly format
- Session service's responsibility: manage session storage/retrieval

**Bug Fix:** Also fixed a bug in the `Add` action where the new todo wasn't being added to the list before saving to session.

---

### 3. Dark Theme with Cookie-Based Persistence ✅

**Date:** 2025-12-12

**Feature:** Toggle between light and dark themes with cookie persistence (30 days).

**Changes Made:**

#### New Filter: `ThemeFilter.cs`
```csharp
public class ThemeFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var theme = context.HttpContext.Request.Cookies["theme"] ?? "light";
        if (context.Controller is Controller controller)
        {
            controller.ViewBag.Theme = theme;
        }
    }
}
```

**Responsibility:** Read theme cookie and pass to ViewBag for all views.

#### Controller Updates:
- **AuthController** - Added `[ThemeFilter]` attribute
- **TodoController** - Added `[ThemeFilter]` attribute and `ToggleTheme()` action

**ToggleTheme Action:**
```csharp
public IActionResult ToggleTheme()
{
    var currentTheme = Request.Cookies["theme"] ?? "light";
    var newTheme = currentTheme == "light" ? "dark" : "light";
    
    Response.Cookies.Append("theme", newTheme, new CookieOptions
    {
        Expires = DateTimeOffset.Now.AddDays(30),
        HttpOnly = true
    });
    
    return Redirect(Request.Headers["Referer"].ToString() ?? "/");
}
```

#### View Updates:
- **_Layout.cshtml** - Added `class="@ViewBag.Theme"` to `<body>` tag
- **_Layout.cshtml** - Added theme toggle button in navbar with 🌙 icon

#### CSS Updates:
- **site.css** - Added comprehensive dark theme styles
  - Dark background (#1a1a1a) and light text (#e0e0e0)
  - Dark navbar and footer styling
  - Dark table styling with striping
  - Dark form controls
  - Smooth transitions (0.3s ease)
  - Custom link colors (#66b3ff)

**SRP Compliance:**
- **ThemeFilter**: Single responsibility - read cookie and pass to views
- **ToggleTheme action**: Single responsibility - toggle and save theme preference
- **CSS**: Single responsibility - visual presentation of themes

**Benefits:**
- ✅ User preference persists for 30 days via cookies
- ✅ Seamless theme switching without page reload UI flash
- ✅ All pages support both themes automatically
- ✅ Clean separation of concerns

**Files Modified:**
- `Filtres/ThemeFilter.cs` - NEW filter for theme management
- `Controllers/AuthController.cs` - Added ThemeFilter attribute
- `Controllers/TodoController.cs` - Added ThemeFilter and ToggleTheme action
- `Views/Shared/_Layout.cshtml` - Added theme class and toggle button
- `wwwroot/css/site.css` - Added dark theme styles

---

### 4. Action Logging Filter ✅

**Date:** 2025-12-12

**Feature:** Log all action executions to a text file with timestamp, username, controller, and action.

**Changes Made:**

#### New Filter: `LoggingFilter.cs`
```csharp
public class LoggingFilter : ActionFilterAttribute
{
    private static readonly object _lockObject = new object();
    private const string LogDirectory = "Logs";
    private const string LogFileName = "actions.log";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
        var actionName = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var userName = context.HttpContext.Session.GetString("UserName") ?? "Anonymous";
        
        var logEntry = $"[{dateTime}] | User: {userName} | Controller: {controllerName} | Action: {actionName}";
        WriteLog(logEntry);
    }
}
```

**Features:**
- **Thread-safe**: Uses lock mechanism for concurrent writes
- **Auto-create directory**: Creates `Logs/` folder if it doesn't exist
- **Error handling**: Catches exceptions to prevent app crashes
- **Log format**: `[DateTime] | User: {UserName} | Controller: {Controller} | Action: {Action}`

#### Controller Updates:
- **AuthController** - Added `[LoggingFilter]` attribute
- **TodoController** - Added `[LoggingFilter]` attribute

**Log File Location:** `Logs/actions.log` in project root

**Example Log Output:**
```
[2025-12-12 22:30:00] | User: Anonymous | Controller: Auth | Action: Inscription
[2025-12-12 22:30:15] | User: JohnDoe | Controller: Todo | Action: Index
[2025-12-12 22:30:30] | User: JohnDoe | Controller: Todo | Action: Add
[2025-12-12 22:31:00] | User: JohnDoe | Controller: Todo | Action: ToggleTheme
```

**SRP Compliance:**
- **LoggingFilter**: Single responsibility - log action executions
- **WriteLog method**: Single responsibility - handle file I/O
- Clear separation between interception and persistence

**Benefits:**
- ✅ Complete audit trail of user actions
- ✅ Thread-safe for concurrent requests
- ✅ Automatic directory creation
- ✅ Graceful error handling
- ✅ Human-readable format

**Files Modified:**
- `Filtres/LoggingFilter.cs` - NEW filter for action logging
- `Controllers/AuthController.cs` - Added LoggingFilter attribute
- `Controllers/TodoController.cs` - Added LoggingFilter attribute

---

## Summary

**Problem:** JSON serialization of session values caused authentication to fail.

**Solution:** Use `HttpContext.Session.SetString()` directly for simple strings.

**Main SOLID Violation:** Single Responsibility Principle - services and controllers have too many responsibilities.

**Recommended Action:** Implement the service layer refactoring shown above to properly separate concerns.
