using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MultyThreadDemo
{
    class Logger : IDisposable
    {
        public Logger(string logFileName)
        {
            _logFileName = logFileName;
            _strm = new FileStream(_logFileName, FileMode.Append);
            _wrtr = new StreamWriter(_strm);
        }

        public void WriteToLog(string msg)
        {
            // lock (<объект синхронизации>) !!! <объект синхронизации> - обязательно reference type
            //lock (this)    // !!! возможны проблемы с производительностью

            lock (_lockObj)
            {
                // 1 вариант: очень меделнный
                // File.ApendAllText(_logFileName, string.Format("{0}: {1}{2}", DateTime.Now.ToString(), msg, Environment.NewLine));    
                // 2
                _wrtr.Write(string.Format("{0}: {1}{2}", DateTime.Now.ToString(), msg, Environment.NewLine));
            }
        }

        public void WriteDataToLog(string msg)
        {
            lock (_lockObj)
            {
                _wrtr.WriteLine(string.Format(msg, Environment.NewLine));
            }
        }

        public void Dispose()
        {
            _wrtr.Flush();    // Принудительный сброс буфера
            _strm.Flush();    // Принудительный сброс буфера
            _strm.Dispose();
        }

        public void Close()
        {
            _wrtr.Close();
        }

        StreamWriter _wrtr = null;
        FileStream _strm = null;

        private object _lockObj = new object();    // объект синхронизации, который связан с файлом _logFileName
        private string _logFileName = "log.txt";
    }
}
