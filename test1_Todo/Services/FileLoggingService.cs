using System;
using System.IO;

namespace test1_Todo.Services
{
    public class FileLoggingService : ILoggingService // implementer l interface , donc fournir la methode logAction
    {
        private static readonly object _lockObject = new object(); // pour avoir la synchronisation lors de l ecriture dans le fichier log , ( static bcs on a une seul instance pour tte la classe  , readonly car l objet ne peut pas etre remplace apres sa creatuon )
        private const string LogDirectory = "Logs"; // dossier ou les log vont etre ecrit ( const bcs facile a changer si besoin et pas d erreur de frappe)
        private const string LogFileName = "actions.log"; // nom fichier de log

        public void LogAction(string userName, string controllerName, string actionName)
        {
            var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // format de la date et heure 
            var logEntry = $"[{dateTime}] | User: {userName} | Controller: {controllerName} | Action: {actionName}"; // format de l entree de log ( $ rend le code plus visible et facile a maintenir car il evite la concatenation) 
            WriteToFile(logEntry); // c est pour ecrire dans le fichier
        }

        private void WriteToFile(string logEntry)
        {
            lock (_lockObject) // pour assurer que l ecriture dans le fichier est thread safe ( evite les conflits si plusieurs threads essaient d ecrire en meme temps) par exe,ple l utilisateur A et B font des actions en meme temps , on va pas avoir de conflitdans le fichier log  
            {
                try
                {
                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }

                    var logPath = Path.Combine(LogDirectory, LogFileName); // chemin complet du fichier de log

                    File.AppendAllText(logPath, logEntry + Environment.NewLine); // ajoute l entree de log a la fin du fichier ( Environment.NewLine pour s assurer que chaque entree est sur une nouvelle ligne)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}"); // en cas d erreur lors de l ecriture dans le fichier log , on affiche un message d erreur dans la console ( on ne relance pas l exception pour ne pas perturber le flux normal de l application)
                }
            }
        }
    }
}
