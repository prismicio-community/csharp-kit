using System;
using System.Threading.Tasks;

namespace prismic
{
    namespace extensions {
        public static class Task {
			///<summary>Transforms a task's result, or propagates its exception or cancellation.</summary>
			public static Task<TOut> Select<TIn, TOut>(this Task<TIn> task, Func<TIn, TOut> projection) {
				var r = new TaskCompletionSource<TOut>();
				task.ContinueWith(self => {
					if (self.IsFaulted) r.SetException(self.Exception.InnerExceptions);
					else if (self.IsCanceled)
						r.SetCanceled();
					else try {
						r.SetResult(projection(self.Result));
					} catch (Exception e) {
						r.SetException(e);
					}
			});
			return r.Task;
		}
	}
	}
}

