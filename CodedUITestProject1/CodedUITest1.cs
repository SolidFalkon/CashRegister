using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Linq;


namespace CodedUITestProject1
{
    // ========== Тесты для класса Product ==========
    [TestClass]
    public class ProductTests
    {
        [TestMethod]
        public void Product_Properties_SetCorrectly()
        {
            // Arrange
            long article = 1234567890123;
            int quantity = 100;

            // Act
            var product = new Product { Article = article, Quantity = quantity };

            // Assert
            Assert.AreEqual(article, product.Article);
            Assert.AreEqual(quantity, product.Quantity);
        }
    }

    // ========== Тесты для класса Cashier ==========
    [TestClass]
    public class CashierTests
    {
        [TestMethod]
        public void GenerateNewPassword_ReturnsNonEmptyString()
        {
            // Arrange
            var cashier = new Cashier { Login = "test", Password = "old" };

            // Act
            string newPassword = cashier.GenerateNewPassword();

            // Assert
            Assert.IsNotNull(newPassword);
            Assert.IsTrue(newPassword.Length > 0);
            Assert.AreNotEqual("old", newPassword);
        }

        [TestMethod]
        public void GenerateNewPassword_ReturnsDifferentValuesOnMultipleCalls()
        {
            // Arrange
            var cashier = new Cashier { Login = "test", Password = "old" };

            // Act
            string pass1 = cashier.GenerateNewPassword();
            string pass2 = cashier.GenerateNewPassword();

            // Assert
            Assert.AreNotEqual(pass1, pass2);
        }
    }

