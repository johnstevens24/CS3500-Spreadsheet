using System;
using System.Collections;
using System.Text.RegularExpressions;

//John Stevens Sept 3, 2021
//CS 3500 Prof. Kopta

namespace FormulaEvaluator
{
    /// <summary>
    /// The evaluator class is used to compute the final value of one cell on a spreadsheet
    /// </summary>
    public static class Evaluator
    {
        private static Stack valueStack = new Stack();
        private static Stack operatorStack = new Stack();

        public delegate int Lookup(String v);

        /// <summary>
        /// This method takes a string, processes it as a math equation, and outputs the value in integer form
        /// </summary>
        /// <param name="equation">the string representing the equation</param>
        /// <param name="variableEvaluator">a delegate that is used to lookup variable values</param>
        /// <returns>the integer value of the string's equation</returns>
        public static int Evaluate(String equation, Lookup variableEvaluator)
        {
            String[] substrings = Regex.Split(equation, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            checkForIllegalChars(equation);
            processTokens(substrings, variableEvaluator);
            return (int)valueStack.Pop();
        }

        /// <summary>
        /// This method ensures that if there is an illegal character such as "$" in the string, that it will be caught
        /// </summary>
        /// <param name="equation">the string representing the equation</param>
        private static void checkForIllegalChars(String equation)
        {
            Char[] characters = equation.ToCharArray();
            int leftCounter = 0;
            int rightCounter = 0;
            for(int i = 0; i < characters.Length; i++)
                if(Char.IsDigit(characters[i]) == false)
                    if (Char.IsLetter(characters[i]) == false)
                        switch (characters[i])
                        {
                        case ' ':
                        case '*': 
                        case '+': 
                        case '-': 
                        case '/': break;
                        case '(': leftCounter++; break;
                        case ')': rightCounter++;
                                if(rightCounter>leftCounter) //if there are ever more right parenthesis than left in the stack, its an invalid argument
                                    throw new ArgumentException("This equation contains an invalid input"); break;

                        default: throw new ArgumentException("This equation contains an invalid input");
                        }

            if(leftCounter != rightCounter)
                throw new ArgumentException("This equation contains an invalid input (incorrect number of parenthesis)");
        }

        /// <summary>
        /// This method takes the array of substrings and processes each one, dividing, adding, placing them into stacks, etc until there
        /// is only one value left in the value stack.
        /// </summary>
        /// <param name="substrings">list of substrings to be processed</param>
        /// <param name="variableEvaluator">a delegate used to look up the value of variables</param>
        private static void processTokens(String[] substrings, Lookup variableEvaluator)
        {
            valueStack.Clear(); 
            operatorStack.Clear(); //failsafe in case FormulaEvaluator is used mulitple times and a previous execution had an invalid argument
            Boolean parenCheck = false; //This makes sure that if the input contains parenthesis with no operators in between, it throws an exception. I know this is a bit cryptic, but its simple and it works.
            foreach (String s in substrings)
                switch (s)
                {
                    case "": break;
                    case " ": break;
                    case "*": operatorStack.Push(s); parenCheck = false; break;
                    case "/": operatorStack.Push(s); parenCheck = false; break;
                    case "-":
                        parenCheck = false;
                        if (valueStack.Count > 1 && (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        {
                            arithmeticFunction();
                            operatorStack.Push(s);
                        }
                        else
                            operatorStack.Push(s);
                        break;
                    case "+":
                        parenCheck = false;
                        if (valueStack.Count > 1 && (operatorStack.Peek().Equals("+") || operatorStack.Peek().Equals("-")))
                        {
                            arithmeticFunction();
                            operatorStack.Push(s);
                        }
                        else
                            operatorStack.Push(s);
                        break;
                    case "(": operatorStack.Push(s); parenCheck = true; break;

                    case ")":
                        
                        if (operatorStack.Peek().Equals("+"))
                        {
                            arithmeticFunction();
                        }
                        else
                        if (operatorStack.Peek().Equals("-"))
                        {
                            arithmeticFunction();
                        }

                        if (operatorStack.Count > 0 && operatorStack.Peek() is "(")
                        {
                            if (parenCheck == true)
                                throw new ArgumentException("This equation contains an invalid input");
                            operatorStack.Pop();
                        }
                        if (operatorStack.Count > 0 && operatorStack.Peek() is "/")
                        {
                            arithmeticFunction();
                        }
                        else
                        if (operatorStack.Count > 0 && operatorStack.Peek() is "*")
                        {
                            arithmeticFunction();
                        }
                        parenCheck = false;
                        break;

                    default:
                        int tempInt; //TryParse requires that the int variable its going to output to already exists
                        if (Int32.TryParse(s, out tempInt))
                        {
                            processInteger(tempInt);
                        }
                        else //at this point "s" must be either a variable or an invalid input
                        {
                            if (stringIsValidVariable(s))
                            {
                                tempInt = variableEvaluator(s); //if the variable doesn't have a value, the delegate shoud throw an exception on its own
                                processInteger(tempInt);
                            }
                            else
                                throw new ArgumentException("This equation contains an invalid input");
                        }
                    break;
                }

            if (operatorStack.Count > 0)
            {
                if (operatorStack.Count == 1)
                    arithmeticFunction();
                else 
                    throw new ArgumentException("This equation contains an invalid input");
            }
            if (valueStack.Count == 0)
                throw new ArgumentException("This equation contains an invalid input"); //probably an empty input

        }

        /// <summary>
        /// This helper method takes values off the value stack and applies whatever operator is ontop of the operator stack
        /// </summary>
        private static void arithmeticFunction()
        {
            if (valueStack.Count > 1) //checks in case the input is "-(3) + ..." or some similar invalid equation
            {
                int tempValue1 = (int)valueStack.Pop();
                int tempValue2 = (int)valueStack.Pop();
                switch (operatorStack.Pop())
                {
                    case "+":
                        valueStack.Push(tempValue2 + tempValue1);
                        break;
                    case "-":
                        valueStack.Push(tempValue2 - tempValue1);
                        break;
                    case "/":
                        valueStack.Push(tempValue2 / tempValue1);
                        break;
                    case "*":
                        valueStack.Push(tempValue2 * tempValue1);
                        break;
                }
            }
            else
                throw new ArgumentException("This equation contains an invalid input");
        }

        /// <summary>
        /// This method checks to see whether or not the passed in string meets the criteria for being a variable
        /// </summary>
        /// <param name="s">the name of a variable</param>
        /// <returns>true or false based on if it meets the criteria</returns>
        private static bool stringIsValidVariable(String s)
        {
            if (s.Length < 2)
                return false;

            if(s.StartsWith(" ")) //sometimes the beginning of s is a " ", this causes a problem if not dealt with
            {
                s = s.Substring(1, s.Length-1);
            }

            char[] chars = s.ToCharArray();

            

            if(Char.IsLetter(chars[0])==false) //if it doesn't start with a letter, its automatically not a variable
            {
                return false;
            }     
            int lastLetterIndex = 0;

            for(int i = 1; i < s.Length; i++)
            {
                if (Char.IsDigit(chars[i]) == true)
                {
                    lastLetterIndex = i;
                    i = s.Length + 1;
                }
            }

            for (int i = lastLetterIndex; i < s.Length; i++)
            {
                if (Char.IsDigit(chars[i]) == false) //if any of the chars after the first digit aren't digits, its not a variable
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// This method applies the logic used to process integers. It decides whether to divide them, multiply them, or just push them straight
        /// onto the value stack.
        /// </summary>
        /// <param name="tempInt">the integer currently being processed</param>
        private static void processInteger(int tempInt)
        {
            if ((operatorStack.Count > 0 && valueStack.Count > 0) && (operatorStack.Peek() is "*" || operatorStack.Peek() is "/"))
            {
                int tempInt2 = (int)valueStack.Pop();
                String tempOp = (String)operatorStack.Pop();
                if (tempOp is "*")
                    tempInt = tempInt2 * tempInt;
                else
                    if(tempInt != 0)
                        tempInt = tempInt2 / tempInt;
                    else
                        throw new ArgumentException("The entered equation causes a divide by zero scenario");
            }

            valueStack.Push(tempInt);
        }
    }
}
