//Author: John Stevens 9/17/2021 2:59 pm

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System;


namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        //------------Constructor Tests------------
        [TestMethod]
        public void constructorTestIllegalChar()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("F23 + $12"));
        }

        [TestMethod]
        public void constructorTestIllegalChar2()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("F23 + a32@1"));
        }

        [TestMethod]
        public void constructorTest()
        {
            Formula f1 = new Formula("F23 + a321");
            //Shouldn't throw any exceptions
        }

        [TestMethod]
        public void constructorBalancedParenthesesTest()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("F23 + ((a321)"));
        }

        [TestMethod]
        public void constructorBalancedParenthesesTest2()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("F23 + (a321+1)+(4.3-2.1"));
        }

        [TestMethod]
        public void constructorTestIncorrectParenthesesTest()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("(a321+1)(4.3-2.1) + f23"));
        }

        [TestMethod]
        public void constructorRightParenthesesRuleTest()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("F23 + (a321+1)+4.3-2.1)"));
        }

        [TestMethod]
        public void constructorTestStartingTokenRule()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("+ F23 + (a321+1)+(4.3-2.1)"));
        }

        [TestMethod]
        public void constructorOneTokenRule()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("   "));
        }

        [TestMethod]
        public void constructorEndingTokenRule()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("1+7+"));
        }

        [TestMethod]
        public void constructorEndingTokenRule2()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("1+7+(4.0/2("));
        }

        [TestMethod]
        public void constructorEndingTokenRule3()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("1+7+(4.0/_4b78!89"));
        }

        [TestMethod]
        public void constructorParenthesisOrOperatorFollowingRule()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("()"));
        }

        [TestMethod]
        public void constructorParenthesisOrOperatorFollowingRule2()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("5.01+(@-4)"));
        }

        [TestMethod]
        public void constructorExtraFollowingRule()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("(4-1)q24"));
        }

        [TestMethod]
        public void constructorExtraFollowingRule2()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("k24 + 4(5-1)"));
        }

        [TestMethod]
        public void constructorExtraFollowingRule3()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("k24 + h5(5-1)"));
        }

        [TestMethod]
        public void constructorExtraFollowingRule4()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("54-(24+2)7"));
        }

        [TestMethod]
        public void constructorExtraFollowingRule5()
        {
            Formula f1;
            Assert.ThrowsException<FormulaFormatException>(() => f1 = new Formula("54-(24+2)(a5*4)"));
        }

        //------------Evaluate Method Tests------------
        [TestMethod]
        public void evaluateSimpleTest()
        {
            Formula f1 = new Formula("23-17");
            Assert.AreEqual(f1.Evaluate(null), 6.0);
        }

        [TestMethod]
        public void evaluateSimpleTest2()
        {
            Formula f1 = new Formula("8.5*4-15");
            Assert.AreEqual(f1.Evaluate(null), 19.0);
        }

        [TestMethod]
        public void evaluateSimpleTest3()
        {
            Formula f1 = new Formula("4*(3.5-.5+1)");
            Assert.AreEqual(f1.Evaluate(null), 16.0);
        }

        [TestMethod]
        public void evaluateScientificNotationTest()
        {
            Formula f1 = new Formula("230-1.1e2");
            Assert.AreEqual(f1.Evaluate(null), 120.0);
        }

        [TestMethod]
        public void evaluateDivideByZeroTest()
        {
            Formula f1 = new Formula("25/0");
            Assert.IsTrue(f1.Evaluate(null) is FormulaError);
        }

        [TestMethod]
        public void evaluateDivideByZeroTest2()
        {
            Formula f1 = new Formula("25/(5-5)");
            Assert.IsTrue(f1.Evaluate(null) is FormulaError);
        }

        [TestMethod]
        public void evaluateLookupTest()
        {
            Formula f1 = new Formula("B7 +120");
            Assert.AreEqual(120.0, f1.Evaluate(x => 0));
        }

        [TestMethod]
        public void evaluateLookupTest2()
        {
            Formula f1 = new Formula("B7 +120");
            Assert.AreEqual(140.0, f1.Evaluate(x => 20));
        }

        [TestMethod]
        public void evaluateComplicatedTest()
        {
            Formula f1 = new Formula("(b7*(4+1))-14+_23*2");
            Assert.AreEqual(126.0, f1.Evaluate(x => 20));
        }

        [TestMethod]
        public void evaluateComplicatedTest2()
        {
            Formula f1 = new Formula("2.5*4*c6-(n54*(c6+3-1.5))");
            Assert.AreEqual(15.0, f1.Evaluate(x => 6));
        }

        //------------Equals Method Tests------------
        [TestMethod]
        public void equalsOneIsDiffObj()
        {
            Formula a = new Formula("a23");
            Assert.IsFalse(a.Equals(2.0));
        }

        [TestMethod]
        public void equalsOneIsDiffCase()
        {
            Formula a = new Formula("A23+1", s => s.ToLower(), s => true);
            Formula b = new Formula("a23+1");
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void equalsOneIsDiffCase2()
        {
            Formula a = new Formula("A23+1");
            Formula b = new Formula("a23+1");
            Assert.IsFalse(a.Equals(b));
        }

        //------------ToString Method Tests------------
        [TestMethod]
        public void toStringBasicTest()
        {
            Formula a = new Formula(" a23 + 1.000");
            string b = "a23+1";
            Assert.AreEqual(a.ToString(), b);
        }

        //------------GetVariables Method Tests------------
        [TestMethod]
        public void getVariablesBasicTest()
        {
            List<string> vars = new List<string>();
            vars.Add("k24");
            vars.Add("s58");
            vars.Add("_2jz");

            Formula f1 = new Formula(" k24 + s58*(_2jz - 10.05)");
            List<string> f1Vars = (List<string>)f1.GetVariables();

            CollectionAssert.AreEqual(vars, f1Vars);
        }


        //------------GetHashcode Method Tests------------
        [TestMethod]
        public void getHashCodeBasicTest()
        {
            Formula f1 = new Formula(" k24 + s58*(_2jz - 10.05)");
            Formula f2 = new Formula("k24+s58*(_2jz-10.05)");
            int f1hashcode = f1.GetHashCode();
            int f2hashcode = f2.GetHashCode();

            Assert.AreEqual(f1hashcode, f2hashcode);
        }

        [TestMethod]
        public void getHashCodeNormalizedBasicTest()
        {
            Formula f1 = new Formula(" k24 + S58*(_2JZ - 10.05)", s => s.ToLower(), s => true);
            Formula f2 = new Formula("K24+s58*(_2jz-10.05)", s => s.ToLower(), s => true);
            int f1hashcode = f1.GetHashCode();
            int f2hashcode = f2.GetHashCode();

            Assert.AreEqual(f1hashcode, f2hashcode);
        }

        //------------==Override Method Tests------------
        [TestMethod]
        public void equalsOverrideTest()
        {
            Formula f1 = new Formula(" k24 + S58*(_2JZ - 10.05)", s => s.ToLower(), s => true);
            Formula f2 = new Formula("K24+s58*(_2jz-10.05)", s => s.ToLower(), s => true);
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod]
        public void equalsOverrideFalseTest()
        {
            Formula f1 = new Formula(" k24 + S58*(_2JZ - 10.05)");
            Formula f2 = new Formula("K24+s58*(_2jz-10.05)", s => s.ToLower(), s => true);
            Assert.IsFalse(f1 == f2);
        }

        [TestMethod]
        public void EqualsBothNullTest()
        {
            Formula f1 = null;
            Formula f2 = null;
            Assert.IsTrue(f1 == f2);
        }

        [TestMethod]
        public void EqualsOneNullTest()
        {
            Formula f1 = new Formula("a+2");
            Formula f2 = null;
            Assert.IsFalse(f1 == f2);
        }


        //------------!=Override Method Tests------------
        [TestMethod]
        public void notEqualsOverrideTest()
        {
            Formula f1 = new Formula(" k24 + S58*(_2JZ - 10.05)");
            Formula f2 = new Formula("K24+s58*(_2jz-10.05)", s => s.ToLower(), s => true);
            Assert.IsTrue(f1 != f2);
        }

        [TestMethod]
        public void notequalsOverrideFalseTest()
        {
            Formula f1 = new Formula(" k24 + S58*(_2JZ - 10.05)", s => s.ToLower(), s => true);
            Formula f2 = new Formula("K24+s58*(_2jz-10.05)", s => s.ToLower(), s => true);
            Assert.IsFalse(f1 != f2);
        }

        [TestMethod]
        public void notEqualsBothNullTest()
        {
            Formula f1 = null;
            Formula f2 = null;
            Assert.IsFalse(f1 != f2);
        }

        [TestMethod]
        public void notEqualsOneNullTest()
        {
            Formula f1 = new Formula("a+2");
            Formula f2 = null;
            Assert.IsTrue(f1 != f2);
        }





        /*
         * ------------------------------------Kopta's Tests-----------------------------------
         */
            // Normalizer tests
            [TestMethod(), Timeout(2000)]
            [TestCategory("1")]
            public void TestNormalizerGetVars()
            {
                Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
                HashSet<string> vars = new HashSet<string>(f.GetVariables());

                Assert.IsTrue(vars.SetEquals(new HashSet<string> { "X1" }));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("2")]
            public void TestNormalizerEquals()
            {
                Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
                Formula f2 = new Formula("2+X1", s => s.ToUpper(), s => true);

                Assert.IsTrue(f.Equals(f2));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("3")]
            public void TestNormalizerToString()
            {
                Formula f = new Formula("2+x1", s => s.ToUpper(), s => true);
                Formula f2 = new Formula(f.ToString());

                Assert.IsTrue(f.Equals(f2));
            }

            // Validator tests
            [TestMethod(), Timeout(2000)]
            [TestCategory("4")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestValidatorFalse()
            {
                Formula f = new Formula("2+x1", s => s, s => false);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("5")]
            public void TestValidatorX1()
            {
                Formula f = new Formula("2+x", s => s, s => (s == "x"));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("6")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestValidatorX2()
            {
                Formula f = new Formula("2+y1", s => s, s => (s == "x"));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("7")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestValidatorX3()
            {
                Formula f = new Formula("2+x1", s => s, s => (s == "x"));
            }


            // Simple tests that return FormulaErrors
            [TestMethod(), Timeout(2000)]
            [TestCategory("8")]
            public void TestUnknownVariable()
            {
                Formula f = new Formula("2+X1");
                Assert.IsInstanceOfType(f.Evaluate(s => { throw new ArgumentException("Unknown variable"); }), typeof(FormulaError));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("9")]
            public void TestDivideByZero()
            {
                Formula f = new Formula("5/0");
                Assert.IsInstanceOfType(f.Evaluate(s => 0), typeof(FormulaError));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("10")]
            public void TestDivideByZeroVars()
            {
                Formula f = new Formula("(5 + X1) / (X1 - 3)");
                Assert.IsInstanceOfType(f.Evaluate(s => 3), typeof(FormulaError));
            }


            // Tests of syntax errors detected by the constructor
            [TestMethod(), Timeout(2000)]
            [TestCategory("11")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestSingleOperator()
            {
                Formula f = new Formula("+");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("12")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestExtraOperator()
            {
                Formula f = new Formula("2+5+");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("13")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestExtraCloseParen()
            {
                Formula f = new Formula("2+5*7)");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("14")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestExtraOpenParen()
            {
                Formula f = new Formula("((3+5*7)");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("15")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestNoOperator()
            {
                Formula f = new Formula("5x");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("16")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestNoOperator2()
            {
                Formula f = new Formula("5+5x");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("17")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestNoOperator3()
            {
                Formula f = new Formula("5+7+(5)8");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("18")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestNoOperator4()
            {
                Formula f = new Formula("5 5");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("19")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestDoubleOperator()
            {
                Formula f = new Formula("5 + + 3");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("20")]
            [ExpectedException(typeof(FormulaFormatException))]
            public void TestEmpty()
            {
                Formula f = new Formula("");
            }

            // Some more complicated formula evaluations
            [TestMethod(), Timeout(2000)]
            [TestCategory("21")]
            public void TestComplex1()
            {
                Formula f = new Formula("y1*3-8/2+4*(8-9*2)/14*x7");
                Assert.AreEqual(5.14285714285714, (double)f.Evaluate(s => (s == "x7") ? 1 : 4), 1e-9);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("22")]
            public void TestRightParens()
            {
                Formula f = new Formula("x1+(x2+(x3+(x4+(x5+x6))))");
                Assert.AreEqual(6, (double)f.Evaluate(s => 1), 1e-9);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("23")]
            public void TestLeftParens()
            {
                Formula f = new Formula("((((x1+x2)+x3)+x4)+x5)+x6");
                Assert.AreEqual(12, (double)f.Evaluate(s => 2), 1e-9);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("53")]
            public void TestRepeatedVar()
            {
                Formula f = new Formula("a4-a4*a4/a4");
                Assert.AreEqual(0, (double)f.Evaluate(s => 3), 1e-9);
            }

            // Test of the Equals method
            [TestMethod(), Timeout(2000)]
            [TestCategory("24")]
            public void TestEqualsBasic()
            {
                Formula f1 = new Formula("X1+X2");
                Formula f2 = new Formula("X1+X2");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("25")]
            public void TestEqualsWhitespace()
            {
                Formula f1 = new Formula("X1+X2");
                Formula f2 = new Formula(" X1  +  X2   ");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("26")]
            public void TestEqualsDouble()
            {
                Formula f1 = new Formula("2+X1*3.00");
                Formula f2 = new Formula("2.00+X1*3.0");
                Assert.IsTrue(f1.Equals(f2));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("27")]
            public void TestEqualsComplex()
            {
                Formula f1 = new Formula("1e-2 + X5 + 17.00 * 19 ");
                Formula f2 = new Formula("   0.0100  +     X5+ 17 * 19.00000 ");
                Assert.IsTrue(f1.Equals(f2));
            }


            [TestMethod(), Timeout(2000)]
            [TestCategory("28")]
            public void TestEqualsNullAndString()
            {
                Formula f = new Formula("2");
                Assert.IsFalse(f.Equals(null));
                Assert.IsFalse(f.Equals(""));
            }


            // Tests of == operator
            [TestMethod(), Timeout(2000)]
            [TestCategory("29")]
            public void TestEq()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsTrue(f1 == f2);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("30")]
            public void TestEqFalse()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("5");
                Assert.IsFalse(f1 == f2);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("31")]
            public void TestEqNull()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsFalse(null == f1);
                Assert.IsFalse(f1 == null);
                Assert.IsTrue(f1 == f2);
            }


            // Tests of != operator
            [TestMethod(), Timeout(2000)]
            [TestCategory("32")]
            public void TestNotEq()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("2");
                Assert.IsFalse(f1 != f2);
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("33")]
            public void TestNotEqTrue()
            {
                Formula f1 = new Formula("2");
                Formula f2 = new Formula("5");
                Assert.IsTrue(f1 != f2);
            }


            // Test of ToString method
            [TestMethod(), Timeout(2000)]
            [TestCategory("34")]
            public void TestString()
            {
                Formula f = new Formula("2*5");
                Assert.IsTrue(f.Equals(new Formula(f.ToString())));
            }


            // Tests of GetHashCode method
            [TestMethod(), Timeout(2000)]
            [TestCategory("35")]
            public void TestHashCode()
            {
                Formula f1 = new Formula("2*5");
                Formula f2 = new Formula("2*5");
                Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
            }

            // Technically the hashcodes could not be equal and still be valid,
            // extremely unlikely though. Check their implementation if this fails.
            [TestMethod(), Timeout(2000)]
            [TestCategory("36")]
            public void TestHashCodeFalse()
            {
                Formula f1 = new Formula("2*5");
                Formula f2 = new Formula("3/8*2+(7)");
                Assert.IsTrue(f1.GetHashCode() != f2.GetHashCode());
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("37")]
            public void TestHashCodeComplex()
            {
                Formula f1 = new Formula("2 * 5 + 4.00 - _x");
                Formula f2 = new Formula("2*5+4-_x");
                Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
            }


            // Tests of GetVariables method
            [TestMethod(), Timeout(2000)]
            [TestCategory("38")]
            public void TestVarsNone()
            {
                Formula f = new Formula("2*5");
                Assert.IsFalse(f.GetVariables().GetEnumerator().MoveNext());
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("39")]
            public void TestVarsSimple()
            {
                Formula f = new Formula("2*X2");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X2" };
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("40")]
            public void TestVarsTwo()
            {
                Formula f = new Formula("2*X2+Y3");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "Y3", "X2" };
                Assert.AreEqual(actual.Count, 2);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("41")]
            public void TestVarsDuplicate()
            {
                Formula f = new Formula("2*X2+X2");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X2" };
                Assert.AreEqual(actual.Count, 1);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("42")]
            public void TestVarsComplex()
            {
                Formula f = new Formula("X1+Y2*X3*Y2+Z7+X1/Z8");
                List<string> actual = new List<string>(f.GetVariables());
                HashSet<string> expected = new HashSet<string>() { "X1", "Y2", "X3", "Z7", "Z8" };
                Assert.AreEqual(actual.Count, 5);
                Assert.IsTrue(expected.SetEquals(actual));
            }

            // Tests to make sure there can be more than one formula at a time
            [TestMethod(), Timeout(2000)]
            [TestCategory("43")]
            public void TestMultipleFormulae()
            {
                Formula f1 = new Formula("2 + a1");
                Formula f2 = new Formula("3");
                Assert.AreEqual(2.0, f1.Evaluate(x => 0));
                Assert.AreEqual(3.0, f2.Evaluate(x => 0));
                Assert.IsFalse(new Formula(f1.ToString()) == new Formula(f2.ToString()));
                IEnumerator<string> f1Vars = f1.GetVariables().GetEnumerator();
                IEnumerator<string> f2Vars = f2.GetVariables().GetEnumerator();
                Assert.IsFalse(f2Vars.MoveNext());
                Assert.IsTrue(f1Vars.MoveNext());
            }

            // Repeat this test to increase its weight
            [TestMethod(), Timeout(2000)]
            [TestCategory("44")]
            public void TestMultipleFormulaeB()
            {
                TestMultipleFormulae();
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("45")]
            public void TestMultipleFormulaeC()
            {
                TestMultipleFormulae();
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("46")]
            public void TestMultipleFormulaeD()
            {
                TestMultipleFormulae();
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("47")]
            public void TestMultipleFormulaeE()
            {
                TestMultipleFormulae();
            }

            // Stress test for constructor
            [TestMethod(), Timeout(2000)]
            [TestCategory("48")]
            public void TestConstructor()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // This test is repeated to increase its weight
            [TestMethod(), Timeout(2000)]
            [TestCategory("49")]
            public void TestConstructorB()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("50")]
            public void TestConstructorC()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            [TestMethod(), Timeout(2000)]
            [TestCategory("51")]
            public void TestConstructorD()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }

            // Stress test for constructor
            [TestMethod(), Timeout(2000)]
            [TestCategory("52")]
            public void TestConstructorE()
            {
                Formula f = new Formula("(((((2+3*X1)/(7e-5+X2-X4))*X5+.0005e+92)-8.2)*3.14159) * ((x2+3.1)-.00000000008)");
            }
    }
}