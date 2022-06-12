using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;

namespace RentrySharp {

    /// <summary>
    /// Class that lets you perform global actions on Rentry
    /// </summary>
    public class RentryClient {

        private HttpClientHandler handler;
        internal HttpClient httpClient;

#pragma warning disable CS8603
        /// <summary>
        /// Represents the base address of the Rentry service (can be set through the 
        /// </summary>
        public Uri Uri { get => httpClient.BaseAddress; set => httpClient.BaseAddress = value; }
#pragma warning restore CS8603

        /// <summary>
        /// Initializes a <see cref="RentryClient"/> that will use the <see cref="Uri"/> <see href="https://rentry.co"/> for each calls
        /// </summary>
        public RentryClient() : this(new Uri("https://rentry.co")) {

        }

        /// <summary>
        /// Initializes a <see cref="RentryClient"/> that will use the specified <see cref="Uri"/> for each calls
        /// </summary>
        /// <param name="rentryUri">The <see cref="Uri"/> that the <see cref="RentryClient"/> will use for each calls</param>
        public RentryClient(Uri rentryUri) {

            // Initialize an handler for the http client (in order to save the cookies)
            handler = new HttpClientHandler();
            // Initialize the http client using the handler
            httpClient = new HttpClient(handler);

            // Set the httpclient base address and it's referrer
            httpClient.BaseAddress = rentryUri;
            httpClient.DefaultRequestHeaders.Referrer = rentryUri;

        }

        #region Create methods

        /// <summary>
        /// Creates a new <see cref="Paste"/> with the specified id, password and text
        /// </summary>
        /// <param name="id">Id of the <see cref="Paste"/> (must only contain latin letters, numbers, underscores or hyphens and have between 2 to 100 characters</param>
        /// <param name="password">Password of the <see cref="Paste"/> (must have between 2 to 100 characters)</param>
        /// <param name="text">Text of the <see cref="Paste"/> (mustn't exceed 200,000 characters)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the argument is either too long or too short</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="id"/> contains invalid characters</exception>
        /// <exception cref="PasteAlreadyExistException">Thrown when a <see cref="Paste"/> with the same <paramref name="id"/> already exist</exception>
        /// <exception cref="Exception">Thrown when a non handled exception occurs from the service's side</exception>
        public async Task<Paste> CreateAsync(string? id, string? password, string text) {

            // Validate the paste informations (id/password and text length, id characters set...)
            ValidatePaste(id, password, text);

            // Send the paste create request and get the response
            HttpResponseMessage response = await httpClient.PostAsync(string.Empty, new FormUrlEncodedContent(new Dictionary<string, string?>() {
                { "csrfmiddlewaretoken", GetCsrf() },
                { "url", id },
                { "edit_code", password },
                { "text", text }
            }));

            // Handle the exceptions
            IHtmlDocument document = await HandleExceptions(response);

            // If either the id and/or the password are randomly
            if (id == null) id = response.RequestMessage?.RequestUri?.Segments.LastOrDefault();
            if (password == null) password = document.QuerySelector(".edit-code")?.TextContent;

            // If after that one of them (or both) are null then throw
            if (id == null || password == null) throw new Exception($"Cannot extract the paste {nameof(id)} and/or {nameof(password)} from the service's response");

            // Return the new paste instance
            return new Paste(this, id, password);

        }
        /// <inheritdoc cref="CreateAsync(string?, string?, string)"/>
        public Paste Create(string? id, string? password, string text) => CreateAsync(id, password, text).GetAwaiter().GetResult();

        /// <summary>
        /// Creates a new <see cref="Paste"/> with the specified text<br/>
        /// (A random <see cref="Paste.Id"/> and <see cref="Paste.Password"/> will be generated)
        /// </summary>
        /// <param name="text">Text of the <see cref="Paste"/> (mustn't exceed 200,000 characters)</param>
        /// <inheritdoc cref="CreateAsync(string?, string?, string)"/>
        public async Task<Paste> CreateAsync(string text) => await CreateAsync(null, null, text);

        /// <inheritdoc cref="CreateAsync(string)"/>
        public Paste Create(string text) => CreateAsync(text).GetAwaiter().GetResult();

        #endregion
        #region Get methods

        /// <inheritdoc cref="Paste(RentryClient, string)"/>
        public Paste Get(string id) => new Paste(this, id);
        /// <inheritdoc cref="Paste(RentryClient, string, string)"/>
        public Paste Get(string id, string password) => new Paste(this, id, password);

        #endregion

        internal string GetCsrf() {

            // Try to get the csrf token from the cookies container
            string? value = handler.CookieContainer.GetAllCookies().FirstOrDefault(a => a.Name == "csrftoken")?.Value;

            // If the cookie is in the container then return it directly
            if (value != null) return value;

            // Do a simple request to rentry to get the csrf cookie and then return it
            httpClient.GetAsync("").Wait();
            return GetCsrf();

        }

        internal static void ValidatePaste(string? id, string? password, string? text) {

            // If the id is too long or too short
            if (id != null && (id.Length > 100 || id.Length < 2))
                throw new ArgumentOutOfRangeException(nameof(id), $"{nameof(id)} must have a length between 2 to 100 characters");
            // If the id contains invalid caracters
            if (id != null && (!Regex.IsMatch(id, @"^[A-Za-z0-9-_]+$", RegexOptions.Compiled)))
                throw new ArgumentException($"{nameof(id)} must only contain latin letters, numbers, underscores or hyphens", nameof(id));

            // If the password is too long or too short
            if (password != null && (password.Length > 100 || password.Length < 2))
                throw new ArgumentOutOfRangeException(nameof(password), $"{nameof(password)} must have between 2 to 100 characters");

            // If the text is too long
            if (text != null && text.Length > 200000)
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} mustn't exceed 200,000 characters");

        }

        internal static async Task<IHtmlDocument> HandleExceptions(HttpResponseMessage response) {

            // If the paste cannot be found
            if (!response.IsSuccessStatusCode) throw new PasteNotFoundException();

            // Parse the response as an html document
            IHtmlDocument document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());

            // If there is an error
            IElement? element = document.QuerySelector(".errorlist");
            if (element != null) {

                // Check which error is it and throw the according exception
                throw element.TextContent switch {
                    "Invalid edit code." => new UnauthorizedAccessException(),
                    "Entry with this url already exists." => new PasteAlreadyExistException(),
                    _ => new Exception(element.TextContent),
                };
            }
            return document;

        }

    }

}