    // ========== Тесты для класса Autorization ==========
    [TestClass]
    public class AutorizationTests
    {
        [TestMethod]
        public void Authorization_ValidCredentials_ReturnsTrue()
        {
            // Arrange
            var auth = new Autorization();  // в конструкторе добавляется admin/1234

            // Act
            bool result = auth.Authorization("admin", "1234");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Authorization_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var auth = new Autorization();

            // Act
            bool result = auth.Authorization("admin", "wrong");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Authorization_InvalidLogin_ReturnsFalse()
        {
            // Arrange
            var auth = new Autorization();

            // Act
            bool result = auth.Authorization("unknown", "1234");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NewCashier_AddsCashier_AndGeneratesPassword()
        {
            // Arrange
            var auth = new Autorization();
            int initialCount = auth.ReadCashier().Count;

            // Act
            auth.NewCashier("new_cashier");
            var cashiers = auth.ReadCashier();

            // Assert
            Assert.AreEqual(initialCount + 1, cashiers.Count);
            var added = cashiers.Last();
            Assert.AreEqual("new_cashier", added.Login);
            Assert.IsNotNull(added.Password);
            Assert.IsTrue(added.Password.Length > 0);
        }

        [TestMethod]
        public void WriteCashier_ReplacesList()
        {
            // Arrange
            var auth = new Autorization();
            var newList = new List<Cashier> { new Cashier { Login = "temp", Password = "pass" } };

            // Act
            auth.WriteCashier(newList);
            var result = auth.ReadCashier();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("temp", result[0].Login);
        }
    }

    // ========== Тесты для класса Search ==========
    [TestClass]
    public class SearchTests
    {
        private List<Product> _stock;

        [TestInitialize]
        public void Setup()
        {
            _stock = new List<Product>
            {
                new Product { Article = 1111111111111, Quantity = 10 },
                new Product { Article = 2222222222222, Quantity = 20 }
            };
        }

        [TestMethod]
        public void SearchF_ExistingArticle_ReturnsProduct()
        {
            // Arrange
            var search = new Search();
            long article = 1111111111111;

            // Act
            var result = search.SearchF(article, _stock);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(article, result.Article);
            Assert.AreEqual(10, result.Quantity);
        }

        [TestMethod]
        public void SearchF_NonExistingArticle_ReturnsNull()
        {
            // Arrange
            var search = new Search();
            long article = 9999999999999;

            // Act
            var result = search.SearchF(article, _stock);

            // Assert
            Assert.IsNull(result);
        }
    }

    // ========== Тесты для класса Storage ==========
    [TestClass]
    public class StorageTests
    {
        private Storage _storage;

        [TestInitialize]
        public void Setup()
        {
            _storage = new Storage();
            _storage.products.Add(new Product { Article = 1111111111111, Quantity = 100 });
            _storage.products.Add(new Product { Article = 2222222222222, Quantity = 50 });
        }

        [TestMethod]
        public void ReadStock_ReturnsListCopy()
        {
            // Act
            var stock = _storage.ReadStock();

            // Assert
            Assert.AreEqual(2, stock.Count);
            Assert.AreEqual(1111111111111, stock[0].Article);
        }

        [TestMethod]
        public void WriteStock_ReplacesProducts()
        {
            // Arrange
            var newProducts = new List<Product>
            {
                new Product { Article = 3333333333333, Quantity = 30 }
            };

            // Act
            _storage.WriteStock(newProducts);
            var stock = _storage.ReadStock();

            // Assert
            Assert.AreEqual(1, stock.Count);
            Assert.AreEqual(3333333333333, stock[0].Article);
        }

        [TestMethod]
        public void Correct_WithTypeQuantity_SetsQuantity()
        {
            // Arrange
            long article = 1111111111111;
            int newQuantity = 200;

            // Act
            _storage.Correct("quantity", article, newQuantity);
            var product = _storage.products.First(p => p.Article == article);

            // Assert
            Assert.AreEqual(newQuantity, product.Quantity);
        }

        [TestMethod]
        public void Correct_WithUnknownArticle_DoesNothing()
        {
            // Arrange
            long article = 9999999999999;
            int initialCount = _storage.products.Count;

            // Act
            _storage.Correct("quantity", article, 100);

            // Assert
            Assert.AreEqual(initialCount, _storage.products.Count);
        }
    }

    // ========== Тесты для класса Sell ==========
    [TestClass]
    public class SellTests
    {
        private List<Product> _stock;

        [TestInitialize]
        public void Setup()
        {
            _stock = new List<Product>
            {
                new Product { Article = 1111111111111, Quantity = 100 }
            };
        }

        [TestMethod]
        public void SellF_SufficientStock_DecreasesQuantityAndReturnsTrue()
        {
            // Arrange
            var sell = new Sell();
            long article = 1111111111111;
            int quantity = 30;

            // Act
            bool result = sell.SellF(article, quantity, _stock);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(70, _stock[0].Quantity);
        }

        [TestMethod]
        public void SellF_InsufficientStock_ReturnsFalseAndQuantityUnchanged()
        {
            // Arrange
            var sell = new Sell();
            long article = 1111111111111;
            int quantity = 200;
            int initial = _stock[0].Quantity;

            // Act
            bool result = sell.SellF(article, quantity, _stock);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(initial, _stock[0].Quantity);
        }

        [TestMethod]
        public void SellF_NonExistingArticle_ReturnsFalse()
        {
            // Arrange
            var sell = new Sell();
            long article = 9999999999999;
            int quantity = 10;

            // Act
            bool result = sell.SellF(article, quantity, _stock);

            // Assert
            Assert.IsFalse(result);
        }
    }

    // ========== Тесты для класса Return ==========
    [TestClass]
    public class ReturnTests
    {
        private Storage _storage;

        [TestInitialize]
        public void Setup()
        {
            _storage = new Storage();
            _storage.products.Add(new Product { Article = 1111111111111, Quantity = 100 });
        }

        [TestMethod]
        public void Ret_ExistingArticle_IncreasesQuantityAndReturnsTrue()
        {
            // Arrange
            var ret = new Return();
            long article = 1111111111111;
            int quantity = 20;

            // Act
            bool result = ret.Ret(article, quantity, _storage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(120, _storage.products[0].Quantity);
        }

        [TestMethod]
        public void Ret_NonExistingArticle_ReturnsFalse()
        {
            // Arrange
            var ret = new Return();
            long article = 9999999999999;
            int quantity = 10;

            // Act
            bool result = ret.Ret(article, quantity, _storage);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, _storage.products.Count); // список не изменился
        }
    }

    // ========== Тесты для класса Supply ==========
    [TestClass]
    public class SupplyTests
    {
        private Storage _storage;

        [TestInitialize]
        public void Setup()
        {
            _storage = new Storage();
            _storage.products.Add(new Product { Article = 1111111111111, Quantity = 100 });
        }

        [TestMethod]
        public void GetSupply_ExistingArticle_IncreasesQuantity()
        {
            // Arrange
            var supply = new Supply();
            long article = 1111111111111;
            int quantity = 50;

            // Act
            supply.GetSupply(article, quantity, _storage);

            // Assert
            var product = _storage.products.First(p => p.Article == article);
            Assert.AreEqual(150, product.Quantity);
        }

        [TestMethod]
        public void GetSupply_NewArticle_AddsProduct()
        {
            // Arrange
            var supply = new Supply();
            long article = 3333333333333;
            int quantity = 30;

            // Act
            supply.GetSupply(article, quantity, _storage);

            // Assert
            Assert.AreEqual(2, _storage.products.Count);
            var newProduct = _storage.products.First(p => p.Article == article);
            Assert.AreEqual(quantity, newProduct.Quantity);
        }
    }
}