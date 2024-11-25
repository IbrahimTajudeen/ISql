using System;
using System.Linq;
using System.Collections.Generic;

namespace Isac
{
    public class ProcessStartEventArgs : EventArgs
    {         
        public string UserID = "main";
        public string ProcessName = "";
        public string ClassName = "";
        public System.Threading.Thread ProcessThread = System.Threading.Thread.CurrentThread;
        public DateTime ProcessStartTime = DateTime.Now;
        public object ObjectInstance = null;
    }
    
    public class ProcessingEventArgs : EventArgs
    {
    	public string UserID = "main";
        public string ProcessName = "";
        public string ClassName = "";
        public System.Threading.Thread ProcessThread = System.Threading.Thread.CurrentThread;
        public DateTime Time = DateTime.Now;
        public TimeSpan Duration = TimeSpan.Zero;
        public object ObjectInstance = null;
        public object EventInstance = null;
    }
    
    public class ProcessEndEventArgs : EventArgs
    {
    	public string UserID = "main";
        public string ProcessName = "";
        public string ClassName = "";
        public System.Threading.Thread ProcessThread = System.Threading.Thread.CurrentThread;
        public DateTime ProcessEndTime = DateTime.Now;
        public TimeSpan Duration = TimeSpan.Zero;
        public object ObjectInstance = null;
    }
}





