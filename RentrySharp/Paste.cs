namespace RentrySharp {

    /// <summary>
    /// Represents a <see cref="Paste"/> and lets you perform actions on it
    /// </summary>
    public class Paste {

        private RentryClient rentryClient;

        /// <summary>
        /// Returns the <see cref="Uri"/> that points towards the <see cref="Paste"/>
        /// </summary>
        public Uri Uri => new Uri(rentryClient.Uri, Id);

        /// <summary>
        /// Represent's the id of the <see cref="Paste"/>
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Represent's the password of the <see cref="Paste"/
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="Paste"/> to which you can just see it's content
        /// </summary>
        /// <param name="id">Id of the <see cref="Paste"/></param>
        internal Paste(RentryClient rentryClient, string id) {
            this.rentryClient = rentryClient;
            Id = id;
        }

        /// <summary>
        /// Creates an instance of <see cref="Paste"/> to which you can see it's content and edit/delete it
        /// </summary>
        /// <param name="id">Id of the <see cref="Paste"/></param>
        /// <param name="password">Password of the <see cref="Paste"/> (everyone can see your paste, they just need the password to edit it)</param>
        internal Paste(RentryClient rentryClient, string id, string password) : this(rentryClient, id) => Password = password;

        #region GetText methods

        /// <summary>
        /// Represents the text of the <see cref="Paste"/>
        /// </summary>
        /// <returns>The text of the <see cref="Paste"/></returns>
        /// <exception cref="PasteNotFoundException">Thrown when the <see cref="Paste"/> doesn't exist</exception>
        public async Task<string> GetTextAsync() {

            // Get the response of the raw request
            HttpResponseMessage response = await rentryClient.httpClient.GetAsync($"{Id}/raw");

            // If the paste doesn't exist anymore
            if (!response.IsSuccessStatusCode) throw new PasteNotFoundException();

            return await response.Content.ReadAsStringAsync();

        }
        /// <inheritdoc cref="GetTextAsync"/>
        public string GetText() => GetTextAsync().GetAwaiter().GetResult();
        #endregion

        #region Create methods

        /// <inheritdoc cref="RentryClient.Create(string?, string?, string)"/>
        public async Task CreateAsync(string text) => await rentryClient.CreateAsync(Id, Password, text);
        /// <inheritdoc cref="RentryClient.Create(string?, string?, string)"/>
        public void Create(string text) => CreateAsync(text).GetAwaiter().GetResult();

        #endregion
        #region Edit methods

        /// <summary>
        /// Edits the <see cref="Paste"/>'s <see cref="Id"/>, <see cref="Password"/> and/or Text
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when you try to edit a <see cref="Paste"/> when you don't have it's <see cref="Password"/></exception>
        /// <exception cref="PasteNotFoundException">Thrown when the <see cref="Paste"/> doesn't exist</exception>
        /// <inheritdoc cref="RentryClient.Create(string?, string?, string)"/>
        public async Task EditAsync(string? id = null, string? password = null, string? text = null) {

            // Validate the paste informations (id/password and text length, id characters set...)
            RentryClient.ValidatePaste(id, password, text);

            // Send the request and get the response
            HttpResponseMessage response = await rentryClient.httpClient.PostAsync($"{Id}/edit", new FormUrlEncodedContent(new Dictionary<string, string?>() {

                // Authenticate with the csrf token and the password
                { "csrfmiddlewaretoken", rentryClient.GetCsrf() },
                { "edit_code", Password },

                // Give the (potentially) new id, password and text
                { "new_url", id },
                { "new_edit_code", password },
                // TODO: I could cache this value a certain amount of time instead of getting back each time?
                // We must set back the text each time (even if we just change id/password) else the text will just be lost
                { "text", text ?? await GetTextAsync() }

            }));

            // Handle the exceptions
            await RentryClient.HandleExceptions(response);

            // Save the id and password if they changed
            if (id != null)
                Id = id;
            if (password != null)
                Password = password;

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
        public async Task DeleteAsync() {

            // Send the request and get the response
            HttpResponseMessage response = await rentryClient.httpClient.PostAsync($"{Id}/edit", new FormUrlEncodedContent(new Dictionary<string, string?>() {
                
                // Authenticate with the csrf token and the password
                { "csrfmiddlewaretoken", rentryClient.GetCsrf() },
                { "edit_code", Password },

                // Tell that we want to delete the paste
                { "delete", "delete"}

            }));

            // Handle the exceptions
            await RentryClient.HandleExceptions(response);

        }
        /// <inheritdoc cref="DeleteAsync"/>
        public void Delete() => DeleteAsync().GetAwaiter().GetResult();

        #endregion
        #region Exists methods

        /// <summary>
        /// Determines if the <see cref="Paste"/> exists
        /// </summary>
        /// <returns>True if the <see cref="Paste"/> exists</returns>
        public async Task<bool> ExistsAsync() => (await rentryClient.httpClient.GetAsync($"{Id}/raw")).IsSuccessStatusCode;
        /// <inheritdoc cref="ExistsAsync"/>
        public bool Exists() => ExistsAsync().GetAwaiter().GetResult();

        #endregion

        /// <summary>
        /// Return's a string representation of the <see cref="Paste"/> that combines it's <see cref="Id"/> and <see cref="Password"/> in the format:<br/>
        /// Id=<see cref="Id"/>;Password=<see cref="Password"/>
        /// </summary>
        /// <returns>A string representation of the <see cref="Paste"/></returns>
        public override string ToString() => $"Id={Id};Password={Password}";

    }

}
