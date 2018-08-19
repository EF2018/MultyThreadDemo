using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace MultyThreadDemo
{
    public class PartMatrix
    {
        public int begin;
        public int end;
    }

    public delegate void CalculateHandler(PartMatrix x);
    public delegate void CalculateHandler1();

    class Matrix
    {
        static Logger _log;
        bool[,] _field;
        int _size;
        int _maxQThreads;
        CountdownEvent _countdown;
        int qp;
        int k1;
        int j11;
        int i11;

        public Matrix(int size, int maxQThreads)
        {
            _log = new Logger("log.txt");
            _size = size;
            _field = new bool[_size, _size];
            _maxQThreads = maxQThreads;
        }

        private void InizializeField()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _field[i, j] = true;
                }
            }
        }

        public List<long> Start()
        {
            _log.WriteDataToLog(DateTime.Now.ToString() + "------------------------");
            _log.WriteDataToLog("Threads    Time, ms ");
            List<long> data = new List<long>();

            for (int i = 1; i <= _maxQThreads; i++)//цикл деления поля на потоки
            {
                InizializeField();
                int p = _size / i;//количество строк в потоке

                Stopwatch stopWatch = new Stopwatch();
                _countdown = new CountdownEvent(i);

                stopWatch.Start();

                //List<Thread> myThrConteiner = new List<Thread>();
                List<IAsyncResult> myDelConteiner = new List<IAsyncResult>();
                for (int k = 0, j = 0; j < i; k = k + p, j++)
                {
                    PartMatrix mypiece = new PartMatrix();
                    mypiece.begin = k;
                    mypiece.end = k + p;

                    CalculateHandler handler = new CalculateHandler(CalculateCells1);
                    //IAsyncResult resultObj = 
                    handler.BeginInvoke(mypiece, handler.EndInvoke, null);

                    //myDelConteiner.Add(resultObj);
                    //Thread myThread = new Thread(new ParameterizedThreadStart(CalculateCells));
                    //myThrConteiner.Add(myThread);
                    //myThread.Start(mypiece);
                }

                //_countdown.Wait();   // Blocks until Signal has been called "i" times
                //for (int n = 0; n < myThrConteiner.Count; n++)
                //{
                //    myThrConteiner[n].Join();
                //}
                //for (int j = 0; j < myDelConteiner.Count; j++)
                //{
                //    myDelConteiner[j].EndInvoke(resultObj)
                //}

                stopWatch.Stop();
                data.Add(stopWatch.ElapsedMilliseconds);
                _log.WriteDataToLog("" + stopWatch.ElapsedMilliseconds + "; " + CheckMatrix());
            }
            MessageBox.Show("Вычисление матрицы завершено");
            _log.Close();
            return data;
        }


        public List<long> StartThreads()
        {
            _log.WriteDataToLog(DateTime.Now.ToString() + "---(Threads)-----------------");
            _log.WriteDataToLog("Threads    Time, ms ");
            List<long> data = new List<long>();

            for (int i = 1; i <= _maxQThreads; i++)//цикл деления поля на потоки
            {
                InizializeField();
                int p = _size / i;//количество строк в потоке
                Stopwatch stopWatch = new Stopwatch();
                _countdown = new CountdownEvent(i);

                stopWatch.Start();

                for (int k = 0, j = 0; j < i; k = k + p, j++)
                {
                    PartMatrix mypiece = new PartMatrix();
                    mypiece.begin = k;
                    mypiece.end = k + p;

                    Thread myThread = new Thread(new ParameterizedThreadStart(CalculateCells));

                    myThread.Start(mypiece);
                }

                _countdown.Wait();   // Blocks until Signal has been called "i" times

                stopWatch.Stop();
                data.Add(stopWatch.ElapsedMilliseconds);
                _log.WriteDataToLog("" + stopWatch.ElapsedMilliseconds + "; " + CheckMatrix());
            }
            MessageBox.Show("Вычисление матрицы завершено");
            _log.Close();
            return data;
        }


        public List<long> StartTasks()
        {
            _log.WriteDataToLog(DateTime.Now.ToString() + "---(Tasks)-----------------");
            _log.WriteDataToLog("Threads    Time, ms ");
            List<long> data = new List<long>();

            for (int i = 1; i <= _maxQThreads; i++)//цикл деления поля на потоки
            {
                InizializeField();
                int p = _size / i;//количество строк в потоке

                Stopwatch stopWatch = new Stopwatch();
                _countdown = new CountdownEvent(i);
                stopWatch.Start();

                for (int k = 0, j = 0; j < i; k = k + p, j++)
                {
                    PartMatrix mypiece = new PartMatrix();
                    mypiece.begin = k;
                    mypiece.end = k + p;
                    Task task = new Task(() =>
                    {
                        for (int i1 = mypiece.begin; i1 < mypiece.end; i1++)
                        {
                            for (int j1 = 0; j1 < _size; j1++)
                            {
                                _field[i1, j1] = false;
                                //_field[i, j] = _field[i, j] * 1;
                            }
                        }
                        _countdown.Signal();
                    });
                    task.Start();
                }
                _countdown.Wait();   // Blocks until Signal has been called 3 times

                stopWatch.Stop();
                data.Add(stopWatch.ElapsedMilliseconds);
                _log.WriteDataToLog("" + stopWatch.ElapsedMilliseconds + "; " + CheckMatrix());
            }
            MessageBox.Show("Вычисление матрицы завершено");
            _log.Close();
            return data;
        }

        public List<long> StartOuterTask()
        {
            _log.WriteDataToLog(DateTime.Now.ToString() + "---(StartOuterTask)-----------------");
            _log.WriteDataToLog("Threads    Time, ms ");
            List<long> data = new List<long>();

            for (int i = 1; i <= _maxQThreads; i++)//цикл деления поля на потоки
            {
                InizializeField();
                int p = _size / i;//количество строк в потоке
                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();
                var outer = Task.Factory.StartNew(() => {

                    for (int k = 0, j = 0; j < i; k = k + p, j++)
                    {
                        PartMatrix mypiece = new PartMatrix();
                        mypiece.begin = k;
                        mypiece.end = k + p;
                        var task = Task.Factory.StartNew(() =>
                        {
                            for (int i1 = mypiece.begin; i1 < mypiece.end; i1++)
                            {
                                for (int j1 = 0; j1 < _size; j1++)
                                {
                                    _field[i1, j1] = false;
                                    //_field[i, j] = _field[i, j] * 1;
                                }
                            }
                        }, TaskCreationOptions.AttachedToParent);
                    }
                });
                outer.Wait();

                stopWatch.Stop();
                data.Add(stopWatch.ElapsedMilliseconds);
                _log.WriteDataToLog("" + stopWatch.ElapsedMilliseconds + "; " + CheckMatrix());
            }
            MessageBox.Show("Вычисление матрицы завершено");
            _log.Close();
            return data;
        }

        public List<long> StartParallel()
        {
            _log.WriteDataToLog(DateTime.Now.ToString() + "---(Parallel)-----------------");
            _log.WriteDataToLog("Threads    Time, ms ");
            List<long> data = new List<long>();

            for (int i = 1; i <= _maxQThreads; i++)//цикл деления поля на потоки
            {
                InizializeField();
                qp = _size / i;//количество строк в потоке
                k1 = 0;

                Stopwatch stopWatch = new Stopwatch();
                _countdown = new CountdownEvent(i);
                stopWatch.Start();
                Parallel.For(0, i, CalculateCells3);
                //for (int k = 0, j = 0; j < i; k = k + p, j++)
                //{
                //    PartMatrix mypiece = new PartMatrix();
                //    mypiece.begin = k;
                //    mypiece.end = k + p;
                //    Parallel.Invoke(() => CalculateCells(mypiece));
                //}
                //_countdown.Wait();   // Blocks until Signal has been called 3 times

                stopWatch.Stop();
                data.Add(stopWatch.ElapsedMilliseconds);
                _log.WriteDataToLog("" + stopWatch.ElapsedMilliseconds + "; " + CheckMatrix());
            }
            MessageBox.Show("Вычисление матрицы завершено");
            _log.Close();
            return data;
        }

        void CalculateCells3(int z)
        {
            int _begin = k1;
            int _end = k1 + qp;
            i11 = _begin;
            //Parallel.For(_begin, _end, CalculateCells31);
            for (int i = _begin; i < _end; i++)
            {
                for (int j1 = 0; j1 < _size; j1++)
                {
                    _field[i, j1] = false;
                    //_field[i, j] = _field[i, j] * 1;
                }
            }
            k1 = k1 + qp;
        }

        //void CalculateCells31(int z)
        //{
        //    j11 = 0;
        //    Parallel.For(0, _size-1, CalculateCells32);
        //    i11++;
        //}

        //void CalculateCells32(int z)
        //{
        //    _field[i11, j11] = false;
        //    j11++;
        //}


        public void CalculateCells(object c)
        {
            PartMatrix c1 = (PartMatrix)c;
            int _begin = c1.begin;
            int _end = c1.end;

            for (int i = _begin; i < _end; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _field[i, j] = false;
                    //_field[i, j] = _field[i, j] * 1;
                }
            }
            _countdown.Signal();
        }

        public void CalculateCells1(PartMatrix c)
        {
            PartMatrix c1 = c;
            int _begin = c1.begin;
            int _end = c1.end;

            for (int i = _begin; i < _end; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _field[i, j] = false;
                    //_field[i, j] = _field[i, j] * 1;
                }
            }
            //_countdown.Signal();
        }

        public bool CheckMatrix()
        {
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (_field[i, j] == true)
                    {
                        return false;
                    }
                }
            }
            //    if (_field[i]==true)
            //    {
            //        return false;
            //    }
            //foreach (var item in _field)
            //{
            //    if (item)
            //    {
            //        return false;
            //    }
            //}
            return true;
        }

        public void CalculateCells2(object c)
        {
            PartMatrix c1 = (PartMatrix)c;
            int _begin = c1.begin;
            int _end = c1.end;

            for (int i = _begin; i < _end; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    _field[i, j] = false;
                    //_field[i, j] = _field[i, j] * 1;
                }
            }
            //_countdown.Signal();
        }


    }
}
