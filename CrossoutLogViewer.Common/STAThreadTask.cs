using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossoutLogView.Common
{
    public static class STAThreadTask
    {
        /// <summary>
        /// Returns a new STAThread spawned <see cref="Task{T}"/> for the provided <see cref="Func{T}"/>.
        /// </summary>
        /// <typeparam name="T">The return type of the <see cref="Task{T}"/>.</typeparam>
        /// <param name="func">The function.</param>
        /// <returns>A new STAThread spawned <see cref="Task{T}"/> for the provided <see cref="Func{T}"/>.</returns>
        public static Task<T> Run<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    tcs.SetResult(func());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Returns a new STAThread spawned <see cref="Task"/> for the provided <see cref="Action"/>.
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns>A new STAThread spawned <see cref="Task"/> for the provided <see cref="Action"/>.</returns>
        public static Task Run(Action func)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    func();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}
