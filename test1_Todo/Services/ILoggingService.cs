namespace test1_Todo.Services
{
 
    public interface ILoggingService
    {
        void LogAction(string userName, string controllerName, string actionName);
    }
}