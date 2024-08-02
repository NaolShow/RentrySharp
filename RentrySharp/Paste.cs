using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;

namespace RentrySharp {

    /// <summary>
    /// Represents a <see cref="Paste"/> and lets you perform actions on it
    /// </summary>
    public class Paste {

        private const string CsrfMiddlewareTokenKey = "csrfmiddlewaretoken";
        private const string UrlKey = "url";
        private const string NewUrlKey = "new_url";
        private const string EditCodeKey = "edit_code";
        private const string NewEditCodeKey = "new_edit_code";
        private const string TextKey = "text";
        private const string DeleteKey = "delete";

        /// <summary>
        /// Determines which base address we are going to use to perform requests to Rentry
        /// </summary>
        public static Uri RentryUri {
#pragma warning disable CS8603
            get => HttpClient.BaseAddress; set {
                HttpClient.BaseAddress = value;
                HttpClient.DefaultRequestHeaders.Referrer = value;
            }
#pragma warning restore CS8603
        }

        public static HttpClientHandler HttpClientHandler { get; } = new HttpClientHandler();
        public static HttpClient HttpClient { get; } = new HttpClient(HttpClientHandler);

        static Paste() {

            // If the RentryUri hasn't been set by the user then assume we use rentry.co
            if (RentryUri == null)
                RentryUri = new Uri("https://rentry.co");

        }

        /// <summary>
        /// Returns the <see cref="Uri"/> that points towards the <see cref="Paste"/> (built with <see cref="RentryUri"/>)
        /// </summary>
        public Uri Uri => new Uri(RentryUri, Id);

        /// <summary>
        /// Represents the id of the <see cref="Paste"/><br/>
        /// Can be null before calling <see cref="Create(string)"/> to get a random id
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Id"/> doesn't have between 2 to 100 characters</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Id"/> contains incorrect characters (must only contain latin letters, numbers, underscores or hyphens)</exception>
        public string? Id {
            get => id; set {

                // If the id is too long or too short
                if (value != null && (value.Length > 100 || value.Length < 2))
                    throw new ArgumentOutOfRangeException(nameof(Id), $"{nameof(Id)} must have a length between 2 to 100 characters");
                // If the id contains invalid caracters
                if (value != null && (!Regex.IsMatch(value, @"^[A-Za-z0-9-_]+$", RegexOptions.Compiled)))
                    throw new ArgumentException($"{nameof(Id)} must only contain latin letters, numbers, underscores or hyphens", nameof(Id));
                id = value;

            }
        }
        private string? id;

        /// <summary>
        /// Represents the password of the <see cref="Paste"/> (required in order to use <see cref="Edit(string?, string?, string?)"/> and <see cref="Delete"/>)<br/>
        /// Can be null before calling <see cref="Create(string)"/> to get a random password
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Password"/> doesn't have between 2 to 100 characters</exception>
        public string? Password {
            get => password; set {

                // If the password is too long or too short
                if (value != null && (value.Length > 100 || value.Length < 2))
                    throw new ArgumentOutOfRangeException(nameof(Password), $"{nameof(Password)} must have between 2 to 100 characters");
                password = value;

            }
        }
        private string? password;

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="Paste"/> that will have a random <see cref="Id"/> and <see cref="Password"/> on the next <see cref="Create(string)"/> call
        /// </summary>
        public Paste() {

        }

        /// <summary>
        /// Creates an instance of <see cref="Paste"/> to which you can just see it's content
        /// </summary>
        /// <param name="id">Id of the <see cref="Paste"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Id"/> doesn't have between 2 to 100 characters</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Id"/> contains incorrect characters (must only contain latin letters, numbers, underscores or hyphens)</exception>
        public Paste(string id) {
            Id = id;
        }

        /// <summary>
        /// Creates an instance of <see cref="Paste"/> to which you can see it's content and edit/delete it
        /// </summary>
        /// <param name="id">Id of the <see cref="Paste"/></param>
        /// <param name="password">Password of the <see cref="Paste"/> (everyone can see your paste, they just need the password to edit it)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="Id"/> and/or the <see cref="Password"/> doesn't have between 2 to 100 characters</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Id"/> contains incorrect characters (must only contain latin letters, numbers, underscores or hyphens)</exception>
        public Paste(string id, string password) {
            Id = id;
            Password = password;
        }

