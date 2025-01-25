namespace RentrySharp;

/// <summary>
/// Thrown when the <see cref="Paste"/> with the specified <see cref="Paste.Id"/> doesn't exist
/// </summary>
public class PasteNotFoundException : Exception {

    /// <inheritdoc cref="Exception.Message"/>
    public override string Message => $"No {nameof(Paste)} with the specified {nameof(Paste.Id)} was found";

    /// <inheritdoc cref="Exception()"/>
    public PasteNotFoundException() {

    }

    /// <inheritdoc cref="Exception(string)"/>
    public PasteNotFoundException(string message) : base(message) {

    }

    /// <inheritdoc cref="Exception(string, Exception)"/>
    public PasteNotFoundException(string message, Exception innerException) : base(message, innerException) {

    }

}