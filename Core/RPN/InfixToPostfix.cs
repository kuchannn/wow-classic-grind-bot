using System;
using System.Collections.Generic;

namespace Core;

public static class InfixToPostfix
{
    public static List<string> Convert(ReadOnlySpan<char> span)
    {
        List<string> output = new();
        Stack<string> stack = new();

        int i = 0;
        while (i < span.Length)
        {
            ReadOnlySpan<char> c = span.Slice(i, 1);

            if (IsSpecial(c))
            {
                if (c.SequenceEqual("("))
                {
                    stack.Push(c.ToString());
                    i++;
                }
                else if (c.SequenceEqual(")"))
                {
                    while (stack.Count != 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop();
                    i++;
                }
                else if (IsOperator(span, i, out ReadOnlySpan<char> op))
                {
                    i += op.Length;

                    while (stack.Count != 0 && OperatorPriority(stack.Peek()) >= OperatorPriority(op))
                    {
                        output.Add(stack.Pop());
                    }

                    stack.Push(op.ToString());
                }
            }
            else
            {
                int start = i;
                while (i < span.Length && !IsSpecial(span.Slice(i, 1)))
                {
                    i++;
                }

                output.Add(span[start..i].ToString()); // operand
            }
        }

        while (stack.Count != 0)
        {
            output.Add(stack.Pop());
        }

        return output;

        static bool IsSpecial(ReadOnlySpan<char> c)
        {
            // where 
            // '|' means "||"
            // '&' means "&&"
            return
                c.SequenceEqual("(") ||
                c.SequenceEqual(")") ||
                c.SequenceEqual("&") ||
                c.SequenceEqual("|");
        }

        static bool IsOperator(ReadOnlySpan<char> span, int index, out ReadOnlySpan<char> @operator)
        {
            @operator = span.Slice(index, 2);
            return
                @operator.SequenceEqual(Requirement.SymbolAnd) ||
                @operator.SequenceEqual(Requirement.SymbolOr);
        }

        static int OperatorPriority(ReadOnlySpan<char> o)
        {
            if (o.SequenceEqual(Requirement.SymbolAnd))
                return 2;
            else if (o.SequenceEqual(Requirement.SymbolOr))
                return 1;

            return 0; // "(" or ")"
        }
    }
}