        #endregion

        #region GetText methods

        /// <summary>
        /// Represents the text of the <see cref="Paste"/>
        /// </summary>
        /// <returns>The text of the <see cref="Paste"/></returns>
        /// <exception cref="PasteNotFoundException">Thrown when the <see cref="Paste"/> doesn't exist</exception>
        public async Task<string> GetTextAsync() {

            // Get the response of the raw request
            HttpResponseMessage response = await HttpClient.GetAsync($"{Id}/raw");

            // If the paste doesn't exist anymore
            if (!response.IsSuccessStatusCode) throw new PasteNotFoundException();

            return await response.Content.ReadAsStringAsync();

        }
        /// <inheritdoc cref="GetTextAsync"/>
        public string Text => GetTextAsync().GetAwaiter().GetResult();

        #endregion

        #region Create methods

        /// <summary>
        /// Creates the <see cref="Paste"/> to Rentry with the specified text
        /// </summary>
        /// <param name="text">Content of the <see cref="Paste"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="text"/> is too long</exception>
        /// <exception cref="PasteAlreadyExistException">Thrown when a <see cref="Paste"/> with the same <paramref name="id"/> already exist</exception>
        /// <exception cref="Exception">Thrown when a non handled exception occurs from the service's side</exception>
        public async Task CreateAsync(string? text) {

            // If the text is too long
            if (text != null && text.Length > 200000)
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} mustn't exceed 200,000 characters");

            // Send the paste create request and get the response
            HttpResponseMessage response = await HttpClient.PostAsync(string.Empty, new FormUrlEncodedContent(new Dictionary<string, string?>() {
                { CsrfMiddlewareTokenKey, await GetCsrf() },
                { UrlKey, Id },
                { TextKey, text }
            }));

            // Handle the exceptions
            IHtmlDocument document = await HandleExceptions(response);

            // If either the id and/or the password are randomly generated
            Id ??= response.RequestMessage?.RequestUri?.Segments.LastOrDefault();
            Password ??= document.QuerySelector(".edit-code span")?.TextContent;

            // If after that one of them (or both) are null then throw
            if (Id == null || Password == null) throw new Exception($"Cannot extract the paste {nameof(Id)} and/or {nameof(Password)} from the service's response");

        }
        /// <inheritdoc cref="CreateAsync(string)"/>
        public void Create(string text) => CreateAsync(text).GetAwaiter().GetResult();

        #endregion
        #region Edit methods

