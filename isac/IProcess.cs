using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Isac
{
    public abstract class Process 
    {         
        public event EventHandler<ProcessStartEventArgs> ProcessStart;
        public event EventHandler<ProcessingEventArgs> Processing;
        public event EventHandler<ProcessEndEventArgs> ProcessEnd;
        private DateTime startTime;
        
        protected virtual void OnProcessStart(ProcessStartEventArgs e)
        {
        	ProcessStart?.Invoke(this, e);
        }
        
        protected virtual void OnProcessing(ProcessingEventArgs e)
        {
        	Processing?.Invoke(this, e);
        }
        
        protected virtual void OnProcessEnd(ProcessEndEventArgs e)
        {
        	ProcessEnd?.Invoke(this, e);
        }
        
        internal void ProcessStartInvoker(string userID, string className, Thread thread, object instance, string processName)
        {
			var process = new ProcessStartEventArgs();
			process.UserID = userID;
			process.ClassName = className;
			process.ProcessThread = thread;
			process.ObjectInstance = instance;
			process.ProcessName = processName;
			process.ProcessStartTime = DateTime.Now;
			startTime = process.ProcessStartTime;
			OnProcessStart(process);
        }
        
        internal void ProcessingInvoker(string userID, string className,Thread thread, object instance, object eventInstance, string processName)
        {
        	var process = new ProcessingEventArgs();
        	process.UserID = userID;
        	process.ClassName = className;
        	process.ProcessName = processName;
        	process.Time = DateTime.Now;
        	process.ProcessThread = thread;
        	process.ObjectInstance = instance;
        	process.Duration = process.Time.Subtract(startTime);
        	OnProcessing(process);
        }
        
        internal void ProcessEndInvoker(string userID, string className,Thread thread, object instance, string processName)
        {
        	var process = new ProcessEndEventArgs();
        	process.UserID = userID;
        	process.ClassName = className;
        	process.ProcessName = processName;
        	process.ProcessEndTime = DateTime.Now;
        	process.ProcessThread = thread;
        	process.ObjectInstance = instance;
        	process.Duration = process.ProcessEndTime.Subtract(startTime);
        	OnProcessEnd(process);
        }
        
    }
}





