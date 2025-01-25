namespace RentrySharp;

/// <summary>
/// Thrown when a <see cref="Paste"/> with the same <see cref="Paste.Id"/> already exists
/// </summary>
public class PasteAlreadyExistsException : Exception {

    /// <inheritdoc cref="Exception.Message"/>
    public override string Message => $"There is already another {nameof(Paste)} that has the same {nameof(Paste.Id)}";

    /// <inheritdoc cref="Exception()"/>
    public PasteAlreadyExistsException() {

    }

    /// <inheritdoc cref="Exception(string)"/>
    public PasteAlreadyExistsException(string message) : base(message) {

    }

    /// <inheritdoc cref="Exception(string, Exception)"/>
    public PasteAlreadyExistsException(string message, Exception innerException) : base(message, innerException) {

    }

}