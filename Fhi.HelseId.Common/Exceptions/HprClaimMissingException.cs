using System;

namespace Fhi.HelseId.Common.Exceptions;

public class HprClaimMissingException : Exception
{
    public HprClaimMissingException(string outgoingApisName) : base(outgoingApisName) { }
    public HprClaimMissingException() { }
    public HprClaimMissingException(string message, Exception inner) : base(message, inner) { }
}