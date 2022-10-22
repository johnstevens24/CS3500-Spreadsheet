/* Author: John Stevens 9/17/2021 2:59 pm
 * Built on the skeleton code provided by Prof. Kopta and Joe Zachary.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private String formula;
        private Func<String, String> normalize;
        private Func<String, bool> isValid;
        private String revisedFormula;
        private List<String> normalizedVars;
        private IEnumerable<String> tokens;
        private Stack<object> valueStack;
        private Stack<object> operatorStack;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            this.formula = formula;
            this.normalize = normalize;
            this.isValid = isValid;
            normalizedVars = new List<string>();
            

            tokens = GetTokens(formula);
            revisedFormula = checkTokens(tokens);
        }

        /// <summary>
        /// This method checks the input formula for syntax errors. It checks to see if the formula complies with all the rules layed out in PS3(one token rule, right parentheses rule, etc) as well as
        /// if the formula contains any illegal chars. Basically this method will catch anything that would throw an exception come evaluation time.
        /// </summary>
        /// <param name="list">tokens representing the formula</param>
        /// <returns></returns>
        private string checkTokens(IEnumerable<string> list)
        {
             String tempFormula = ""; //this string will be built up over each iteration and returned at the end
            String lastToken = "";
            double tempDub;
            int leftCounter = 0; //These count the number of parentheses so it can check if there are exactly one '(' for each ')'
            int rightCounter = 0;

            //One token rule
            if (tokens.Count() < 1)
                throw new FormulaFormatException("There are no tokens in this formula. Add some and try again");

            foreach (String s in list)
            {
                //Starting token Rule
                if(tempFormula.Equals(""))
                {
                    if (double.TryParse(s, out tempDub) || isVar(s) || s.Equals("("))
                    {
                        //Do nothing, starting token rule is fulfilled
                    }
                    else
                        throw new FormulaFormatException("Starting token rule (Formula must star with a number, variable, or '(', has not been fulfilled. Check the first thing in your formula to see if it is what its supposed to be.");
                }

                //Parenthesis/Operator Following rule
                if(lastToken == "(" || lastToken == "+" || lastToken == "-" || lastToken == "/" || lastToken == "*")
                {
                    if ((double.TryParse(s, out tempDub) || isVar(s) || s.Equals("(")) == false)
                        throw new FormulaFormatException("Parenthesis/Operator Following rule has not been fulfilled. Check your formula for tokens following parentheses and operators that aren't correct.");
                }

                //Extra following rule
                //The lastToken check is to make sure this doesn't run on the first iteration and throw an exception
                if (lastToken != "" && ((double.TryParse(lastToken, out tempDub) || isVar(lastToken) || lastToken.Equals(")")) == true))
                {
                    if ((s == ")" || s == "+" || s == "-" || s == "/" || s == "*") == false)
                        throw new FormulaFormatException("Extra Following rule has not been fulfilled. Check your formula for tokens following parentheses and operators that aren't correct.");   
                }

                switch (s)
                {
                    case "+":
                    case "/":
                    case "-":
                    case "*": tempFormula = tempFormula + s; break;
                    case "(": tempFormula = tempFormula + s; leftCounter++; break;
                    case ")": tempFormula = tempFormula + s; rightCounter++;
                             //Right Parentheses rule
                             if (leftCounter < rightCounter)
                                 throw new FormulaFormatException("This formula has an unequal number of left and right parentheses. Check and see if one was added or left out by mistake");
                             break;
                    default:
                        //doubles
                        if (Double.TryParse(s, out tempDub))
                        {
                            tempFormula = tempFormula + tempDub.ToString();
                        }
                        else
                        {
                            if (isVar(s)) //check that it meets requirements of a variable. If its not a valid variable, isVar() will throw an exception
                            {
                                String tempVarString = normalize(s);
                                if (isVar(tempVarString) && isValid(tempVarString))
                                {
                                    //we have a valid variable
                                    tempFormula = tempFormula + tempVarString;
                                    if(!normalizedVars.Contains(tempVarString))
                                        normalizedVars.Add(tempVarString); //adds to the list of variables. This list will be called in the getVariables method
                                }
                            }
                            else
                                throw new FormulaFormatException("This formula contains an invalid char or sequence of chars. Look over your formula, especially your variables, for any mistakes.");
                            
                        }
                        break;
                }
                lastToken = s;
            }

            //Balanced Parentheses rule
            if (leftCounter != rightCounter)
                throw new FormulaFormatException("This formula has an unequal number of left and right parentheses. Check and see if one was added or left out by mistake");

            //Ending Token Rule
            if((double.TryParse(lastToken, out tempDub) || isVar(lastToken) || lastToken.Equals(")")) == false)
            {
                throw new FormulaFormatException("Ending token rule (Formula must end with a number, variable, or ')', has not been fulfilled. Check the last thing in your formula to see if it is what its supposed to be.");
            }

            return tempFormula;
        }

        /// <summary>
        /// This method determines if the string passed in meets the conditions for being a variable.
        /// </summary>
        /// <param name="s">potential variable name</param>
        /// <returns></returns>
        private bool isVar(String s)
        {
            Char[] chars = s.ToCharArray();
            
            if(char.IsLetter(chars[0]) || chars[0].Equals('_'))
            {
                if(chars.Length>1)
                    foreach(char c in chars)
                    {
                        if (char.IsDigit(c) || char.IsLetter(c) || c.Equals('_'))
                        {
                            //do nothing, it meets the conditions of being a variable
                        }
                        else
                            return false;
                    }
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method applies the logic used to process doubles. It decides whether to divide them, multiply them, or just push them straight
        /// onto the value stack.
        /// </summary>
        /// <param name="tempInt">the integer currently being processed</param>
        private bool processDouble(double tempDub)
        {
            if ((operatorStack.Count > 0 && valueStack.Count > 0) && (operatorStack.Peek() is "*" || operatorStack.Peek() is "/"))
            {
                double tempDub2 = (double)valueStack.Pop();
                String tempOp = (String)operatorStack.Pop();
                if (tempOp is "*")
                    tempDub = tempDub2 * tempDub;
                else
                    if (tempDub != 0)
                    tempDub = tempDub2 / tempDub;
                else
                    return false; //only returns false if its divide by zero scenario
            }

            valueStack.Push(tempDub);

            return true;
        }

        /// <summary>
        /// This helper method takes values off the value stack and applies whatever operator is ontop of the operator stack
        /// </summary>
        private bool arithmeticFunction()
        {
            if (valueStack.Count > 1) //checks in case the input is "-(3) + ..." or some similar invalid equation
            {
                double tempValue1 = (double)valueStack.Pop();
                double tempValue2 = (double)valueStack.Pop();
                switch (operatorStack.Pop())
                {
                    case "+":
                        valueStack.Push(tempValue2 + tempValue1);
                        break;
                    case "-":
                        valueStack.Push(tempValue2 - tempValue1);
                        break;
                    case "/":
                        if (tempValue1 == 0)
                            return false; //Divide by zero scenario
                        valueStack.Push(tempValue2 / tempValue1);
                        break;
                    case "*":
                        valueStack.Push(tempValue2 * tempValue1);
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            //instantiates the stacks here instead of at the creation of a formula class since that causes problems in certain scenarios
            valueStack = new Stack<object>();
            operatorStack = new Stack<object>();

            foreach (String s in tokens)
                switch (s)
                {
                    case " ": break;
                    case "*": operatorStack.Push(s); break;
                    case "/": operatorStack.Push(s); break;
                    case "-":
                        if (valueStack.Count > 1 && (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        {
                            //The below if statement calls the arithmeticFunction method, which takes tokens off the two stacks computes something, then puts them back onto the values stack.
                            //The only time its return value matters is if its a divide-by-zero scenario, in which case it returns false.
                            //This comment is true for every other time this block of code is used in this method.
                            if (arithmeticFunction() == false)
                                return new FormulaError("Divide by zero scenario.");
                            operatorStack.Push(s);
                        }
                        else
                            operatorStack.Push(s);
                        break;

                    case "+":
                        if (valueStack.Count > 1 && (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        {
                            if (arithmeticFunction() == false)
                                return new FormulaError("Divide by zero scenario.");
                            operatorStack.Push(s);
                        }
                        else
                            operatorStack.Push(s);
                        break;

                    case "(": 
                        operatorStack.Push(s); break;

                    case ")":
                        if (operatorStack.Peek().Equals("+"))
                        {
                            if (arithmeticFunction() == false)
                                return new FormulaError("Divide by zero scenario.");
                        }
                        else
                        if (operatorStack.Peek().Equals("-"))
                        {
                            if (arithmeticFunction() == false)
                                return new FormulaError("Divide by zero scenario.");
                        }

                        if (operatorStack.Count > 0 && operatorStack.Peek() is "(")
                        {
                            operatorStack.Pop();
                        }
                        if (operatorStack.Count > 0 && operatorStack.Peek() is "/")
                        {
                            if (arithmeticFunction() == false)
                                return new FormulaError("Divide by zero scenario.");
                        }
                        else
                        if (operatorStack.Count > 0 && operatorStack.Peek() is "*")
                        {
                            if(arithmeticFunction()==false)
                                return new FormulaError("Divide by zero scenario.");
                        }
                        break;

                    default:
                        double tempDub; //TryParse requires that the double variable it is going to output to already exists
                        if (Double.TryParse(s, out tempDub))
                        {
                            if(processDouble(tempDub) == false)
                                return new FormulaError("Divide by zero scenario.");
                        }
                        else //at this point "s" must be a variable
                        {
                            //This is probably unnecessary and the odds of it catching an exception are very unlikely, but it still prevents this method from possibly throwing an exception
                            try
                            {
                            tempDub = lookup(normalize(s));
                            if(!processDouble(tempDub))
                                {
                                    //only returns false if its a divide by zero scenario^^
                                    return new FormulaError("Divide by zero scenario.");
                                }
                            }
                            catch(Exception e)
                            {
                                return new FormulaError("Variable " + s + " threw an exception when being passed into the lookup delegate.");
                            }
                        }
                        break;
                }

            
            if (operatorStack.Count > 0)
            {
                if (operatorStack.Count == 1)
                    arithmeticFunction();
            }

            //Just in case converts the final answer to a string then back to a double
            double finalValue = (double)valueStack.Pop();
            String tempS = finalValue.ToString();
            finalValue = double.Parse(tempS);

            return finalValue;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return normalizedVars;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return revisedFormula;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if(obj is Formula)
            if (revisedFormula.Equals(obj.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (f1 is null && f2 is null)
                return true;

            if (f1 is null || f2 is null)
                return false;

            if (f1.GetHashCode() == f2.GetHashCode())
                return true;

            return false;
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (f1 is null && f2 is null)
                return false;

            if (f1 is null || f2 is null)
                return true;

            if (f1.GetHashCode() == f2.GetHashCode())
                return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return revisedFormula.GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}