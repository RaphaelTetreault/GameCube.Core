using System;

namespace GameCube.GCI;

/// <summary>
///     TODO
/// </summary>
internal class InvalidGciException : Exception
{
    public InvalidGciException()
    {
    }

    public InvalidGciException(string message)
        : base (message)
    {
    }

    public InvalidGciException (string message, Exception innerException)
        : base (message, innerException)
    {
    }

}
