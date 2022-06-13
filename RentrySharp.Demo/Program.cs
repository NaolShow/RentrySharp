namespace RentrySharp.Demo {

    public class Program {

        private static void Main(string[] args) {

            // Create a paste with a random id/password
            Paste paste = new Paste();
            paste.Create("Creating a paste on https://rentry.co using RentrySharp");
            Console.WriteLine($"Created a Paste with Id={paste.Id} and Password={paste.Password} (Uri={paste.Uri})");

            // Wait for the user to press enter
            Console.WriteLine("Press ENTER to delete the Paste");
            Console.ReadLine();
            paste.Delete();

        }

    }

}