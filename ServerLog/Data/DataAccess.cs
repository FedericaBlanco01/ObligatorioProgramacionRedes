using ServerLog.Model;
namespace ServerLog.Data
{
    public class DataAccess
    {
        private static object logsLock;
        private static object singletonPadlock = new object();
        public List<LogModel> Logs;
        private static DataAccess instance;
        private DataAccess()
        {
            Logs = new List<LogModel>();
            logsLock = new object();
        }

        public List<LogModel> filterLogs(string email, string date, string eventDone)
        {
            lock (logsLock)
            {
                Boolean filterDate = true;
                List<LogModel> ret = new List<LogModel>();
                if (email == null)
                {
                    email = "";
                }
                if (eventDone == null)
                {
                    eventDone = "";
                }
                foreach (LogModel log in Logs)
                {
                    if (log.UserEmail.Contains(email) && log.Event.Contains(eventDone))
                    {
                        if (date == null)
                        {
                            ret.Add(log);
                        }
                        else if (log.Date.ToString("MM/dd/yyyy").Equals(date))
                        {
                            ret.Add(log);
                        }
                    }
                }
                return ret;
            }
        }

        public void AddLog(LogModel aLog)
        {
            lock (logsLock)
            {
                Logs.Add(aLog);
            }
        }

        public static DataAccess GetInstance()
        {

            lock (singletonPadlock)
            { // bloqueante 
                if (instance == null)
                {
                    instance = new DataAccess();
                }
            }
            return instance;
        }

    }
}