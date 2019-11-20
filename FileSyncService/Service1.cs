using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Threading;

namespace FileSyncService
{
    public partial class Service1 : ServiceBase
    {
        Synchronizer synchronizer;
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
        }

        protected override void OnStart(string[] args)
        {
            synchronizer = new Synchronizer();
            Thread synchronizerThread = new Thread(new ThreadStart(synchronizer.Start));
            synchronizerThread.Start();
        }

        protected override void OnStop()
        {
            synchronizer.Stop();
            Thread.Sleep(1000);
        }
    }


    class Synchronizer
    {
        FileSystemWatcher watcher;
        Serializer serializer;

        string _folder;
        string _source;
        string _destination;
        string _logfile;

        object obj = new object();
        bool enabled = true;
        public Synchronizer()
        {
            serializer = new Serializer();
            _folder = serializer.Configdata.folder;
            _source = serializer.Configdata.source;
            _logfile = serializer.Configdata.logfile;
            _destination = serializer.Configdata.destination;

            watcher = new FileSystemWatcher(_folder);
            watcher.Filter = _source;
            watcher.Changed += Watcher_Changed;
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
            while (enabled)
            {
                Thread.Sleep(1000);
            }
        }
        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        // изменение файлов
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                string fileEvent = "был изменен";
                string filePath = e.FullPath;
                RecordEntry(fileEvent, filePath);
                Copy(filePath, _destination);
            }
            catch (Exception ex)
            {
                RecordEntry(ex.Message, "");
            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        private void Copy(string filepath, string destination)
        {
            try
            {
                File.Copy(filepath, destination, true);
                RecordEntry("был скопирован", filepath);
            }
            catch (Exception ex)
            {
                RecordEntry("НЕ был скопирован: " + ex.Message, filepath);
            }
        }

        private void RecordEntry(string fileEvent, string filePath)
        {
            lock (obj)
            {
                using (StreamWriter writer = new StreamWriter(_logfile, true))
                {
                    writer.WriteLine(String.Format("{0} > {1} {2}",
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), filePath, fileEvent));
                    writer.Flush();
                }
            }
        }
    }
}
