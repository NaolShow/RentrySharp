namespace RentrySharp;

/// <summary>
/// Thrown when the <see cref="Paste"/> with the specified <see cref="Paste.Id"/> doesn't exist
/// </summary>
public class PasteNotFoundException : Exception {

    public override string Message => $"The {nameof(Paste)} with the specified {nameof(Paste.Id)} doesn't exist";

}