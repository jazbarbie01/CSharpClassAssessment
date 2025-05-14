using System;
using System.Collections.Generic;
using System.Linq;

namespace BookManager
{
    class Program
    {
        private static List<Book> books = new List<Book>();

        public static void AddBook()
        {
            Console.WriteLine("Enter the book name:");
            string name = Console.ReadLine();

            Console.WriteLine("Choose a category (Teen, Adventure, AI, Geography):");
            string category = Console.ReadLine();

            List<string> validCategories = new List<string> { "Teen", "Adventure", "AI", "Geography" };
            if (!validCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("Invalid category selected. Please choose from Teen, Adventure, AI, Geography.");
                return;
            }

            if (books.Any(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Error: Book with name '{name}' already exists in category '{category}'.");
                return;
            }

            Book newBook = new Book { Name = name, Category = category };
            books.Add(newBook);

            Console.WriteLine($"Book '{name}' added successfully to category '{category}'.");
        }

        public static void ViewBooks()
        {
            if (!books.Any())
            {
                Console.WriteLine("No books available.");
                return;
            }

            Console.WriteLine("\nList of Books:");
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("{0,-30} | {1,-20}", "Name", "Category");
            Console.WriteLine("--------------------------------------------------");
            foreach (var book in books)
            {
                Console.WriteLine("{0,-30} | {1,-20}", book.Name, book.Category);
            }
            Console.WriteLine("--------------------------------------------------");
        }

        public static void RemoveBook()
        {
            Console.WriteLine("Enter the name of the book to remove:");
            string nameToRemove = Console.ReadLine();

            var foundBooks = books.Where(b => b.Name.Equals(nameToRemove, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!foundBooks.Any())
            {
                Console.WriteLine($"Error: Book with name '{nameToRemove}' not found.");
                return;
            }

            Book bookToRemove;
            if (foundBooks.Count > 1)
            {
                Console.WriteLine($"Multiple books found with the name '{nameToRemove}'. Please specify the category:");
                for (int i = 0; i < foundBooks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {foundBooks[i].Name} ({foundBooks[i].Category})");
                }
                Console.WriteLine("Enter the category of the book to remove:");
                string categoryToRemove = Console.ReadLine();
                bookToRemove = foundBooks.FirstOrDefault(b => b.Category.Equals(categoryToRemove, StringComparison.OrdinalIgnoreCase));

                if (bookToRemove == null)
                {
                    Console.WriteLine($"Error: Book with name '{nameToRemove}' and category '{categoryToRemove}' not found.");
                    return;
                }
            }
            else
            {
                bookToRemove = foundBooks.First();
            }

            books.Remove(bookToRemove);
            Console.WriteLine($"Book '{bookToRemove.Name}' from category '{bookToRemove.Category}' removed successfully.");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Book Manager!");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("\nMenu:");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. View Books");
                Console.WriteLine("3. Remove Book");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();
                Console.WriteLine(); // Add a new line for better readability

                switch (choice)
                {
                    case "1":
                        AddBook();
                        break;
                    case "2":
                        ViewBooks();
                        break;
                    case "3":
                        RemoveBook();
                        break;
                    case "4":
                        exit = true;
                        Console.WriteLine("Exiting Book Manager. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }
    }
}
