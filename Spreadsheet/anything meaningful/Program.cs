using System;
using FormulaEvaluator;

namespace anything_meaningful
{
    class Program
    {
        public delegate int Lookup(String v);

        static void Main(string[] args)
        {
            //Console.WriteLine(Evaluator.Evaluate("37+4*5-(-10)", null)); //
            //Console.WriteLine(Evaluator.Evaluate("3+ (aacoolj1) ", null)); //42
            Console.WriteLine(Evaluator.Evaluate("(10)-34/2+13*(4-1)", null)); //32
            Console.WriteLine(Evaluator.Evaluate("((14+1)*2)+((((5*2))))", null)); //40
            Console.WriteLine(Evaluator.Evaluate("((14+1)*2)+((((5*2))))-502/2*(341+7)-3251*(23+4-5)+2*3*2*4*5*(9-8+4-8/(1+1))", null)); //-158590
        }

    }
}
