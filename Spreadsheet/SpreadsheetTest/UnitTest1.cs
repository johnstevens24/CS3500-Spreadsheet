using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Xml;
using System.Linq;
using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using SpreadsheetUtilities;
using System.Threading;
using System.Xml;

namespace SS
{
    [TestClass()]
    public class Ps5Spreadsheettests
    {
        [TestMethod()]
        public void simpleTest()
        {
            AbstractSpreadsheet sheet1 = new Spreadsheet();
            AbstractSpreadsheet sheet2 = new Spreadsheet(s => true, s => s, "version");
        }

        [TestMethod()]
        public void simpleTest2()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
        }

        [TestMethod()]
        public void moreComplexXmlTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");
            s.Save("moreComplexXmlTest");
        }
        

        //----------------Actual Tests------------------------

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void invalidFilePathTest()
        {
            Spreadsheet s = new Spreadsheet("IncorrectFilePath",s => true, s => s, "1.0");
        }

        [TestMethod()]
        public void validFilePathTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "24");
            s.Save("validFilePathTest");

            Spreadsheet s2 = new Spreadsheet("validFilePathTest", s => true, s => s, "1.0");
        }


        //[TestMethod()]
        //[ExpectedException(typeof(SpreadsheetReadWriteException))]
        //public void incorrectVersionTest()
        //{
        //    Spreadsheet a = new Spreadsheet(s => true, s => s, "2.0");
        //    a.SetContentsOfCell("a1", "2.0");
        //    a.Save("incorrectVersionTest");

        //    Spreadsheet b = new Spreadsheet("incorrectVersionTest", s => true, x => x, "1.0");
        //}

        [TestMethod()]
        public void correctVersionTest()
        {
            Spreadsheet a = new Spreadsheet(s => true, s => s, "2.0");
            a.SetContentsOfCell("a1", "2.0");
            a.Save("correctVersionTest");

            Spreadsheet b = new Spreadsheet("correctVersionTest", s => true, x => x, "2.0");
        }

        
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingIncorrectlyFormattedFileTest()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingIncorrectlyFormattedFileTest", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("bmw");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }



            Spreadsheet a = new Spreadsheet("readingIncorrectlyFormattedFileTest", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingFileWithCircularDependency()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingFileWithCircularDependency", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a1");
                writer.WriteElementString("contents", "=z4");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "b1");
                writer.WriteElementString("contents", "=2/a1");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "z4");
                writer.WriteElementString("contents", "=b1");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            Spreadsheet a = new Spreadsheet("readingFileWithCircularDependency", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void invalidCellName()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("invalidCellName", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A$");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("invalidCellName", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void invalidFormula()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("invalidFormula", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "n54");
                writer.WriteElementString("contents", "=a3 + 24 + !43");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }



            Spreadsheet a = new Spreadsheet("invalidFormula", s => true, x => x, "1.0");
        }


        [TestMethod()]
        public void getCellContentsTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");

            Assert.IsTrue(s.GetCellContents("a1").Equals(2.0));
            Assert.IsTrue(s.GetCellContents("b5").Equals("x4 + 35"));
            Assert.IsTrue(s.GetCellContents("z1").Equals(new Formula("a1-10/4")));
            Assert.IsTrue(s.GetCellContents("b58").Equals("budget: "));

        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void getCellContentsTest2()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");

            String name = null;

            s.GetCellContents(name);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void getCellContentsTest3()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");

            String invalidName = "A4#@";

            s.GetCellContents(invalidName);
        }

        [TestMethod()]
        public void getCellContentsTest4()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");

            Assert.IsTrue("".Equals(s.GetCellContents("a3948")));
        }

        [TestMethod()]
        public void getNamesOfAllNonEmptyCellsTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-10/4");
            s.SetContentsOfCell("b58", "budget: ");

            int count = 0;
            List<string> cellList = new List<string>();
            cellList.Add("a1");
            cellList.Add("b5");
            cellList.Add("z1");
            cellList.Add("b58");

            IEnumerable<string> list = s.GetNamesOfAllNonemptyCells();
            foreach (string cell in list)
            {
                Assert.IsTrue(cellList.Contains(cell));
                count++;
            }
            Assert.IsTrue(count == 4);  
        }


        [TestMethod()]
        public void getCellValueTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "10.0");
            s.SetContentsOfCell("b5", "x4 + 35");
            s.SetContentsOfCell("z1", "=a1-4");
            s.SetContentsOfCell("b58", "budget: ");

            Assert.IsTrue(s.GetCellValue("a1").Equals(10.0));
            Assert.IsTrue(s.GetCellValue("b5").Equals("x4 + 35"));
            Assert.IsTrue(s.GetCellValue("z1").Equals(6.0));
            Assert.IsTrue(s.GetCellValue("b58").Equals("budget: "));

        }


        [TestMethod()]
        public void lookupDelegateTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "=a1 + 35");

            Assert.IsTrue(s.GetCellValue("b5").Equals(37.0));
        }

        [TestMethod()]
        public void updateCellsTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "=a1 + 35");
            s.SetContentsOfCell("c5", "=a1 + 22");
            s.SetContentsOfCell("d5", "=b5 + c5/4");
            
            Assert.IsTrue(s.GetCellValue("c5").Equals(24.0));
            Assert.IsTrue(s.GetCellValue("d5").Equals(43.0));
        }

        [TestMethod()]
        public void updateCellsTest2()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "2.0");
            s.SetContentsOfCell("b5", "=a1 + 35");
            s.SetContentsOfCell("c5", "=a1 + 22");
            s.SetContentsOfCell("d5", "=b5 + c5/4");

            Assert.IsTrue(s.GetCellValue("c5").Equals(24.0));
            Assert.IsTrue(s.GetCellValue("d5").Equals(43.0));

            s.SetContentsOfCell("a1", "= 10 / 2.0");

            Assert.IsTrue(s.GetCellValue("a1").Equals(5.0));
            Assert.IsTrue(s.GetCellValue("b5").Equals(40.0));
            Assert.IsTrue(s.GetCellValue("c5").Equals(27.0));
            Assert.IsTrue(s.GetCellValue("d5").Equals(46.75));

        }

        [TestMethod()]
        public void updateCellsTest3()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "5.0");
            s.SetContentsOfCell("b5", "=a1 + 35");
            s.SetContentsOfCell("c5", "=a1 + 22");
            s.SetContentsOfCell("d5", "=b5 +1");

            Assert.IsTrue(s.GetCellValue("a1").Equals(5.0));
            Assert.IsTrue(s.GetCellValue("b5").Equals(40.0));
            Assert.IsTrue(s.GetCellValue("c5").Equals(27.0));
            Assert.IsTrue(s.GetCellValue("d5").Equals(41.0));

            s.SetContentsOfCell("b5", "Money:");
            object obj = s.GetCellValue("d5");
            Assert.IsTrue(obj is FormulaError);
        }

        [TestMethod()]
        public void updateCellsTest4()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "5.0");
            s.SetContentsOfCell("b5", "=a1 + 35");
            s.SetContentsOfCell("c5", "=a1 + 22");
            s.SetContentsOfCell("d5", "=b5 +1");

            Assert.IsTrue(s.GetCellValue("a1").Equals(5.0));
            Assert.IsTrue(s.GetCellValue("b5").Equals(40.0));
            Assert.IsTrue(s.GetCellValue("c5").Equals(27.0));
            Assert.IsTrue(s.GetCellValue("d5").Equals(41.0));

            s.SetContentsOfCell("a1", "Money:");
            Assert.IsTrue(s.GetCellValue("a1") is String);
            Assert.IsTrue(s.GetCellValue("b5") is FormulaError);
            Assert.IsTrue(s.GetCellValue("c5") is FormulaError);
            Assert.IsTrue(s.GetCellValue("d5") is FormulaError);
        }

        //[TestMethod()]
        //public void emptyCellLookupTest()
        //{
        //    Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
        //    s.SetContentsOfCell("a1", "5.0");
        //    s.SetContentsOfCell("b5", "=n63 + 35"); //what should be done if the cell in a formula doesn't contain anything?
           
        //    Assert.IsTrue(s.GetCellValue("b5").Equals(35.0));
        //}

        [TestMethod()]
        public void changedTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            s.SetContentsOfCell("a1", "5.0");
            Assert.IsTrue(s.Changed == true);
            s.Save("filePath");
            Assert.IsTrue(s.Changed == false);
            s.SetContentsOfCell("c1", "=a1 -4");
            Assert.IsTrue(s.Changed == true);

        }

        [TestMethod()]
        public void versionGetTest()
        {
            Spreadsheet s = new Spreadsheet(s => true, s => s, "1.0");
            Assert.IsTrue(s.Version == "1.0");
        }

        [TestMethod()]
        public void getEmptyCellContentsTest()
        {
            Spreadsheet s = new Spreadsheet(null, null, null);
            Assert.IsTrue(s.GetCellContents("a1").Equals(""));
        }

        [TestMethod()]
        public void getEmptyCellValueTest()
        {
            Spreadsheet s = new Spreadsheet(null, null, null);
            Assert.IsTrue(s.GetCellValue("a1").Equals(""));
        }

        [TestMethod()]
        public void fourParamNullTest()
        {
            Spreadsheet s = new Spreadsheet(null, null, null);
            s.SetContentsOfCell("a1", "2.0");
            s.Save("fourParamNullTest");
            Spreadsheet s2 = new Spreadsheet("fourParamNullTest", null, null, null);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void nullFilePathTest()
        {
            Spreadsheet s = new Spreadsheet(null, null, null, null);
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void missingNameElement()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("missingNameElement", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("contents", "=a3 + 24 + !43");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("missingNameElement", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void missingContentsElement()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("missingContentsElement", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("Name", "a3");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("missingContentsElement", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingInvalidCellNamesTest()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingInvalidCellNamesTest", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a");
                writer.WriteElementString("contents", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingInvalidCellNamesTest", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingInvalidCellnameTest2()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingInvalidCellnameTest2", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a3a");
                writer.WriteElementString("contents", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingInvalidCellnameTest2", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingInvalidCellnameTest3()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingInvalidCellnameTest3", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a3");
                writer.WriteElementString("contents", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingInvalidCellnameTest3", s => s.Equals("a2"), x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingInvalidCellnameTest4()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingInvalidCellnameTest4", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a");
                writer.WriteElementString("contents", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingInvalidCellnameTest4", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingInvalidCellnameTest5()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingInvalidCellnameTest5", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "1a");
                writer.WriteElementString("contents", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingInvalidCellnameTest5", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingMissingContentsTest()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingMissingContentsTest", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "1.0");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a");
                writer.WriteElementString("somethingElse", "24");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }


            Spreadsheet a = new Spreadsheet("readingMissingContentsTest", s => true, x => x, "1.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void readingNoVersionTag()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create("readingNoVersionTag", settings))
            {
                writer.WriteStartDocument();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "a");
                writer.WriteElementString("somethingElse", "24");
                writer.WriteEndElement();

                writer.WriteEndDocument();
            }

            Spreadsheet a = new Spreadsheet("readingNoVersionTag", s => true, x => x, "1.0");
        }

        [TestMethod()]
        public void formulaVariableValidityTest()
        {
            Spreadsheet a = new Spreadsheet(s => true, x => x, "1.0");
            a.SetContentsOfCell("a1", "= 2.4 + _32");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void formulaVariableValidityTest2()
        {
            Spreadsheet a = new Spreadsheet(s => true, x => x, "1.0");
            a.SetContentsOfCell("a1", "= 2.4 + _32@45");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void invalidCellNameTest()
        {
            Spreadsheet a = new Spreadsheet(s => true, x => x, "1.0");
            a.SetContentsOfCell("aa35.", "4.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void invalidCellNameTest2()
        {
            Spreadsheet a = new Spreadsheet(s => true, x => x, "1.0");
            a.SetContentsOfCell("a3a", "4.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void isValidDelegateTest()
        {
            Spreadsheet a = new Spreadsheet(s => s.Equals("a1") , x => x, "1.0");
            a.SetContentsOfCell("a2", "4.0");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void StressTest()
        {
            Spreadsheet s = new Spreadsheet(s => s.Equals("a1"), x => x, "1.0");
            for(int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("a" + i.ToString(), "=a" + (i + 1).ToString());
            }
            s.SetContentsOfCell("a500", "2.0");
            Assert.IsTrue(s.GetCellValue("a1").Equals(2.0));
        }


    }

    /// <summary>
    ///This is a test class for SpreadsheetTest and is intended
    ///to contain all SpreadsheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KoptasPS4TestsModifiedForPS5
    {

        // EMPTY SPREADSHEETS
        [TestMethod(), Timeout(2000)]
        [TestCategory("1")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetNull()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestEmptyGetContents()
        {
            Spreadsheet s = new Spreadsheet();
            s.GetCellContents("1AA");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("3")]
        public void TestGetEmptyContents()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod(), Timeout(2000)]
        [TestCategory("4")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "1.5");
        }


        [TestMethod(), Timeout(2000)]
        [TestCategory("5")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetInvalidNameDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1A1A", "1.5");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("6")]
        public void TestSimpleSetDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod(), Timeout(2000)]
        [TestCategory("7")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetNullStringVal()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", (string)null);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("8")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullStringName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "hello");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("9")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "hello");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("10")]
        public void TestSetGetSimpleString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod(), Timeout(2000)]
        [TestCategory("11")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSetNullFormVal()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", null);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("12")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetNullFormName()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "=2");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("13")]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetSimpleForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("1AZ", "=2");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("14")]
        public void TestSetGetForm()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "=3");
            Formula f = (Formula)s.GetCellContents("Z7");
            Assert.AreEqual(new Formula("3"), f);
            Assert.AreNotEqual(new Formula("2"), f);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod(), Timeout(2000)]
        [TestCategory("15")]
        [ExpectedException(typeof(CircularException))]
        public void TestSimpleCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("16")]
        [ExpectedException(typeof(CircularException))]
        public void TestComplexCircular()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A3", "=A4+A5");
            s.SetContentsOfCell("A5", "=A6+A7");
            s.SetContentsOfCell("A7", "=A1+A1");
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2+A3");
                s.SetContentsOfCell("A2", "15");
                s.SetContentsOfCell("A3", "30");
                s.SetContentsOfCell("A2", "=A3*A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("17b")]
        [ExpectedException(typeof(CircularException))]
        public void TestUndoCellsCircular()
        {
            Spreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1", "=A2");
                s.SetContentsOfCell("A2", "=A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual("", s.GetCellContents("A2"));
                Assert.IsTrue(new HashSet<string> { "A1" }.SetEquals(s.GetNamesOfAllNonemptyCells()));
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("18")]
        public void TestEmptyNames()
        {
            Spreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("19")]
        public void TestExplicitEmptySet()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("20")]
        public void TestSimpleNamesString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }


        [TestMethod(), Timeout(2000)]
        [TestCategory("21")]
        public void TestSimpleNamesDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("22")]
        public void TestSimpleNamesFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("23")]
        public void TestMixedNames()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", "=3.5");
            Assert.IsTrue(new HashSet<string>(s.GetNamesOfAllNonemptyCells()).SetEquals(new HashSet<string>() { "A1", "B1", "C1" }));
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("24")]
        public void TestSetSingletonDouble()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("A1", "17.2").SequenceEqual(new List<string>() { "A1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("25")]
        public void TestSetSingletonString()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "=5");
            Assert.IsTrue(s.SetContentsOfCell("B1", "hello").SequenceEqual(new List<string>() { "B1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("26")]
        public void TestSetSingletonFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");

            IList<string> list = new List<string>() { "C1" };


            Assert.IsTrue(s.SetContentsOfCell("C1", "=5").SequenceEqual(new List<string>() { "C1" }));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("27")]
        public void TestSetChain()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");

            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
        }

        // CHANGING CELLS
        [TestMethod(), Timeout(2000)]
        [TestCategory("28")]
        public void TestChangeFtoD()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("29")]
        public void TestChangeFtoS()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("30")]
        public void TestChangeStoF()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", "=23");
            Assert.AreEqual(new Formula("23"), (Formula)s.GetCellContents("A1"));
            Assert.AreNotEqual(new Formula("24"), (Formula)s.GetCellContents("A1"));
        }

        // STRESS TESTS
        [TestMethod(), Timeout(2000)]
        [TestCategory("31")]
        public void TestStress1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            IList<String> cells = s.SetContentsOfCell("E1", "0");
            Assert.IsTrue(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }.SetEquals(cells));
        }

        // Repeated for extra weight
        [TestMethod(), Timeout(2000)]
        [TestCategory("32")]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("33")]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("34")]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("35")]
        public void TestStress2()
        {
            Spreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {

                cells.Add("A" + i.ToString());
                Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i.ToString(), "=A" + (i + 1).ToString())));
            }
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("36")]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("37")]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("38")]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("39")]
        public void TestStress3()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i.ToString(), "=A" + (i + 1).ToString());
            }
            try
            {
                s.SetContentsOfCell("A150", "=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("40")]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("41")]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("42")]
        public void TestStress3c()
        {
            TestStress3();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("43")]
        public void TestStress4()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i.ToString(), "=A" + (i + 1).ToString());
            }
            LinkedList<string> firstCells = new LinkedList<string>();
            LinkedList<string> lastCells = new LinkedList<string>();
            for (int i = 0; i < 250; i++)
            {
                firstCells.AddFirst("A1" + i.ToString());
                lastCells.AddFirst("A1" + (i + 250).ToString());
            }
            Assert.IsTrue(s.SetContentsOfCell("A1249", "25.0").SequenceEqual(firstCells));
            Assert.IsTrue(s.SetContentsOfCell("A1499", "0").SequenceEqual(lastCells));
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("44")]
        public void TestStress4a()
        {
            TestStress4();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("45")]
        public void TestStress4b()
        {
            TestStress4();
        }
        [TestMethod(), Timeout(2000)]
        [TestCategory("46")]
        public void TestStress4c()
        {
            TestStress4();
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("47")]
        public void TestStress5()
        {
            RunRandomizedTest(47, 2519);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("48")]
        public void TestStress6()
        {
            RunRandomizedTest(48, 2521);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("49")]
        public void TestStress7()
        {
            RunRandomizedTest(49, 2526);
        }

        [TestMethod(), Timeout(2000)]
        [TestCategory("50")]
        public void TestStress8()
        {
            RunRandomizedTest(50, 2521);
        }

        /// <summary>
        /// Sets random contents for a random cell 10000 times
        /// </summary>
        /// <param name="seed">Random seed</param>
        /// <param name="size">The known resulting spreadsheet size, given the seed</param>
        public void RunRandomizedTest(int seed, int size)
        {
            Spreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.14");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        /// <summary>
        /// Generates a random cell name with a capital letter and number between 1 - 99
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        /// <summary>
        /// Generates a random Formula
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }
    }
   
    [TestClass]
    public class MyPS4TestsModifiedForPS5
        {
            [TestMethod]
            public void Test1()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
            }

            [TestMethod]
            public void simpleSetTest()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "2.0");
                sheet.SetContentsOfCell("b1", "Net Income:");
                sheet.SetContentsOfCell("c1", "=b7 +32");
            }


            //-------------------GetCellContents Tests---------------------
            [TestMethod]
            public void getCellContentsNumber()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "2.0");
                object contents = sheet.GetCellContents("a1");
                Assert.IsTrue(contents.Equals(2.0));
            }

            [TestMethod]
            public void getCellContentsString()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "Profit:");
                object contents = sheet.GetCellContents("a1");
                Assert.IsTrue(contents.Equals("Profit:"));
            }

            [TestMethod]
            public void getCellContentsFormula()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "=25+n20-3.0");
                object contents = sheet.GetCellContents("a1");
                Assert.IsTrue(contents.Equals(new Formula("25+n20-3.0")));
            }

            [TestMethod]
            public void getCellContentsEmpty()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                String empty = "";
                Assert.IsTrue(empty.Equals(sheet.GetCellContents("a1")));
            }

            //-------------------SetContentsOfCell Tests---------------------
            [TestMethod]
            public void SetContentsOfCellNumber()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                List<string> list = (List<string>)sheet.SetContentsOfCell("a1", "2.0");
                foreach(string s in list)
                    Assert.IsTrue(s == "a1");
                Assert.IsTrue(list.Count == 1);
            }

            [TestMethod]
            public void SetContentsOfCellString()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                List<string> list = (List<string>)sheet.SetContentsOfCell("a1", "budget: ");
                foreach (string s in list)
                    Assert.IsTrue(s == "a1");
                Assert.IsTrue(list.Count == 1);
            }
            [TestMethod]
            public void SetContentsOfCellFormula()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "=d1");
                List<string> list = (List<string>)sheet.SetContentsOfCell("d1", "=32");
                foreach (string s in list)
                    Assert.IsTrue(s == "a1" || s == "d1");
                Assert.IsTrue(list.Count == 2);
            }
            [TestMethod]
            public void SetContentsOfCellSameCell()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "24");
                sheet.SetContentsOfCell("a1", "13");
                sheet.SetContentsOfCell("b1", "f1");
                sheet.SetContentsOfCell("b1", "a1");
                sheet.SetContentsOfCell("c1", "budget:");
                sheet.SetContentsOfCell("c1", "income:");
            }

            //------------------GetDirectDependents Tests------------------
            [TestMethod]
            public void GetDirectDependents()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "24");
                sheet.SetContentsOfCell("b1", "=a1");
                sheet.SetContentsOfCell("c1", "=a1");
                sheet.SetContentsOfCell("z1", "=b1");
                sheet.SetContentsOfCell("e1", "=c1");
                sheet.SetContentsOfCell("f1", "=z1");
                IEnumerable<string> list = sheet.SetContentsOfCell("a1", "21");

                foreach(string s in list)
                {
                    Console.WriteLine(s);
                }
                int i = 0;
            }


            //--------------GetNamesOfAllNonemptyCells Tests-----------------
            [TestMethod]
            public void GetNamesOfAllNonemptyCells()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "24");
                sheet.SetContentsOfCell("b1", "a1");
                sheet.SetContentsOfCell("c1", "a1");

                IEnumerable<string> list = sheet.GetNamesOfAllNonemptyCells();

                int count = 0;
                foreach (string s in list)
                {
                    Assert.IsTrue(s == "a1" || s == "b1" || s == "c1");
                    count++;
                }

                Assert.IsTrue(count == 3);
            }






            //-------------------NullorInvalid Tests----------------------
            [TestMethod]
            [ExpectedException(typeof(InvalidNameException))]
            public void noiTest()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("$1", "24");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidNameException))]
            public void noiTest2()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                string isNull = null;
                sheet.SetContentsOfCell(isNull, "24");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidNameException))]
            public void noiTest3()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("_4b1", "24");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidNameException))]
            public void noiTest4()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("4cc.", "24");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void noiTest5()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                String a = null;
                sheet.SetContentsOfCell("A1", a);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidNameException))]
            public void noiTest6()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                String a = null;
                sheet.SetContentsOfCell(null, a);
            }



            //--------------------Circular Dependency Tests----------------
            [TestMethod]
            [ExpectedException(typeof(CircularException))]
            public void circularDependencyTest()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("c1", "=a1");
                sheet.SetContentsOfCell("a1", "=c1");
            }


            [TestMethod]
            [ExpectedException(typeof(CircularException))]
            public void circularDependencyTest2()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("c1", "=c1 + 4");
            }

            [TestMethod]
            [ExpectedException(typeof(CircularException))]
            public void circularDependencyTest3()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "24");
                sheet.SetContentsOfCell("b1", "=a1 + 4");
                sheet.SetContentsOfCell("c1", "=b1");
                sheet.SetContentsOfCell("d1", "=b1");
                sheet.SetContentsOfCell("e1", "=d1");
                sheet.SetContentsOfCell("f1", "=d1 + c1");
                sheet.SetContentsOfCell("a1", "=z1");
                sheet.SetContentsOfCell("z1", "=d1 +f1+ c1");

            }

            [TestMethod]
            public void shouldntThrowACircularExcpetion()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "24");
                sheet.SetContentsOfCell("b1", "=a1 + 4");
                sheet.SetContentsOfCell("c1", "=b1");
                sheet.SetContentsOfCell("d1", "=b1");
                sheet.SetContentsOfCell("e1", "=d1");
                sheet.SetContentsOfCell("f1", "=d1 + c1 + z1");
                sheet.SetContentsOfCell("a1", "=z1 + 3");
            }



            //----------------------Get All Dependents Tests-----------------------
            [TestMethod]
            public void getAllDependentsTest()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "=b1 +32");
                sheet.SetContentsOfCell("b1", "=24*(c1-13.5)");
                sheet.SetContentsOfCell("d1", "=c1+4");
                sheet.SetContentsOfCell("e1", "=d1");
                sheet.SetContentsOfCell("f1", "=b1");
                sheet.SetContentsOfCell("n54", "=j24+3");


                List<string> list = (List<string>)sheet.SetContentsOfCell("c1", "=j24");
                string[] values = { "b1", "d1", "c1", "a1", "f1", "e1" };
                foreach (string s in values)
                    Assert.IsTrue(list.Contains(s));
            }

            [TestMethod]
            public void getAllDependentsTest2()
            {
                AbstractSpreadsheet sheet = new Spreadsheet();
                sheet.SetContentsOfCell("a1", "=b1 +32");
                sheet.SetContentsOfCell("b1", "=24*(c1-13.5)");
                sheet.SetContentsOfCell("d1", "=c1+4");
                sheet.SetContentsOfCell("e1", "=d1");
                sheet.SetContentsOfCell("f1", "=b1");
                sheet.SetContentsOfCell("n54", "=j24+3");
                sheet.SetContentsOfCell("b1", "=n54");

                List<string> list = (List<string>)sheet.SetContentsOfCell("c1", "=j24");
                string[] values = { "d1", "c1", "e1" };
                foreach (string s in values)
                    Assert.IsTrue(list.Contains(s));
            }
        }

    /// <summary>
    ///This is a test class for SpreadsheetTest and is intended
    ///to contain all SpreadsheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KoptasPS5Tests
    {

        // Verifies cells and their values, which must alternate.
        public void VV(AbstractSpreadsheet sheet, params object[] constraints)
        {
            for (int i = 0; i < constraints.Length; i += 2)
            {
                if (constraints[i + 1] is double)
                {
                    Assert.AreEqual((double)constraints[i + 1], (double)sheet.GetCellValue((string)constraints[i]), 1e-9);
                }
                else
                {
                    Assert.AreEqual(constraints[i + 1], sheet.GetCellValue((string)constraints[i]));
                }
            }
        }


        // For setting a spreadsheet cell.
        public IEnumerable<string> Set(AbstractSpreadsheet sheet, string name, string contents)
        {
            List<string> result = new List<string>(sheet.SetContentsOfCell(name, contents));
            return result;
        }

        // Tests IsValid
        [TestMethod, Timeout(2000)]
        [TestCategory("1")]
        public void IsValidTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "x");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void IsValidTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("A1", "x");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("3")]
        public void IsValidTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "= A1 + C1");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("4")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IsValidTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("B1", "= A1 + C1");
        }

        // Tests Normalize
        [TestMethod, Timeout(2000)]
        [TestCategory("5")]
        public void NormalizeTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("", s.GetCellContents("b1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("6")]
        public void NormalizeTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("hello", ss.GetCellContents("b1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("7")]
        public void NormalizeTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a1", "5");
            s.SetContentsOfCell("A1", "6");
            s.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(5.0, (double)s.GetCellValue("B1"), 1e-9);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("8")]
        public void NormalizeTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("a1", "5");
            ss.SetContentsOfCell("A1", "6");
            ss.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(6.0, (double)ss.GetCellValue("B1"), 1e-9);
        }

        // Simple tests
        [TestMethod, Timeout(2000)]
        [TestCategory("9")]
        public void EmptySheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            VV(ss, "A1", "");
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("10")]
        public void OneString()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneString(ss);
        }

        public void OneString(AbstractSpreadsheet ss)
        {
            Set(ss, "B1", "hello");
            VV(ss, "B1", "hello");
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("11")]
        public void OneNumber()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneNumber(ss);
        }

        public void OneNumber(AbstractSpreadsheet ss)
        {
            Set(ss, "C1", "17.5");
            VV(ss, "C1", 17.5);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("12")]
        public void OneFormula()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneFormula(ss);
        }

        public void OneFormula(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "5.2");
            Set(ss, "C1", "= A1+B1");
            VV(ss, "A1", 4.1, "B1", 5.2, "C1", 9.3);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("13")]
        public void ChangedAfterModify()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.IsFalse(ss.Changed);
            Set(ss, "C1", "17.5");
            Assert.IsTrue(ss.Changed);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("13b")]
        public void UnChangedAfterSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "C1", "17.5");
            ss.Save("changed.txt");
            Assert.IsFalse(ss.Changed);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("14")]
        public void DivisionByZero1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero1(ss);
        }

        public void DivisionByZero1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "0.0");
            Set(ss, "C1", "= A1 / B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("15")]
        public void DivisionByZero2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero2(ss);
        }

        public void DivisionByZero2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "5.0");
            Set(ss, "A3", "= A1 / 0.0");
            Assert.IsInstanceOfType(ss.GetCellValue("A3"), typeof(FormulaError));
        }



        [TestMethod, Timeout(2000)]
        [TestCategory("16")]
        public void EmptyArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            EmptyArgument(ss);
        }

        public void EmptyArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("17")]
        public void StringArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            StringArgument(ss);
        }

        public void StringArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "hello");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("18")]
        public void ErrorArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ErrorArgument(ss);
        }

        public void ErrorArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= C1");
            Assert.IsInstanceOfType(ss.GetCellValue("D1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("19")]
        public void NumberFormula1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula1(ss);
        }

        public void NumberFormula1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + 4.2");
            VV(ss, "C1", 8.3);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("20")]
        public void NumberFormula2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula2(ss);
        }

        public void NumberFormula2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "= 4.6");
            VV(ss, "A1", 4.6);
        }


        // Repeats the simple tests all together
        [TestMethod, Timeout(2000)]
        [TestCategory("21")]
        public void RepeatSimpleTests()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "17.32");
            Set(ss, "B1", "This is a test");
            Set(ss, "C1", "= A1+B1");
            OneString(ss);
            OneNumber(ss);
            OneFormula(ss);
            DivisionByZero1(ss);
            DivisionByZero2(ss);
            StringArgument(ss);
            ErrorArgument(ss);
            NumberFormula1(ss);
            NumberFormula2(ss);
        }

        // Four kinds of formulas
        [TestMethod, Timeout(2000)]
        [TestCategory("22")]
        public void Formulas()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formulas(ss);
        }

        public void Formulas(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.4");
            Set(ss, "B1", "2.2");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= A1 - B1");
            Set(ss, "E1", "= A1 * B1");
            Set(ss, "F1", "= A1 / B1");
            VV(ss, "C1", 6.6, "D1", 2.2, "E1", 4.4 * 2.2, "F1", 2.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("23")]
        public void Formulasa()
        {
            Formulas();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("24")]
        public void Formulasb()
        {
            Formulas();
        }


        // Are multiple spreadsheets supported?
        [TestMethod, Timeout(2000)]
        [TestCategory("25")]
        public void Multiple()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet();
            Set(s1, "X1", "hello");
            Set(s2, "X1", "goodbye");
            VV(s1, "X1", "hello");
            VV(s2, "X1", "goodbye");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("26")]
        public void Multiplea()
        {
            Multiple();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("27")]
        public void Multipleb()
        {
            Multiple();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("28")]
        public void Multiplec()
        {
            Multiple();
        }

        // Reading/writing spreadsheets
        [TestMethod, Timeout(2000)]
        [TestCategory("29")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save(Path.GetFullPath("/missing/save.txt"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("30")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(Path.GetFullPath("/missing/save.txt"), s => true, s => s, "");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("31")]
        public void SaveTest3()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            Set(s1, "A1", "hello");
            s1.Save("save1.txt");
            s1 = new Spreadsheet("save1.txt", s => true, s => s, "default");
            Assert.AreEqual("hello", s1.GetCellContents("A1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("32")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest4()
        {
            using (StreamWriter writer = new StreamWriter("save2.txt"))
            {
                writer.WriteLine("This");
                writer.WriteLine("is");
                writer.WriteLine("a");
                writer.WriteLine("test!");
            }
            AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("33")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save3.txt");
            ss = new Spreadsheet("save3.txt", s => true, s => s, "version");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("34")]
        public void SaveTest6()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "hello");
            ss.Save("save4.txt");
            Assert.AreEqual("hello", new Spreadsheet().GetSavedVersion("save4.txt"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("35")]
        public void SaveTest7()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            using (XmlWriter writer = XmlWriter.Create("save5.txt", settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A2");
                writer.WriteElementString("contents", "5.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A3");
                writer.WriteElementString("contents", "4.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A4");
                writer.WriteElementString("contents", "= A2 + A3");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AbstractSpreadsheet ss = new Spreadsheet("save5.txt", s => true, s => s, "");
            VV(ss, "A1", "hello", "A2", 5.0, "A3", 4.0, "A4", 9.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("36")]
        public void SaveTest8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "hello");
            Set(ss, "A2", "5.0");
            Set(ss, "A3", "4.0");
            Set(ss, "A4", "= A2 + A3");
            ss.Save("save6.txt");
            using (XmlReader reader = XmlReader.Create("save6.txt"))
            {
                int spreadsheetCount = 0;
                int cellCount = 0;
                bool A1 = false;
                bool A2 = false;
                bool A3 = false;
                bool A4 = false;
                string name = null;
                string contents = null;

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "spreadsheet":
                                Assert.AreEqual("default", reader["version"]);
                                spreadsheetCount++;
                                break;

                            case "cell":
                                cellCount++;
                                break;

                            case "name":
                                reader.Read();
                                name = reader.Value;
                                break;

                            case "contents":
                                reader.Read();
                                contents = reader.Value;
                                break;
                        }
                    }
                    else
                    {
                        switch (reader.Name)
                        {
                            case "cell":
                                if (name.Equals("A1")) { Assert.AreEqual("hello", contents); A1 = true; }
                                else if (name.Equals("A2")) { Assert.AreEqual(5.0, Double.Parse(contents), 1e-9); A2 = true; }
                                else if (name.Equals("A3")) { Assert.AreEqual(4.0, Double.Parse(contents), 1e-9); A3 = true; }
                                else if (name.Equals("A4")) { contents = contents.Replace(" ", ""); Assert.AreEqual("=A2+A3", contents); A4 = true; }
                                else Assert.Fail();
                                break;
                        }
                    }
                }
                Assert.AreEqual(1, spreadsheetCount);
                Assert.AreEqual(4, cellCount);
                Assert.IsTrue(A1);
                Assert.IsTrue(A2);
                Assert.IsTrue(A3);
                Assert.IsTrue(A4);
            }
        }


        // Fun with formulas
        [TestMethod, Timeout(2000)]
        [TestCategory("37")]
        public void Formula1()
        {
            Formula1(new Spreadsheet());
        }
        public void Formula1(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= b1 + b2");
            Assert.IsInstanceOfType(ss.GetCellValue("a1"), typeof(FormulaError));
            Assert.IsInstanceOfType(ss.GetCellValue("a2"), typeof(FormulaError));
            Set(ss, "a3", "5.0");
            Set(ss, "b1", "2.0");
            Set(ss, "b2", "3.0");
            VV(ss, "a1", 10.0, "a2", 5.0);
            Set(ss, "b2", "4.0");
            VV(ss, "a1", 11.0, "a2", 6.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("38")]
        public void Formula2()
        {
            Formula2(new Spreadsheet());
        }
        public void Formula2(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= a3");
            Set(ss, "a3", "6.0");
            VV(ss, "a1", 12.0, "a2", 6.0, "a3", 6.0);
            Set(ss, "a3", "5.0");
            VV(ss, "a1", 10.0, "a2", 5.0, "a3", 5.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("39")]
        public void Formula3()
        {
            Formula3(new Spreadsheet());
        }
        public void Formula3(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a3 + a5");
            Set(ss, "a2", "= a5 + a4");
            Set(ss, "a3", "= a5");
            Set(ss, "a4", "= a5");
            Set(ss, "a5", "9.0");
            VV(ss, "a1", 18.0);
            VV(ss, "a2", 18.0);
            Set(ss, "a5", "8.0");
            VV(ss, "a1", 16.0);
            VV(ss, "a2", 16.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("40")]
        public void Formula4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formula1(ss);
            Formula2(ss);
            Formula3(ss);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("41")]
        public void Formula4a()
        {
            Formula4();
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("42")]
        public void MediumSheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
        }

        public void MediumSheet(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "1.0");
            Set(ss, "A2", "2.0");
            Set(ss, "A3", "3.0");
            Set(ss, "A4", "4.0");
            Set(ss, "B1", "= A1 + A2");
            Set(ss, "B2", "= A3 * A4");
            Set(ss, "C1", "= B1 + B2");
            VV(ss, "A1", 1.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 3.0, "B2", 12.0, "C1", 15.0);
            Set(ss, "A1", "2.0");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 4.0, "B2", 12.0, "C1", 16.0);
            Set(ss, "B1", "= A1 / A2");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("43")]
        public void MediumSheeta()
        {
            MediumSheet();
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("44")]
        public void MediumSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
            ss.Save("save7.txt");
            ss = new Spreadsheet("save7.txt", s => true, s => s, "default");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("45")]
        public void MediumSavea()
        {
            MediumSave();
        }


        // A long chained formula. Solutions that re-evaluate 
        // cells on every request, rather than after a cell changes,
        // will timeout on this test.
        // This test is repeated to increase its scoring weight
        [TestMethod, Timeout(6000)]
        [TestCategory("46")]
        public void LongFormulaTest()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("47")]
        public void LongFormulaTest2()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("48")]
        public void LongFormulaTest3()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("49")]
        public void LongFormulaTest4()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("50")]
        public void LongFormulaTest5()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        public void LongFormulaHelper(out object result)
        {
            try
            {
                AbstractSpreadsheet s = new Spreadsheet();
                s.SetContentsOfCell("sum1", "= a1 + a2");
                int i;
                int depth = 100;
                for (i = 1; i <= depth * 2; i += 2)
                {
                    s.SetContentsOfCell("a" + i, "= a" + (i + 2) + " + a" + (i + 3));
                    s.SetContentsOfCell("a" + (i + 1), "= a" + (i + 2) + "+ a" + (i + 3));
                }
                s.SetContentsOfCell("a" + i, "1");
                s.SetContentsOfCell("a" + (i + 1), "1");
                Assert.AreEqual(Math.Pow(2, depth + 1), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + i, "0");
                Assert.AreEqual(Math.Pow(2, depth), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + (i + 1), "0");
                Assert.AreEqual(0.0, (double)s.GetCellValue("sum1"), 0.1);
                result = "ok";
            }
            catch (Exception e)
            {
                result = e;
            }
        }

    }
}
