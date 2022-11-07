
using SpreadsheetUtilities;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FormulaTests
{
    
    [TestClass]
    public class FormulaTests
    {
        public bool IsVar(string s)
        {
            return Char.IsLetter(s.ToCharArray()[0]);
        }

        [TestMethod]
        public void TestMethod1()
        {
        }

        //Test Constructors & ToString
        [TestMethod]
        public void TestOneParamterConstructor()
        {
            Formula f1 = new("3 + 4");
            Assert.AreEqual("3+4", f1.ToString());
        }
        [TestMethod]
        public void TestFullConstructor()
        {
            Formula f1 = new("A2 + 3", s => s.ToLower(), s => true);
            Assert.AreEqual("a2+3", f1.ToString());
        }
        [TestMethod]
        public void TestToStringComplex()
        {
            Formula f1 = new("(3 * A2)+ 7/b3 * (c6/Cc3)", s => s.ToUpper(), s => true);
            Assert.AreEqual("(3*A2)+7/B3*(C6/CC3)", f1.ToString());
        }
        [TestMethod]
        public void TestToStringVarChecking()
        {
            Formula f1 = new("(3 * A2)+ 7/b3 * (c6/Cc3)", s => s.ToLower(), IsVar);
            Assert.AreEqual("(3*a2)+7/b3*(c6/cc3)", f1.ToString());
        }
        //Test Syntax Checking
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Given formula was empty.")]
        public void TestEmptyFormula()
        {
            Formula f1 = new("");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Starting token may not be and operator.")]
        public void TestStartingOp()
        {
            Formula f1 = new("-3 + 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Ending token may not be and operator.")]
        public void TestEndingOp()
        {
            Formula f1 = new("3 + 2 *");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Token after an open paren may not be an operator.")]
        public void TestParenRule()
        {
            Formula f1 = new("(-3 + 2)");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Number of closing parens exceeds number of opening parens.")]
        public void TestRightParenRule()
        {
            Formula f1 = new("(3 + 2))))) + 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Token after closing paren must be another closing paren or operator.")]
        public void TestExtraRuleParen()
        {
            Formula f1 = new("(3 + 2) 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "You may not have 2 operators in a row.")]
        public void TestOperatorRule()
        {
            Formula f1 = new("3 +- 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "You must have a closing paren or operator following a number.")]
        public void TestExtraRuleNum()
        {
            Formula f1 = new("2 A6 + 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "You must have a closing paren or operator following a variable.")]
        public void TestExtraRuleVar()
        {
            Formula f1 = new("A6 2 + 2");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Invalid token in formula.")]
        public void TestInvalidVar()
        {
            Formula f1 = new("A6 * 2 + 2", s => s, s => false);
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Number of opening parens must equal number of closing parens.")]
        public void TestBalancedParenRule()
        {
            Formula f1 = new("(((3 +2) * 5)");
        }
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException), "Invalid token in formula.")]
        public void TestEndBadVar()
        {
            Formula f1 = new("3 * A6", s => s, s => false);
        }
        //Test Evaluate
        [TestMethod]
        public void TestSimpleEval()
        {
            Formula f1 = new("(3 * 3) + 1.5");
            Assert.AreEqual(Double.Parse("10.5"), f1.Evaluate(s => 0.0));
        }
        [TestMethod]
        public void TestSimpleEvalNoEndOp()
        {
            Formula f1 = new("3 * 3");
            Assert.AreEqual(Double.Parse("9"), f1.Evaluate(s => 0.0));
        }
        [TestMethod]
        public void TestComplexEval()
        {
            Formula f1 = new("(3 * 20.2) + 7.63 / 5 + (4.00 + 2.0) + (3.0 * (4 * 10) * 7)");
            Assert.AreEqual(Double.Parse("908.126"), f1.Evaluate(s => 0.0));
        }
        [TestMethod]
        public void TestSimpleEvalWithVar()
        {
            Formula f1 = new("A6 * 3 + 1.5");
            Assert.AreEqual(Double.Parse("10.5"), f1.Evaluate(s => 3));
        }
        [TestMethod]
        public void TestComplexEvalWithVar()
        {
            Formula f1 = new("(A6 * 20.2) + 7.63 / 5 + (A7 - 2.0)");
            Assert.AreEqual(Double.Parse("84.326"), f1.Evaluate(s => 4.0));
        }
        [TestMethod]
        public void TestDivByZero()
        {
            Formula f1 = new("10 / 0");
            string error = ((FormulaError)(f1.Evaluate(s => 0))).Reason;
            Assert.AreEqual("Tried to divide by 0.", error);
        }
        //Test GetVariables
        [TestMethod]
        public void TestOneVarGet()
        {
            Formula f1 = new("A6");
            List<string> l1= f1.GetVariables().ToList();
            Assert.AreEqual(1, l1.Count);
            Assert.AreEqual("A6", l1[0]);
        }
        [TestMethod]
        public void TestNoVarGet()
        {
            Formula f1 = new("6 + 2 * 4 - 1", s => s, s => false);
            List<string> l1 = f1.GetVariables().ToList();
            Assert.AreEqual(0, l1.Count);
        }
        [TestMethod]
        public void TestFewVarGet()
        {
            Formula f1 = new("(A6 + A7) * A8 + A9");
            List<string> l1 = f1.GetVariables().ToList();
            Assert.AreEqual(4, l1.Count);
            Assert.AreEqual("A6", l1[0]);
            Assert.AreEqual("A7", l1[1]);
            Assert.AreEqual("A8", l1[2]);
            Assert.AreEqual("A9", l1[3]);
        }
        [TestMethod]
        public void TestManyVarGet()
        {
            StringBuilder sb = new();
            List<string> vars = new();
            for(int i = 0; i < 26; i++)
            {
                vars.Add("" + (char)('a' + i));
                sb.Append("" + (char)('a' + i));
                sb.Append(" + ");
            }
            sb.Append("A6");
            vars.Add("A6");
            Formula f1 = new(sb.ToString());
            List<string> l1 = f1.GetVariables().ToList();
            for (int i = 0; i < l1.Count; i++)
                Assert.AreEqual(vars[i], l1[i]);
        }
        //Test Equals, ==, !=
        [TestMethod]
        public void TestSpaceDiff()
        {
            Formula f1 = new("3+3 * 4");
            Formula f2 = new("3+3*4");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f2 == f1);
            Assert.IsFalse(f1 != f2);
        }
        [TestMethod]
        public void TestVarDiff()
        {
            Formula f1 = new("3+3 * a6", s => s.ToUpper(), s => true);
            Formula f2 = new("3+3*A6");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f2 == f1);
            Assert.IsFalse(f1 != f2);
        }
        [TestMethod]
        public void TestOrderDiff()
        {
            Formula f1 = new("4*3+3");
            Formula f2 = new("3+3*4");
            Assert.IsFalse(f1.Equals(f2));
            Assert.IsFalse(f2 == f1);
            Assert.IsTrue(f1 != f2);
        }
        [TestMethod]
        public void TestNoDiff()
        {
            Formula f1 = new("3+3*4");
            Formula f2 = new("3+3*4");
            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(f2 == f1);
            Assert.IsFalse(f1 != f2);
        }
        [TestMethod]
        public void TestSelfEqual()
        {
            Formula f1 = new("3+3*4");
            Assert.IsTrue(f1.Equals(f1));
        }
        [TestMethod]
        public void TestNullEqual()
        {
            Formula f1 = new("3+3*4");
            Assert.IsFalse(f1.Equals(null));
        }
        [TestMethod]
        public void TestTypeDiff()
        {
            Formula f1 = new("3+3*4");
            string f2 = "3+3*4";
            Assert.IsFalse(f1.Equals(f2));
        }
        //Test hashcode
        [TestMethod]
        public void TestSpaceDiffHash()
        {
            Formula f1 = new("3+3 * 4");
            Formula f2 = new("3+3*4");
            Assert.IsTrue(f2.GetHashCode() == f1.GetHashCode());
        }
        [TestMethod]
        public void TestVarDiffHash()
        {
            Formula f1 = new("3+3 * a6", s => s.ToUpper(), s => true);
            Formula f2 = new("3+3*A6");
            Assert.IsTrue(f2.GetHashCode() == f1.GetHashCode());
        }
        [TestMethod]
        public void TestOrderDiffHash()
        {
            Formula f1 = new("4*3+3");
            Formula f2 = new("3+3*4");
            Assert.IsTrue(f2.GetHashCode() != f1.GetHashCode());
        }
        [TestMethod]
        public void TestNoDiffHash()
        {
            Formula f1 = new("3+3*4");
            Formula f2 = new("3+3*4");
            Assert.IsTrue(f2.GetHashCode() == f1.GetHashCode());
        }
        [TestMethod]
        public void TestSelfEqualHash()
        {
            Formula f1 = new("3+3*4");
            Assert.IsTrue(f1.GetHashCode() == f1.GetHashCode());
        }
    }
}