using System;

namespace Fhi.HelseId.Common.Exceptions;

public class NotInitializedAuthBuilderException : Exception
{
    public NotInitializedAuthBuilderException() { }
    public NotInitializedAuthBuilderException(string message) : base(message) { }
    public NotInitializedAuthBuilderException(string message, Exception inner) : base(message, inner) { }
}
