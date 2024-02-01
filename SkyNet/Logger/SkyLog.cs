using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SkyNet.Utils;

public static class SkyLog
{
    private static readonly object _lock = new object();
    private static List<IWriter> _writers = new List<IWriter>();

    public static void RemoveAll()
    {
        lock (_lock)
        {
            for (int index = 0; index < _writers.Count; ++index)
                _writers[index].Dispose();
            _writers = new List<IWriter>();
        }
    }

    public static void Add<T>(T instance) where T : class, IWriter
    {
        lock (_lock)
            _writers.Add(instance);
    }

    public static void Info(string message)
    {
        lock (_lock)
        {
            VerifyOneWriter();
            for (int index = 0; index < _writers.Count; ++index)
                _writers[index].Info(message);
        }
    }

    public static void Info(object message)
    {
        Info(Format(message));
    }

    public static void Info(string message, object arg0)
    {
        Info(Format(message, arg0));
    }

    public static void Info(string message, object arg0, object arg1)
    {
        Info(Format(message, arg0, arg1));
    }

    public static void Info(string message, object arg0, object arg1, object arg2)
    {
        Info(Format(message, arg0, arg1, arg2));
    }

    public static void Info(string message, params object[] args)
    {
        Info(Format(message, args));
    }

    internal static void Debug(string message)
    {
        lock (_lock)
        {
            VerifyOneWriter();
            for (int index = 0; index < _writers.Count; ++index)
                _writers[index].Debug(message);
        }
    }

    internal static void Debug(object message)
    {
        Debug(Format(message));
    }

    public static void Debug(string message, object arg0)
    {
        Debug(Format(message, arg0));
    }

    public static void Debug(string message, object arg0, object arg1)
    {
        Debug(Format(message, arg0, arg1));
    }

    public static void Debug(string message, object arg0, object arg1, object arg2)
    {
        Debug(Format(message, arg0, arg1, arg2));
    }

    public static void Debug(string message, params object[] args)
    {
        Debug(Format(message, args));
    }

    public static void Warn(string message)
    {
        lock (_lock)
        {
            VerifyOneWriter();
            for (int index = 0; index < _writers.Count; ++index)
                _writers[index].Warn(message);
        }
    }

    public static void Warn(object message)
    {
        Warn(Format(message));
    }

    public static void Warn(string message, object arg0)
    {
        Warn(Format(message, arg0));
    }

    public static void Warn(string message, object arg0, object arg1)
    {
        Warn(Format(message, arg0, arg1));
    }

    public static void Warn(string message, object arg0, object arg1, object arg2)
    {
        Warn(Format(message, arg0, arg1, arg2));
    }

    public static void Warn(string message, params object[] args)
    {
        Warn(Format(message, args));
    }

    private static object[] FixNulls(object[] args)
    {
        if (args == null)
            args = new object[0];
        for (int index = 0; index < args.Length; ++index)
        {
            if (object.ReferenceEquals(args[index], (object)null))
                args[index] = (object)"NULL";
        }
        return args;
    }

    public static void Error(string message)
    {
        lock (_lock)
        {
            VerifyOneWriter();
            for (int index = 0; index < _writers.Count; ++index)
                _writers[index].Error(message);
        }
    }

    public static void Error(object message)
    {
        Error(Format(message));
    }

    public static void Error(string message, object arg0)
    {
        Error(Format(message, arg0));
    }

    public static void Error(string message, object arg0, object arg1)
    {
        Error(Format(message, arg0, arg1));
    }

    public static void Error(string message, object arg0, object arg1, object arg2)
    {
        Error(Format(message, arg0, arg1, arg2));
    }

    public static void Error(string message, params object[] args)
    {
        Error(Format(message, args));
    }

    public static void Exception(Exception exception)
    {
        lock (_lock)
        {
            for (int index = 0; index < _writers.Count; ++index)
            {
                var st = new StackTrace(exception, true);
                var frame = st.GetFrame(0);

                _writers[index].Error(exception.GetType().ToString() + "(" + frame.GetFileLineNumber() + "): " + exception.Message);
                _writers[index].Error(exception.StackTrace);
            }
        }
    }

    private static void VerifyOneWriter()
    {
        if (_writers.Count != 0)
            return;
    }

    private static string Format(object message)
    {
        return message == null ? "NULL" : message.ToString();
    }

    private static string Format(string message, object arg0)
    {
        return string.Format(message, arg0);
    }

    private static string Format(string message, object arg0, object arg1)
    {
        return string.Format(message, arg0, arg1);
    }

    private static string Format(string message, object arg0, object arg1, object arg2)
    {
        return string.Format(message, arg0, arg1, arg2);
    }

    private static string Format(string message, object[] args)
    {
        if (args == null)
            return message;
        for (int index = 0; index < args.Length; ++index)
            args[index] = args[index];
        return string.Format(message, args);
    }

    internal static void Setup(LogTargets logTargets)
    {
        if ((logTargets & LogTargets.File) == LogTargets.File)
            Add(new File());

        if ((logTargets & LogTargets.SystemOut) == LogTargets.SystemOut)
            Add(new SystemOut());
    }

    internal static string GetTime()
    {
        return string.Format("{0} | ", DateTime.Now);
    }

    internal class File : IWriter, IDisposable
    {
        private volatile bool running = true;
        private Queue<string> threadQueue;
        private string logDir;
        private FileStream fileStream;
        private StreamWriter streamWriter;
        private Thread thread;
        private AutoResetEvent threadEvent;

        public File()
        {
            threadQueue = new Queue<string>(1024);
            threadEvent = new AutoResetEvent(false);

            logDir = Directory.GetCurrentDirectory() + "/Log/";

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            fileStream = System.IO.File.Open(string.Format("{0}SkyNet_Log_{1}.txt", logDir, DateTime.Now.ToString("dd-MM-yyyy_HH-mm")), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            streamWriter = new StreamWriter(fileStream);

            thread = new Thread(WriteLoop);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Queue(string message)
        {
            lock (threadQueue)
            {
                threadQueue.Enqueue(message);
                threadEvent.Set();
            }
        }

        void IWriter.Info(string message)
        {
            Queue(GetTime() + message);
        }

        void IWriter.Debug(string message)
        {
            Queue(GetTime() + message);
        }

        void IWriter.Warn(string message)
        {
            Queue(GetTime() + message);
        }

        void IWriter.Error(string message)
        {
            Queue(GetTime() + message);
        }

        public void Dispose()
        {
            running = false;
        }

        private void WriteLoop()
        {
            try
            {
                while (running)
                {
                    if (threadEvent.WaitOne(100))
                    {
                        lock (threadQueue)
                        {
                            for (int i = 0; i < threadQueue.Count; i++)
                            {
                                streamWriter.WriteLine(threadQueue.Dequeue());
                            }
                        }
                    }
                    streamWriter.Flush();
                    fileStream.Flush();
                }
                streamWriter.Flush();
                streamWriter.Close();
                streamWriter.Dispose();
                threadEvent.Close();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }
    }

    internal class SystemOut : IWriter, IDisposable
    {
        void IWriter.Info(string message)
        {
            Console.Out.WriteLine(GetTime() + message);
        }

        void IWriter.Debug(string message)
        {
            Console.Out.WriteLine(GetTime() + message);
        }

        void IWriter.Warn(string message)
        {
            Console.Out.WriteLine(GetTime() + message);
        }

        void IWriter.Error(string message)
        {
            Console.Error.WriteLine(GetTime() + message);
        }

        public void Dispose()
        {
        }
    }
}