        /// <summary>
        /// Edits the <see cref="Paste"/>'s <see cref="Id"/>, <see cref="Password"/> and/or Text
        /// </summary>
        /// <param name="id">New <see cref="Id"/> of the <see cref="Paste"/></param>
        /// <param name="password">New <see cref="Password"/> of the <see cref="Paste"/></param>
        /// <param name="text">New text of the <see cref="Paste"/></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the one of the parameter is either too long or too short</exception>
        /// <exception cref="ArgumentException">Thrown when the <see cref="Id"/> contains incorrect characters (must only contain latin letters, numbers, underscores or hyphens)</exception>
        /// <exception cref="PasteNotFoundException">Thrown when the <see cref="Paste"/> doesn't exist</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when you try to edit a <see cref="Paste"/> when you don't have it's <see cref="Password"/></exception>
        /// <exception cref="PasteAlreadyExistException">Thrown when a <see cref="Paste"/> with the same <paramref name="id"/> already exist</exception>
        public async Task EditAsync(string? id = null, string? password = null, string? text = null) {

            // Save the current password
            string? currentPassword = Password;

            // If the id and/or the password should get changed, save them to validate them
            if (id != null) Id = id;
            if (password != null) Password = password;

            // If the text is too long
            if (text != null && text.Length > 200000)
                throw new ArgumentOutOfRangeException(nameof(text), $"{nameof(text)} mustn't exceed 200,000 characters");

            // Send the request and get the response
            HttpResponseMessage response = await HttpClient.PostAsync($"{Id}/edit", new FormUrlEncodedContent(new Dictionary<string, string?>() {

                // Authenticate with the csrf token and the password
                { CsrfMiddlewareTokenKey, await GetCsrf() },
                { EditCodeKey, currentPassword },

                // Give the (potentially) new id, password and text
                { NewUrlKey, (id == null) ? null : Id },
                { NewEditCodeKey, (password == null) ? null : Password },
                // TODO: I could cache this value a certain amount of time instead of getting back each time?
                // We must set back the text each time (even if we just change id/password) else the text will just be lost
                { TextKey, text ?? await GetTextAsync() }

            }));

            // Handle the exceptions
            await HandleExceptions(response);

        }
        /// <inheritdoc cref="EditAsync(string?, string?, string?)"/>
        public void Edit(string? id = null, string? password = null, string? text = null) => EditAsync(id, password, text).GetAwaiter().GetResult();

        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes the <see cref="Paste"/>
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when you try to delete a <see cref="Paste"/> when you don't have it's <see cref="Password"/></exception>
        /// <exception cref="PasteNotFoundException">Thrown when the <see cref="Paste"/> doesn't exist</exception>
        /// <exception cref="Exception">Thrown when a non handled exception occurs from the service's side</exception>
        public async Task DeleteAsync() {

            // Send the request and get the response
            HttpResponseMessage response = await HttpClient.PostAsync($"{Id}/edit", new FormUrlEncodedContent(new Dictionary<string, string?>() {
                
                // Authenticate with the csrf token and the password
                { CsrfMiddlewareTokenKey, await GetCsrf() },
                { EditCodeKey, Password },

                // Tell that we want to delete the paste
                { DeleteKey, DeleteKey }

            }));

            // Handle the exceptions
            await HandleExceptions(response);

        }
        /// <inheritdoc cref="DeleteAsync"/>
        public void Delete() => DeleteAsync().GetAwaiter().GetResult();

        #endregion
        #region Exists methods

        /// <summary>
        /// Determines if the <see cref="Paste"/> exists
        /// </summary>
        /// <returns>True if the <see cref="Paste"/> exists</returns>
        public async Task<bool> ExistsAsync() => (await HttpClient.GetAsync($"{Id}/raw")).IsSuccessStatusCode;
        /// <inheritdoc cref="ExistsAsync"/>
        public bool Exists => ExistsAsync().GetAwaiter().GetResult();

        #endregion

        /// <summary>
        /// Return's a string representation of the <see cref="Paste"/> that combines it's <see cref="Id"/> and <see cref="Password"/> in the format:<br/>
        /// Uri=<see cref="Uri"/>;Id=<see cref="Id"/>;Password=<see cref="Password"/>
        /// </summary>
        /// <returns>A string representation of the <see cref="Paste"/></returns>
        public override string ToString() => $"Uri={Uri};Id={Id};Password{Password}";

        internal async Task<string> GetCsrf() {

            // Try to get the csrf token from the cookies container
            string? value = HttpClientHandler.CookieContainer.GetAllCookies().FirstOrDefault(a => a.Name == "csrftoken")?.Value;

            // If the cookie is in the container then return it directly
            if (value != null) return value;

            // Do a simple request to rentry to get the csrf cookie and then return it
            await HttpClient.GetAsync(string.Empty);
            return await GetCsrf();

        }

        private const string InvalidEditCode = "Invalid edit code.";
        private const string InvalidAlreadyExist = "Entry with this url already exists.";

        private static async Task<IHtmlDocument> HandleExceptions(HttpResponseMessage response) {

            // If the paste cannot be found
            if (!response.IsSuccessStatusCode) throw new PasteNotFoundException();

            // Parse the response as an html document
            IHtmlDocument document = await new HtmlParser().ParseDocumentAsync(await response.Content.ReadAsStringAsync());

            // If there is an error
            IElement? element = document.QuerySelector(".errorlist");
            if (element != null) {

                // Check which error is it and throw the according exception
                throw element.TextContent switch {
                    InvalidEditCode => new UnauthorizedAccessException(),
                    InvalidAlreadyExist => new PasteAlreadyExistException(),
                    _ => new Exception(element.TextContent),
                };
            }
            return document;

        }

    }

}
