using Microsoft.VisualStudio.TestTools.UnitTesting;


using Mobile_Application_Development.Utility;

namespace Mobile_Application_Development.Tests
{
    [TestClass]
    public class ValidationTests
    {
        [DataTestMethod]
        [DataRow("alice@example.com", true)]
        [DataRow("bob.smith@sub.domain.co", true)]
        [DataRow("bad@", false)]
        [DataRow("@nodomain", false)]
        [DataRow("", false)]
        public void Email_IsValidated(string input, bool expected)
            => Assert.AreEqual(expected, Validation.IsValidEmail(input));

        [DataTestMethod]
        [DataRow("555-123-4567", true)]
        [DataRow("(212) 555 7890", true)]
        [DataRow("+1 415 555 1212", true)]
        [DataRow("12345", false)]
        [DataRow("", false)]
        public void Phone_IsValidated(string input, bool expected)
            => Assert.AreEqual(expected, Validation.IsLikelyPhone(input));

        [TestMethod]
        public void Clean_TrimsAndCollapsesSpaces()
            => Assert.AreEqual("Intro to MAUI", Validation.Clean("  Intro   to   MAUI  "));
    }
}
