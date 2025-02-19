using System;

namespace Fhi.HelseId.Common.Exceptions;

public class NoContextException : Exception
{
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    public NoContextException()
    {
    }

    public NoContextException(string message) : base(message)
    {
    }

    public NoContextException(string message, Exception inner) : base(message, inner)
    {
    }
}
