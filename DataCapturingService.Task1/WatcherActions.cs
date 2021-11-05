using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace DataCapturingService.Task1
{
    public static class WatcherActions
    {
        public static FileSystemWatcher Watcher;
        public static bool IsWatcherInUse = false;
        public static void InitializeWatcher(string path, string filter)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            Watcher = new FileSystemWatcher(path, filter);
            Watcher.EnableRaisingEvents = true;
            Watcher.NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Size |
                                   NotifyFilters.Security;

            Watcher.Created += new FileSystemEventHandler(OnCreated);
        }

        public static void OnCreated(object sender, FileSystemEventArgs e)
        {
            WatcherActions.IsWatcherInUse = true;
            var filePath = e.FullPath;
            var fileName = System.IO.Path.GetFileName(filePath);

            var fileInfo = new FileInfo(filePath);
            while (Helpers.IsFileLocked(fileInfo))
            {
                Thread.Sleep(50);
            }

            var connection = RabbitMqActions.GetRabbitMqConnection();
            var channel = connection.CreateModel();

            try
            {
                RabbitMqActions.SendInChunks(channel,fileName, filePath);
                Console.WriteLine("Chunks complete!");
            }
            finally
            {
                channel.Close();
                connection.Close();
                WatcherActions.IsWatcherInUse = false;
            }
        }

        public static bool IsWatcherBusy()
        {
            return WatcherActions.IsWatcherInUse;
        }
    }
}
