using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ConanExilesServer.Helpers
{
    /// <summary>
    /// Implementation taken from Microsoft.AspNet.Identity.AsyncHelper
    /// </summary>
    public class AsyncHelper
    {
        private static readonly CancellationToken DefaultCancellationToken = CancellationToken.None;
        private static readonly TaskCreationOptions DefaultTaskCreationOptions = TaskCreationOptions.None;
        private static readonly TaskContinuationOptions DefaultTaskContinuationOptions = TaskContinuationOptions.None;
        private static readonly TaskScheduler DefaultTaskScheduler = TaskScheduler.Default;

        private static readonly TaskFactory TaskFactory = new
            TaskFactory(DefaultCancellationToken, DefaultTaskCreationOptions,
                DefaultTaskContinuationOptions, DefaultTaskScheduler);

        /// <summary>
        /// Run async task synchronously
        /// </summary>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return TaskFactory.StartNew(delegate
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }, cancellationToken).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Run async task synchronously
        /// </summary>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return RunSync(func, DefaultCancellationToken);
        }

        /// <summary>
        /// Run async task synchronously
        /// </summary>
        public static void RunSync(Func<Task> func, CancellationToken cancellationToken)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            TaskFactory.StartNew(delegate
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }, cancellationToken).Unwrap().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Run async task synchronously
        /// </summary>
        public static void RunSync(Func<Task> func)
        {
            RunSync(func, DefaultCancellationToken);
        }
    }
}