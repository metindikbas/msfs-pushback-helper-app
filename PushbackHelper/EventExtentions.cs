using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PushbackHelper
{
    /// <summary>
    /// Extension methods for EventHandler-type delegates.
    /// </summary>
    public static class EventExtensions
    {
        // Extension method which marshals events back onto the main thread
        public static void Raise(this MulticastDelegate multicastDelegate, object sender, EventArgs args)
        {
            foreach (var del in multicastDelegate.GetInvocationList())
            {
                // Try for WPF first
                if (del.Target is DispatcherObject dispatcherTarget && !dispatcherTarget.Dispatcher.CheckAccess())
                {
                    // WPF target which requires marshaling
                    dispatcherTarget.Dispatcher.BeginInvoke(del, sender, args);
                }
                else
                {
                    // Maybe its WinForms?
                    if (del.Target is ISynchronizeInvoke syncTarget && syncTarget.InvokeRequired)
                    {
                        // WinForms target which requires marshaling
                        syncTarget.BeginInvoke(del, new object[] { sender, args });
                    }
                    else
                    {
                        // Just do it.
                        del.DynamicInvoke(sender, args);
                    }
                }
            }
        }
        
        // Extension method which marshals actions back onto the main thread
        public static void Raise<T>(this Action<T> action, T args)
        {
            // Try for WPF first
            if (action.Target is DispatcherObject dispatcherTarget && !dispatcherTarget.Dispatcher.CheckAccess())
            {
                // WPF target which requires marshaling
                dispatcherTarget.Dispatcher.BeginInvoke(action, args);
            }
            else
            {
                // Maybe its WinForms?
                if (action.Target is ISynchronizeInvoke syncTarget && syncTarget.InvokeRequired)
                {
                    // WinForms target which requires marshaling
                    syncTarget.BeginInvoke(action, new object[] { args });
                }
                else
                {
                    // Just do it.
                    action.DynamicInvoke(args);
                }
            }
        }
    }
}
