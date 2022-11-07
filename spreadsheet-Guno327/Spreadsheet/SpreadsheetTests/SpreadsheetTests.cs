using NuGet.Frameworks;
using SpreadsheetUtilities;
using SS;
using System.Text;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
        //GetNamesOfAllNonemptyCells
        [TestMethod]
        public void TestGetCellsSimple()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "100");
            s.SetContentsOfCell("A2", "Balls");
            s.SetContentsOfCell("A3", "=3 + 3");
            IEnumerable<string> result = s.GetNamesOfAllNonemptyCells();
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Contains("A1"));
            Assert.IsTrue(result.Contains("A2"));
            Assert.IsTrue(result.Contains("A3"));
        }
        [TestMethod]
        public void TestGetCellsComplex()
        {
            Spreadsheet s = new();
            List<string> names = new();
            for(int i = 1; i < 500; i++)
            {
                s.SetContentsOfCell("A" + i, "" + i);
                names.Add("A" + i);
            }
            IEnumerable<string> result = s.GetNamesOfAllNonemptyCells();
            Assert.AreEqual(499, result.Count());
            
            foreach(string n in names)
                Assert.IsTrue(result.Contains(n));
        }
        //GetCellContents
        [TestMethod]
        public void GetStringTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "hamborger");
            Assert.AreEqual("hamborger", s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetNumberTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "10");
            Assert.AreEqual(Double.Parse("10"), s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetDoubleTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "10.0");
            Assert.AreEqual(Double.Parse("10.0"), s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetDoubleLongTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "10.000");
            Assert.AreEqual(Double.Parse("10.000"), s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetDoubleSciTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "1e1");
            Assert.AreEqual(Double.Parse("1e1"), s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetFormulaTest()
        {
            Spreadsheet s = new();
            Formula f = new("A3 + A2 * 3");
            s.SetContentsOfCell("A1", "=A3 + A2 * 3");
            Assert.AreEqual(f, s.GetCellContents("A1"));
        }
        [TestMethod]
        public void GetErrorTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "1e1");
            Assert.ThrowsException<InvalidNameException>(() => s.GetCellContents("1_1"));
        }
        [TestMethod]
        public void GetEmptyTest()
        {
            Spreadsheet s = new();
            Assert.AreEqual("", s.GetCellContents("a1"));
        }
        //SetCellContents
        [TestMethod]
        public void SetSameTypeTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("a1", "10");
            Assert.AreEqual(Double.Parse("10"), s.GetCellContents("a1"));
            s.SetContentsOfCell("a1", "100");
            Assert.AreEqual(Double.Parse("100"), s.GetCellContents("a1"));
        }
        [TestMethod]
        public void SetDiffTypeTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("a1", "10");
            Assert.AreEqual(Double.Parse("10"), s.GetCellContents("a1"));
            s.SetContentsOfCell("a1", "ten");
            Assert.AreEqual("ten", s.GetCellContents("a1"));
        }
        [TestMethod]
        public void SetEmptyTypeTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("a1", "10");
            Assert.AreEqual(Double.Parse("10"), s.GetCellContents("a1"));
            s.SetContentsOfCell("a1", "");
            Assert.AreEqual("", s.GetCellContents("a1"));
        }
        [TestMethod]
        public void SetTwiceTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("a1", "10");
            Assert.AreEqual(Double.Parse("10"), s.GetCellContents("a1"));
            s.SetContentsOfCell("a1", "");
            Assert.AreEqual("", s.GetCellContents("a1"));
            Formula f = new("3 + 3 +5");
            s.SetContentsOfCell("a1", "=" + f);
            Assert.AreEqual(f, s.GetCellContents("a1"));
        }
        [TestMethod]
        public void SetErrorDoubleTest()
        {
            Spreadsheet s = new();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("1_1", "1000"));
        }
        [TestMethod]
        public void SetErrorStringTest()
        {
            Spreadsheet s = new();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("1_1", "k"));
        }
        [TestMethod]
        public void SetErrorFormTest()
        {
            Spreadsheet s = new();
            Assert.ThrowsException<InvalidNameException>(() => s.SetContentsOfCell("A&$#1", "=A2"));
        }

        [TestMethod]
        public void SetListFormCheck()
        {
            Spreadsheet s = new();
            Formula f = new("A2 + A3");
            s.SetContentsOfCell("A2", "100");
            s.SetContentsOfCell("A3", "5");
            s.SetContentsOfCell("A1", "=" + f);
            List<string> l = s.SetContentsOfCell("A2", "=A3").ToList();
            Assert.AreEqual(2, l.Count);
            Assert.IsTrue(l.Contains("A1"));
            Assert.IsTrue(l.Contains("A2"));
        }
        [TestMethod]
        public void SetListDoubleCheck()
        {
            Spreadsheet s = new();
            Formula f = new("A1 + 5");
            s.SetContentsOfCell("A2", "=" + f);
            s.SetContentsOfCell("A1", "10");
            List<string> l = s.SetContentsOfCell("A1", "20").ToList();
            Assert.AreEqual(2, l.Count);
            Assert.IsTrue(l.Contains("A1"));
            Assert.IsTrue(l.Contains("A2"));
        }
        [TestMethod]
        public void SetListStringCheck()
        {
            Spreadsheet s = new();
            Formula f = new("A1 + 5");
            s.SetContentsOfCell("A2", "=" + f);
            s.SetContentsOfCell("A1", "10");
            List<string> l = s.SetContentsOfCell("A1", "ten").ToList();
            Assert.AreEqual(2, l.Count);
            Assert.IsTrue(l.Contains("A1"));
            Assert.IsTrue(l.Contains("A2"));
        }
        [TestMethod]
        public void ReplaceFormWithStringTest()
        {
            Spreadsheet s = new();
            Formula f = new("A1 + 5");
            s.SetContentsOfCell("A1", "100");
            s.SetContentsOfCell("A2", "=" + f);
            List<string> l = s.SetContentsOfCell("A2", "empty").ToList();
            Assert.AreEqual(1, l.Count);
            Assert.IsTrue(l.Contains("A2"));
        }
        [TestMethod]
        public void ReplaceFormWithDoubleTest()
        {
            Spreadsheet s = new();
            Formula f = new("A1 + 5");
            Formula f2 = new("A3");
            s.SetContentsOfCell("A1", "=" + f2);
            s.SetContentsOfCell("A3", "100");
            s.SetContentsOfCell("A2", "=" + f);
            List<string> l2 = s.SetContentsOfCell("A2", "100").ToList();
            Assert.AreEqual(1, l2.Count());
            Assert.IsTrue(l2.Contains("A2"));
        }
        [TestMethod]
        public void ReplaceFormWithFormTest()
        {
            Spreadsheet s = new();
            Formula f = new("A1 + 5");
            Formula f2 = new("A3");
            s.SetContentsOfCell("A1", "100");
            s.SetContentsOfCell("A2", "=" + f);
            s.SetContentsOfCell("A3", "5");
            List<string> l1 = s.SetContentsOfCell("A1", "50").ToList();
            s.SetContentsOfCell("A2", "=" + f2);
            List<string> l2 = s.SetContentsOfCell("A1", "100").ToList();
            Assert.AreEqual(2, l1.Count);
            Assert.AreEqual(1, l2.Count);
        }

        [TestMethod]
        public void LinearTest()
        {
            Spreadsheet s = new();
            Random r = new();
            List<string> actual1 = new();

            //Make a linear set of dependencies
            for(int i = 1; i < 1000; i++)
            {
                Formula f = new("a" + (i - 1));
                s.SetContentsOfCell("a" + i, "=" + f);
                actual1.Add("a" + i);
            }

            //Change the first link, should update entire chain
            List<string> result1 = s.SetContentsOfCell("a1", "100").ToList();
            Assert.AreEqual(actual1.Count, result1.Count);
            foreach(string name in actual1)
                Assert.IsTrue(result1.Contains(name));
        }

        [TestMethod]
        public void CircularTest()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            Assert.ThrowsException<CircularException>(() => s.SetContentsOfCell("A2", "=A1"));
        }

        //*NEW TEST*\\

        //Test read/write
        [TestMethod]
        public void SimpleReadWrite()
        {
            Spreadsheet org = new();
            org.SetContentsOfCell("A1", "100.0");
            org.SetContentsOfCell("A2", "12.2");
            org.SetContentsOfCell("A3", "=A1 + A2");
            org.SetContentsOfCell("A4", "Math");
            org.Save("test.txt");

            Spreadsheet reb = new("test.txt", s => true, s => s, "default");
            File.Delete("test.txt");

            //Make all of the cells are the same
            foreach (string s in org.GetNamesOfAllNonemptyCells())
            {
                Assert.IsTrue(reb.GetNamesOfAllNonemptyCells().Contains(s));
                Assert.AreEqual(reb.GetCellValue(s), org.GetCellValue(s));
                Assert.AreEqual(reb.GetCellContents(s), org.GetCellContents(s));
            }

        }
        [TestMethod]
        public void LinearBigReadWrite()
        {
            Spreadsheet org = new();
            for (int i = 1; i < 1000; i++)
            {
                Formula f = new("a" + (i - 1));
                org.SetContentsOfCell("a" + i, "=" + f);
            }

            org.Save("test.txt");

            Spreadsheet reb = new("test.txt", s => true, s => s, "default");
            File.Delete("test.txt");

            //Make all of the cells are the same
            foreach (string s in org.GetNamesOfAllNonemptyCells())
            {
                Assert.IsTrue(reb.GetNamesOfAllNonemptyCells().Contains(s));
                Assert.AreEqual(reb.GetCellValue(s), org.GetCellValue(s));
                Assert.AreEqual(reb.GetCellContents(s), org.GetCellContents(s));
            }

        }
        [TestMethod]
        public void IncorrectFilepathSave()
        {
            Spreadsheet s = new();
            Assert.ThrowsException<SpreadsheetReadWriteException>(() => s.Save("/doesntexsist/notevernclose/file.txt"));
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void IncorrectFilepathLoad()
        {
            Spreadsheet s = new("/doesntexsist/notevernclose/file.txt", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void MismatchVersionTest()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "stuff");
            s.Save("test.txt");
            Spreadsheet s2 = new("test.txt", s => true, s => s, "version 2");
        }

        //More tests
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void MeddlingWithJsonForm()
        {
            Spreadsheet s = new(s => true, s => s, "1");
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "abc");
            s.Save("test.txt");

            string json = File.ReadAllText("test.txt");
            char[] chars = json.ToCharArray();
            chars[56] = '=';
            chars[57] = 'A';
            chars[58] = '1';

            File.Delete("test.txt");
            File.WriteAllText("test.txt", chars.ToString());
            Spreadsheet s2 = new("test.txt", s => true, s => s, "1");
        }

        [TestMethod]
        [ExpectedException (typeof(InvalidNameException))]
        public void TestGetInvalidName()
        {
            Spreadsheet s = new();
            s.GetCellValue("1a2");
        }

        [TestMethod]
        public void TestCircularOldString()
        {
            Spreadsheet s = new();
            try
            {
                s.SetContentsOfCell("A1", "=A2");
                s.SetContentsOfCell("A2", "bruh");
                s.SetContentsOfCell("A2", "=A1");
            }
            catch
            {
                Assert.AreEqual("bruh", s.GetCellValue("A2"));
            }
        }

        [TestMethod]
        public void TestCircularRepairDG()
        {
            Spreadsheet s = new();
            try
            {
                s.SetContentsOfCell("A1", "1");
                s.SetContentsOfCell("A2", "=A1 + A3");
                s.SetContentsOfCell("A2", "=A2");
            }
            catch
            {
                Assert.AreEqual(2, s.SetContentsOfCell("A1", "2").ToList().Count);
                Assert.IsTrue(s.SetContentsOfCell("A1", "2").ToList().Contains("A1"));
                Assert.IsTrue(s.SetContentsOfCell("A1", "2").ToList().Contains("A2"));
            }
        }

        [TestMethod]
        [ExpectedException (typeof(InvalidNameException))]
        public void VariableNameTestWord()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("apple", "apple");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void VariableNameTestMisorder()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1A1", "1");
        }

        [TestMethod]
        public void FormulaWithBadVar()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=A1A11");
            Assert.IsTrue(s.GetCellValue("A1").GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void FormulaWithBadVarInsideFormula()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "=A1A11");
            s.SetContentsOfCell("A2", "=A1");
            Assert.IsTrue(s.GetCellValue("A2").GetType() == typeof(FormulaError));
        }

        [TestMethod]
        public void FormulaEvaluation()
        {
            Spreadsheet s = new();
            s.SetContentsOfCell("A1", "10");
            s.SetContentsOfCell("A2", "5");
            s.SetContentsOfCell("A3", "=A1 + A2");
            Assert.AreEqual(Double.Parse("15"), s.GetCellValue("A3"));
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void ReadRandomFile()
        {
            File.WriteAllText("test.txt", "bruh momento");
            Spreadsheet s = new("test.txt", s => true, s => s, "default");
        }
    }
}