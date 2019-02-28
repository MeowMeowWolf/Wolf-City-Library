using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NsDebugLog
{
    public static class LogCenter
    {
        /*配置*/
        private static string paramFilePath;  // 文件路径(结尾需要带\)
        private static string paramFileNameHead;  // 文件头
        private static string paramFileNameTail;  // 文件尾

        public static uint paramMinLevel;  // 最小日志级别
        public static uint paramMaxLevel;  // 最大日志级别
        private static uint paramMaxLine;  // 日志文件最大行数

        private static uint paramShortSleepTime;  // 短休眠时间(毫秒)
        private static uint paramLongSleepCount;  // 进入长休眠所需的短次数

        private static Boolean isParam = false;

        /*主体*/
        private static int ProcessNum;  // 进程号
        private static Dictionary<int,Logger> loggerList;
        private static Queue<DebugInfo> DebugList;

        private static eWorkState WorkState;
        private static uint LineCount;
        private static uint FileCount;

        /*初始化*/
        public static void Init()
        {
            ProcessNum = System.Diagnostics.Process.GetCurrentProcess().Id;  // 进程号
            loggerList = new Dictionary<int, Logger>();
            DebugList = new Queue<DebugInfo>();

            WorkState = eWorkState.LongSleep;
            LineCount = 0;
            FileCount = 0;
        }
        // 参数配置
        public static void Param(string FilePath,string FileNameHead,string FileNameTail,uint MaxLine, uint ShortSleepTime,uint LongSleepCount)
        {
            if(isParam==false)
            {
                paramFilePath = FilePath;
                paramFileNameHead = FileNameHead;
                paramFileNameTail = FileNameTail;
                CreatePathFile(paramFilePath, paramFileNameHead + paramFileNameTail);

                paramMinLevel = 0;
                paramMaxLevel = 100;
                paramMaxLine = MaxLine;

                paramShortSleepTime = ShortSleepTime;
                paramLongSleepCount = LongSleepCount;

                isParam = true;
            }
        }
        /*日志推送*/
        public static void Push(DebugInfo info)
        {
            if(paramMinLevel <= info.DebugLevel && info.DebugLevel <= paramMaxLevel)
            DebugList.Enqueue(info);
            if (WorkState == eWorkState.LongSleep)
            {
                WorkState = eWorkState.Work;
                Task task = new Task(Writing);
                task.Start();
            }
        }
        /*执行日志输出*/
        private static void Writing()
        {
            uint SleepCount = 0;
            StreamWriter writer = new StreamWriter(paramFilePath + paramFileNameHead + paramFileNameTail, true);
            while (SleepCount < paramLongSleepCount)
            {
                switch (WorkState)
                {
                    case eWorkState.LongSleep:
                        if (DebugList.Count == 0)
                        {
                            // 不会走到这里
                            break;
                        }
                        else
                        {
                            // 长眠期间来日志包，唤醒
                            SleepCount = 0;
                            WorkState = eWorkState.Work;
                            break;
                        }
                    case eWorkState.ShortSleep:
                        if (DebugList.Count == 0)
                        {
                            // 短眠，没日志包，继续睡一会儿
                            System.Threading.Thread.Sleep((int)paramShortSleepTime);
                            SleepCount++;
                            break;
                        }
                        else
                        {
                            // 短眠，来日志包，唤醒
                            SleepCount = 0;
                            WorkState = eWorkState.Work;
                            break;
                        }
                    case eWorkState.Pause:
                        if (DebugList.Count == 0)
                        {
                            // 暂停，不管有没有日志包，不计入短眠次数
                            System.Threading.Thread.Sleep((int)paramShortSleepTime);
                            break;
                        }
                        else
                        {
                            // 暂停，不管有没有日志包，不计入短眠次数
                            System.Threading.Thread.Sleep((int)paramShortSleepTime);
                            break;
                        }
                    case eWorkState.Work:
                        if (DebugList.Count == 0)
                        {
                            // 工作状态，发现没日志包了，进入短眠
                            WorkState = eWorkState.ShortSleep;
                            break;
                        }
                        else
                        {
                            // 干活
                            DebugInfo info = DebugList.Dequeue();
                            writer.WriteLine(DebugFormat(info));
                            writer.Flush();
                            LineCount++;
                            if (LineCount >= paramMaxLine)
                            {//日志文件太大了，换一份新的
                                WorkState = eWorkState.Pause;
                                writer.Close();
                                UseNewFile();
                                LineCount = 0;
                                FileCount++;
                                writer = new StreamWriter(paramFilePath + paramFileNameHead + paramFileNameTail, true);
                                WorkState = eWorkState.Work;
                            }
                            break;
                        }
                }//switch
            }//while
            WorkState = eWorkState.LongSleep;
            writer.Close();
        } // Writing

        /*注册*/
        public static void LogOn(int threadNum,Logger logger)
        {
            loggerList.Add(threadNum,logger);
        }
        /*注销*/
        public static void LogOut(int threadNum)
        {
            if (loggerList.ContainsKey(threadNum))
            {
                loggerList.Remove(threadNum);
            }
        }

        /*如果目录或者文件不存在，则创建目录或文件*/
        private static void CreatePathFile(string filePath, string fileName)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            if (!File.Exists(filePath + fileName))
            {
                File.Create(filePath + fileName).Close();
            }
        }

        /*切换新的文件*/
        private static void UseNewFile()
        {
            System.Threading.Thread.Sleep(1000);

            string time = DateTime.Now.ToString("yyyyMMddTHHmmss");
            string throwFileName = paramFileNameHead + "_" + FileCount + "_" + time + paramFileNameTail;
            string throwFile = paramFilePath + throwFileName;
            string rowFile = paramFilePath + paramFileNameHead + paramFileNameTail;

            File.Move(rowFile, throwFile);
        }

        /*日志信息格式化输出*/
        public static string DebugFormat(DebugInfo debugInfo)
        {
            string debugPack = debugInfo.DebugTime.ToString("<yyyy-MM-dd HH:mm:ss:fff>");
            debugPack = debugPack + "[" + debugInfo.DebugLevel + "]";
            debugPack = debugPack + "[" + ProcessNum + "·" + debugInfo.ThreadNum + "]";
            debugPack = debugPack + "|" + debugInfo.DebugType.ToString() + ">";
            debugPack = debugPack + debugInfo.DebugText;
            return debugPack;
        }

    } //LogCenter

    enum eWorkState
    {
        LongSleep = 0,
        ShortSleep = 1,
        Work = 2,
        Pause = 3 
    }

    public enum eDebugType
    {
        Info = 0,
        War = 1,
        Err = 2
    }

    public struct DebugInfo
    {
        public DateTime DebugTime;
        public uint DebugLevel;
        public int ProcessNum;
        public int ThreadNum;
        public eDebugType DebugType;
        public string DebugText;

        public DebugInfo(uint debugLevel, eDebugType debugType, string debugText)
        {
            DebugTime = DateTime.Now;
            DebugLevel = debugLevel;
            ProcessNum = -1;
            ThreadNum = -1;
            DebugType = debugType;
            DebugText = debugText;
        }
        public DebugInfo(uint debugLevel, string debugType, string debugText)
        {
            DebugTime = DateTime.Now;
            DebugLevel = debugLevel;
            ProcessNum = -1;
            ThreadNum = -1;
            DebugType = (eDebugType)Enum.Parse(typeof(eDebugType), debugType);
            DebugText = debugText;
        }

    }

    public class Logger
    {
        private int ThreadNum;
        public Logger()
        {
            ThreadNum = -1;
        }
        /*注册*/
        public void LogOn()
        {
            ThreadNum = System.Threading.Thread.CurrentThread.ManagedThreadId;
            LogCenter.LogOn(ThreadNum,this);
        }
        /*注销*/
        public void LogOut()
        {
            LogCenter.LogOut(ThreadNum);
            ThreadNum = -1;
        }
        /*日志推送*/
        public void Push(DebugInfo info)
        {
            if (ThreadNum != -1)
            {
                info.ThreadNum = this.ThreadNum;
                LogCenter.Push(info);
            }
        }
    }

}
