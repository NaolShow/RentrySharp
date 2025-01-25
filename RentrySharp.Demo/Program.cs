namespace RentrySharp.Demo {

    public class Program {

        private static void Main() {

            // Create a paste with a random id/password
            Paste paste = new Paste();
            paste.Create("Creating a paste on https://rentry.co using RentrySharp");
            Console.WriteLine($"Created a Paste with Id={paste.Id} and Password={paste.Password} (Uri={paste.Uri})");

            //Fetch the text contents of the paste. paste.GetTextAsync is also available
            Console.WriteLine($"New paste contents: {paste.Text}");

            //Update the paste with stored credentials
            paste.Edit(text: "Updating a paste on https://rentry.co using RentrySharp");

            Console.WriteLine($"Updated paste contents: {paste.Text}");

            // Wait for the user to press enter
            Console.WriteLine("Press ENTER to delete the Paste");
            _ = Console.ReadLine();
            paste.Delete();

        }

    }

}