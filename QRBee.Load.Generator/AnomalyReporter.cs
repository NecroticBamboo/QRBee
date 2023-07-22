using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRBee.Load.Generator
{
    internal class AnomalyReporter : IDisposable, IAnomalyReporter
    {
        private readonly StreamWriter _writer;
        private bool disposedValue;
        private readonly object _lock = new();

        public AnomalyReporter()
        {
            const string fileName = "anomalies.csv";
            var writeHeader = !File.Exists(fileName);

            var file = new FileStream(fileName, writeHeader ? FileMode.Create : FileMode.Append, FileAccess.Write);
            _writer = new StreamWriter(file);

            if (writeHeader)
            {
                _writer.WriteLine("Start,End,Label");
            }
        }

        public void Report(DateTime start, DateTime end, string description)
        {
            lock (_lock)
            {
                _writer.WriteLine($"\"{start:O}\",\"{end:O}\",\"{description}\"");
                _writer.Flush();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
                }

                disposedValue = true;
            }
        }

        ~AnomalyReporter()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
