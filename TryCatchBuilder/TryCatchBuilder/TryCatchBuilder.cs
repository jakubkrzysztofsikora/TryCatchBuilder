using System;
using System.Collections.Generic;
using System.Threading;

namespace TryCatchBuilder
{
    public class TryCatchBuilder<TReturnFromWrappedFunction>
    {
		private static readonly Action DoActionNothing = () => { };

		private readonly Func<TReturnFromWrappedFunction> _function;
		private readonly Dictionary<Type, Action<Exception>> _catchCollection;
		private int _retryCount;
		private TimeSpan _retryInterval;
		private TimeSpan _retryIncrement;
		private Action _onReTryAction;
		private Action _onRepeatedFailureAction;

		public static TryCatchBuilder<TReturnFromWrappedFunction> Try(Func<TReturnFromWrappedFunction> function)
		{
			return new TryCatchBuilder<TReturnFromWrappedFunction>(function);
		}

		protected TryCatchBuilder(Func<TReturnFromWrappedFunction> function)
		{
			_function = function;
			_catchCollection = new Dictionary<Type, Action<Exception>>();
			_retryCount = 0;
			_retryInterval = TimeSpan.Zero;
			_retryIncrement = TimeSpan.Zero;
			_onReTryAction = DoActionNothing;
			_onRepeatedFailureAction = DoActionNothing;
		}


		public TryCatchBuilder<TReturnFromWrappedFunction> Catch<TException>(Action<Exception> onExceptionCatched)
			where TException : Exception
		{
			_catchCollection.Add(typeof(TException), onExceptionCatched);
			return this;
		}

		public Func<TReturnFromWrappedFunction> Build()
		{
			return () =>
			{
				for (int i = 0; i <= _retryCount; ++i, Thread.Sleep(_retryInterval), _retryInterval += _retryIncrement, _onReTryAction())
				{
					try
					{
						return _function();
					}
					catch (Exception e)
					{
						foreach (var catchElement in _catchCollection)
						{
							if (e.GetType() == catchElement.Key)
							{
								catchElement.Value(e);
							}
						}
					}
				}

				_onRepeatedFailureAction();

				return default(TReturnFromWrappedFunction);
			};
		}

		public TryCatchBuilder<TReturnFromWrappedFunction> ReTry(int retryCount, TimeSpan baseRetryInterval, TimeSpan? retryIncrement = null, Action onReTryAction = null)
		{
			_retryCount = retryCount < 0 ? 0 : retryCount;
			_retryInterval = baseRetryInterval;
			_retryIncrement = retryIncrement ?? TimeSpan.Zero;
			_onReTryAction = onReTryAction ?? DoActionNothing;
			return this;
		}

		public TryCatchBuilder<TReturnFromWrappedFunction> OnRepeatedFailure(Action onRepeatedFailureAction)
		{
			_onRepeatedFailureAction = onRepeatedFailureAction ?? DoActionNothing;
			return this;
		}
	}
}
