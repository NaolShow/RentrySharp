namespace RentrySharp;

/// <summary>
/// Thrown when a <see cref="Paste"/> with the same specified <see cref="Paste.Id"/> already exist
/// </summary>
public class PasteAlreadyExistException : Exception {

    public override string Message => $"There is already another {nameof(Paste)} that have the same specified {nameof(Paste.Id)}";

}