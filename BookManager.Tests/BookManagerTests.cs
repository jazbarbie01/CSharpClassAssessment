using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookManager; // Assuming your Program class is in this namespace
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BookManager.Tests
{
    [TestClass]
    public class BookManagerTests // Ensure class name matches file name
    {
        private static FieldInfo booksField;
        private static MethodInfo addBookMethod;
        private static MethodInfo viewBooksMethod;
        private static MethodInfo removeBookMethod;

        private StringWriter consoleOutput;
        private StringReader consoleInput;
        private TextWriter originalConsoleOut;
        private TextReader originalConsoleIn;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Type programType = typeof(Program); 
            booksField = programType.GetField("books", BindingFlags.NonPublic | BindingFlags.Static);
            addBookMethod = programType.GetMethod("AddBook", BindingFlags.Public | BindingFlags.Static);
            viewBooksMethod = programType.GetMethod("ViewBooks", BindingFlags.Public | BindingFlags.Static);
            removeBookMethod = programType.GetMethod("RemoveBook", BindingFlags.Public | BindingFlags.Static);

            if (booksField == null)
                throw new InvalidOperationException("Could not find the private static field 'books' in Program.cs. Ensure it exists, is static, and is named 'books'.");
            if (addBookMethod == null)
                throw new InvalidOperationException("Could not find the public static method 'AddBook' in Program.cs.");
            if (viewBooksMethod == null)
                throw new InvalidOperationException("Could not find the public static method 'ViewBooks' in Program.cs.");
            if (removeBookMethod == null)
                throw new InvalidOperationException("Could not find the public static method 'RemoveBook' in Program.cs.");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            originalConsoleOut = Console.Out;
            originalConsoleIn = Console.In;
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
            ClearBooksList(); 
        }
        
        private void ClearBooksList()
        {
            var booksList = (List<Book>)booksField.GetValue(null);
            booksList.Clear();
        }

        private List<Book> GetBooksList()
        {
            return (List<Book>)booksField.GetValue(null);
        }

        private void SetConsoleInput(string input)
        {
            consoleInput = new StringReader(input);
            Console.SetIn(consoleInput);
        }

        private string GetConsoleOutput()
        {
            return consoleOutput.ToString();
        }

        // --- AddBook Tests ---

        [TestMethod]
        public void AddBook_UniqueBook_ShouldAdd()
        {
            SetConsoleInput("TestBook1\nTeen\n");
            addBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(1, books.Count, "Book count should be 1.");
            Assert.AreEqual("TestBook1", books[0].Name, "Book name is incorrect.");
            Assert.AreEqual("Teen", books[0].Category, "Book category is incorrect.");
            Assert.IsTrue(GetConsoleOutput().Contains("Book 'TestBook1' added successfully to category 'Teen'."), "Success message not found.");
        }

        [TestMethod]
        public void AddBook_DuplicateBookSameCategory_ShouldNotAdd()
        {
            SetConsoleInput("TestBook2\nAdventure\n");
            addBookMethod.Invoke(null, null);
            consoleOutput = new StringWriter(); 
            Console.SetOut(consoleOutput);

            SetConsoleInput("TestBook2\nAdventure\n");
            addBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(1, books.Count, "Book count should still be 1.");
            Assert.IsTrue(GetConsoleOutput().Contains("Error: Book with name 'TestBook2' already exists in category 'Adventure'."), "Error message for duplicate not found.");
        }

        [TestMethod]
        public void AddBook_SameNameDifferentCategory_ShouldAdd()
        {
            SetConsoleInput("TestBook3\nAI\n");
            addBookMethod.Invoke(null, null);
            consoleOutput = new StringWriter(); 
            Console.SetOut(consoleOutput);

            SetConsoleInput("TestBook3\nGeography\n");
            addBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(2, books.Count, "Book count should be 2.");
            Assert.IsTrue(books.Any(b => b.Name == "TestBook3" && b.Category == "AI"), "First book (AI category) not found.");
            Assert.IsTrue(books.Any(b => b.Name == "TestBook3" && b.Category == "Geography"), "Second book (Geography category) not found.");
            Assert.IsTrue(GetConsoleOutput().Contains("Book 'TestBook3' added successfully to category 'Geography'."), "Success message for second book not found.");
        }

        [TestMethod]
        public void AddBook_InvalidCategory_ShouldNotAdd()
        {
            SetConsoleInput("TestBook4\nInvalidCat\n");
            addBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(0, books.Count, "Book count should be 0 for invalid category.");
            Assert.IsTrue(GetConsoleOutput().Contains("Invalid category selected. Please choose from Teen, Adventure, AI, Geography."), "Invalid category message not found.");
        }

        // --- RemoveBook Tests ---

        [TestMethod]
        public void RemoveBook_ExistingBook_ShouldRemove()
        {
            GetBooksList().Add(new Book { Name = "BookToRemove", Category = "Teen" });
            
            SetConsoleInput("BookToRemove\n");
            removeBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(0, books.Count, "Book count should be 0 after removal.");
            Assert.IsTrue(GetConsoleOutput().Contains("Book 'BookToRemove' from category 'Teen' removed successfully."), "Removal success message not found.");
        }

        [TestMethod]
        public void RemoveBook_NonExistingBook_ShouldShowError()
        {
            SetConsoleInput("NonExistentBook\n");
            removeBookMethod.Invoke(null, null);

            Assert.AreEqual(0, GetBooksList().Count, "Book list should remain empty.");
            Assert.IsTrue(GetConsoleOutput().Contains("Error: Book with name 'NonExistentBook' not found."), "Non-existent book error message not found.");
        }

        [TestMethod]
        public void RemoveBook_MultipleSameName_CorrectCategoryShouldRemove()
        {
            GetBooksList().Add(new Book { Name = "DuplicateName", Category = "Teen" });
            GetBooksList().Add(new Book { Name = "DuplicateName", Category = "AI" });

            SetConsoleInput("DuplicateName\nAI\n"); 
            removeBookMethod.Invoke(null, null);

            var books = GetBooksList();
            Assert.AreEqual(1, books.Count, "Book count should be 1 after specific removal.");
            Assert.AreEqual("Teen", books[0].Category, "Incorrect book removed, 'Teen' category book should remain.");
            Assert.IsTrue(GetConsoleOutput().Contains("Book 'DuplicateName' from category 'AI' removed successfully."), "Specific removal success message not found.");
        }
        
        [TestMethod]
        public void RemoveBook_MultipleSameName_IncorrectCategorySpecified_ShouldShowError()
        {
            GetBooksList().Add(new Book { Name = "DuplicateName", Category = "Teen" });
            GetBooksList().Add(new Book { Name = "DuplicateName", Category = "AI" });

            SetConsoleInput("DuplicateName\nGeography\n"); 
            removeBookMethod.Invoke(null, null);

            var books = GetBooksList();

            Assert.AreEqual(2, books.Count, "Book count should remain 2 if category is incorrect.");
            Assert.IsTrue(GetConsoleOutput().Contains("Error: Book with name 'DuplicateName' and category 'Geography' not found."), "Incorrect category error message not found.");
        }

        // --- ViewBooks Tests ---

        [TestMethod]
        public void ViewBooks_EmptyList_ShouldShowNoBooksMessage()
        {
            viewBooksMethod.Invoke(null, null);
            Assert.IsTrue(GetConsoleOutput().Contains("No books available."), "'No books available' message not found for empty list.");
        }

        [TestMethod]
        public void ViewBooks_WithBooks_ShouldDisplayBooks()
        {
            GetBooksList().Add(new Book { Name = "Book1View", Category = "TeenView" });
            GetBooksList().Add(new Book { Name = "Book2View", Category = "AIView" });

            viewBooksMethod.Invoke(null, null);

            string output = GetConsoleOutput();
            Assert.IsTrue(output.Contains("List of Books:"), "View books header not found.");
            Assert.IsTrue(output.Contains("Book1View") && output.Contains("TeenView"), "Book1 details not found.");
            Assert.IsTrue(output.Contains("Book2View") && output.Contains("AIView"), "Book2 details not found.");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Console.SetOut(originalConsoleOut); 
            Console.SetIn(originalConsoleIn);   
            consoleInput?.Dispose();
            consoleOutput?.Dispose();
        }
    }
}
