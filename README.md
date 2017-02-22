# TryCatchBuilder
Very small utility used to handle exceptions without polluting the code base. Adding it here, because got tired of rewriting it again.

Nuget Package: https://www.nuget.org/packages/TryCatchBuilder/

## How to use:

`Func<int> wrappedFunction = TryCatchBuilder<int>
				.Try(functionToWrap)
				.Catch<Exception>(actionOnFail)
				.Build();
wrappedFunction();`

`Func<int> wrappedFunction = TryCatchBuilder<int>
				.Try(functionToWrap)
				.Catch<Exception>(JustCountTheRetries)
				.ReTry(numberofRetries, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(1))
				.OnRepeatedFailure(actionOnRepeatedFail)
				.Build();
wrappedFunction();`
