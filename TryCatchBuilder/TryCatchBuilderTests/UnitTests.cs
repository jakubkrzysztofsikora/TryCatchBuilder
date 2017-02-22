using System;
using System.Collections.Generic;
using NUnit.Framework;
using TryCatchBuilder;

namespace TryCatchBuilderTests
{
	[TestFixture]
	public class UnitTests
	{
		private int _throwedExceptions;

		[TestCase(true)]
		[TestCase(false)]
		public void
			GivenFunctionToPerformAndActionToDoOnFail_WhenWrappingSimpleFunctionWithCatchAndExecuting_ShouldPerformActionGivenToCatchIfFailedAndNotIfNoExceptionThrown(bool shouldThrowException)
		{
			//Given
			Func<int> functionToWrap = () => FunctionToPerformByTryCatchBuilder(shouldThrowException);
			bool hasOnFailActionPerformed = false;
			Action<Exception> actionOnFail = exception => hasOnFailActionPerformed = true;

			//When
			Func<int> wrappedFunction = TryCatchBuilder<int>
				.Try(functionToWrap)
				.Catch<Exception>(actionOnFail)
				.Build();
			wrappedFunction();

			//Then
			Assert.That(hasOnFailActionPerformed, Is.EqualTo(shouldThrowException));
		}

		[Test]
		public void
			GivenRepeatedlyFailingFunctionToPerformAndActionToPerformOnRepeatedFailure_WhenWrappingSimpleFunctionWithCatchAndExecuting_ShouldPerformFunctionAFewTimesBeforeExecutingActionOnRepeatedFailure
			()
		{
			//Given
			Func<int> functionToWrap = () => FunctionToPerformByTryCatchBuilder(true);
			bool hasOnRepeatedFailActionPerformed = false;
			Action actionOnRepeatedFail = () => hasOnRepeatedFailActionPerformed = true;
			int numberOfTries = 4;
			int numberofRetries = numberOfTries - 1;
			_throwedExceptions = 0;

			//When
			Func<int> wrappedFunction = TryCatchBuilder<int>
				.Try(functionToWrap)
				.Catch<Exception>(JustCountTheRetries)
				.ReTry(numberofRetries, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(1))
				.OnRepeatedFailure(actionOnRepeatedFail)
				.Build();
			wrappedFunction();

			//Then
			Assert.That(hasOnRepeatedFailActionPerformed, Is.True);
			Assert.That(_throwedExceptions, Is.EqualTo(numberOfTries));
		}

		[Test]
		public void
			GivenFunctionAFewTimesFailingButFinallySucceeding_WhenWrappingSimpleFunctionFunctionWithCatchAndExecuting_ShouldNotPerformRepeatedFailureAction
			()
		{
			//Given
			int numberOfFailedTries = 2;
			int numberOfTries = 4;
			int numberofRetries = numberOfTries - 1;
			Func<int> functionToWrap = () =>
			{
				if (_throwedExceptions < numberOfFailedTries)
				{
					throw new Exception();
				}

				return 1;
			};
			bool hasOnRepeatedFailActionPerformed = false;
			Action actionOnRepeatedFail = () => hasOnRepeatedFailActionPerformed = true;
			_throwedExceptions = 0;

			//When
			Func<int> wrappedFunction = TryCatchBuilder<int>
				.Try(functionToWrap)
				.Catch<Exception>(JustCountTheRetries)
				.ReTry(numberofRetries, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(1))
				.OnRepeatedFailure(actionOnRepeatedFail)
				.Build();
			wrappedFunction();

			//Then
			Assert.That(hasOnRepeatedFailActionPerformed, Is.False);
			Assert.That(_throwedExceptions, Is.EqualTo(numberOfFailedTries));
		}

		private int FunctionToPerformByTryCatchBuilder(bool shouldThrowException)
		{
			if (shouldThrowException)
				throw new Exception("Just executing the orders here :)");

			return 1;
		}

		private void JustCountTheRetries(Exception exception)
		{
			++_throwedExceptions;
		}
	}
}