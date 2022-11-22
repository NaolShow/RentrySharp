<h1 align="center" style="border-bottom: none">📝 RentrySharp</h1>

---

<h4 align="center">« Interact with https://rentry.co and/or https://rentry.org, a markdown pastebin-like service that allows custom url, password to edit and remove »</h4>

<div align="center">
  
  <a href="https://github.com/NaolShow/RentrySharp/blob/main/LICENSE"><img alt="GitHub license" src="https://img.shields.io/github/license/NaolShow/RentrySharp?style=flat-square"></a>  
  
</div>
<div align="center">

  <a href="https://github.com/NaolShow/RentrySharp/issues"><img alt="GitHub issues" src="https://img.shields.io/github/issues/NaolShow/RentrySharp?style=flat-square"></a>
  <a href="https://github.com/NaolShow/RentrySharp/pulls"><img alt="GitHub pull requests" src="https://img.shields.io/github/issues-pr/NaolShow/RentrySharp?style=flat-square"/></a>
  <a href="https://github.com/NaolShow/RentrySharp/commits/main"><img alt="GitHub last commit" src="https://img.shields.io/github/last-commit/NaolShow/RentrySharp?style=flat-square"/></a>

</div>

---

The library is around **~300 lines of code** and has been made to be **simple**, **lightweight**. There is only one dependency which is:
* [AngleSharp](https://github.com/AngleSharp/AngleSharp)

This dependency is needed because the **JSON Api** of https://rentry.co doesn't allow everything (like deleting a file, modifying it's url and/or code)
And so I need to use the **HTML/Form Api** which requires to get back the errors through **HTML Source** and here's how **AngleSharp** helps me to parse it

# 🚀 Quick Start

As stated before, this library is made to be simple, so there is only one class that lets you interact with the service!

## Paste

## Structure

Quick pseudo-code to see the whole structure of the class 'Paste':
```cs

class Paste:

// - Some static stuff
static Uri RentryUri; // Uri of the service that will be used
static HttpClientHandler HttpClientHandler;
static HttpClient HttpClient;

// - Properties
// Can be null to get a random value on next create
string? Id;
string? Password;

// - Constructors
// Initialize a Paste that will have a random Id/Password on the next Create call
Paste();
// Initialize a Paste to which you will only be able to see it's content
Paste(string id);
// Initialize a Paste to which you will be able to see it's content, edit and remove it
Paste(string id, string password);

// - Actions (async and non async versions!)
// Get the content of the Paste
string Text; or string GetTextAsync();
// Create the Paste on Rentry
void Create(string text); or string CreateAsync(string text);
// Edit the Paste on Rentry (let any value to null to keep the previous one)
void Edit(string? id, string? password, string? text) or void EditAsync(string? id, string? password, string? text)
// Delete the Paste
void Delete(); or void DeleteAsync();
// Determines if the Paste exists
bool Exists; or bool ExistsAsync();

```
(All the code is documented, so if you need more informations, just check it out!)

## Examples

### Create and Delete

```cs
// Create a paste with a random id/password
Paste rentryPaste = new Paste();
rentryPaste.Create("Creating a paste on https://rentry.co using RentrySharp");
Console.WriteLine($"Created a Paste with Id={rentryPaste.Id} and Password={rentryPaste.Password} (Uri={rentryPaste.Uri})");

// Wait for the user to press enter
Console.WriteLine("Press ENTER to delete the Paste");
Console.ReadLine();
rentryPaste.Delete();
```

### Edit

```cs
// Get the paste (https://rentry.co/rentrysharp)
Paste rentryPaste = new Paste("rentrysharp", "pastePassword");

// Edit it's password (other values are null so I keep the Paste's content and id!)
rentryPaste.Edit(password: "myNewPassword");

Console.WriteLine($"Check the Paste and it's new password here: {rentryPaste.Uri}");
```

### Exists and GetText

```cs
// Get the paste (https://rentry.co/rentrysharp)
Paste rentryPaste = new Paste("rentrysharp");

// If the paste exists
if (rentryPaste.Exists) {

    // Write it's content
    Console.WriteLine(rentryPaste.Text);

} else Console.WriteLine($"Sadly... the Paste '{rentryPaste.Id}' doesn't exist!");
```
