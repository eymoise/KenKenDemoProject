using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KenKenLogic;

namespace KenKenGUI
{
    public static class ExtKenKen
    {
        public static string StringFormat(this MathOp mathOp)
        {
            if (mathOp == MathOp.Addition)
            {
                return "+";
            }
            else if (mathOp == MathOp.Subtraction)
            {
                return "\u2212";
            }
            else if (mathOp == MathOp.Multiplication)
            {
                return "\u00D7";
            }
            else
            {
                // Division
                return "\u00F7";
            }
        }
    }
}
