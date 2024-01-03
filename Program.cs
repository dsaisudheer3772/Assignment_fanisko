using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Define an interface for the calculator operations
public interface ICalculatorOperation
{
    double Apply(double operand1, double operand2);
}

// Concrete class for addition operation
public class AdditionOperation : ICalculatorOperation
{
    public double Apply(double operand1, double operand2)
    {
        return operand1 + operand2;
    }
}

// Concrete class for subtraction operation
public class SubtractionOperation : ICalculatorOperation
{
    public double Apply(double operand1, double operand2)
    {
        return operand1 - operand2;
    }
}

// Concrete class for multiplication operation
public class MultiplicationOperation : ICalculatorOperation
{
    public double Apply(double operand1, double operand2)
    {
        return operand1 * operand2;
    }
}

// Concrete class for division operation
public class DivisionOperation : ICalculatorOperation
{
    public double Apply(double operand1, double operand2)
    {
        if (operand2 == 0)
        {
            throw new ArgumentException("Cannot divide by zero.");
        }
        return operand1 / operand2;
    }
}

// Calculator class to perform calculations based on user input
public class Calculator
{
    private readonly Dictionary<string, ICalculatorOperation> operations;

    public Calculator()
    {
        operations = new Dictionary<string, ICalculatorOperation>(StringComparer.OrdinalIgnoreCase)
        {
            { "+", new AdditionOperation() },
            { "-", new SubtractionOperation() },
            { "*", new MultiplicationOperation() },
            { "/", new DivisionOperation() }
        };
    }

    public double PerformCalculation(string input)
    {
        // Use regular expression to split the input into numbers and operators
        string[] tokens = Regex.Split(input, @"(?<=[-+*/()])|(?=[-+*/()])")
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();

        Stack<double> operandStack = new Stack<double>();
        Stack<string> operatorStack = new Stack<string>();

        foreach (var token in tokens)
        {
            if (double.TryParse(token, out double number))
            {
                operandStack.Push(number);
            }
            else if (token == "(")
            {
                operatorStack.Push(token);
            }
            else if (token == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                {
                    ApplyOperator(operandStack, operatorStack.Pop());
                }

                if (operatorStack.Count == 0)
                {
                    throw new ArgumentException("Mismatched parentheses.");
                }

                operatorStack.Pop(); // Pop the "("
            }
            else if (operations.ContainsKey(token))
            {
                while (operatorStack.Count > 0 && Precedence(operatorStack.Peek()) >= Precedence(token))
                {
                    ApplyOperator(operandStack, operatorStack.Pop());
                }

                operatorStack.Push(token);
            }
            else
            {
                throw new ArgumentException($"Unsupported token: {token}");
            }
        }

        while (operatorStack.Count > 0)
        {
            ApplyOperator(operandStack, operatorStack.Pop());
        }

        if (operandStack.Count != 1 || operatorStack.Count != 0)
        {
            throw new ArgumentException("Invalid expression.");
        }

        return operandStack.Pop();
    }

    private void ApplyOperator(Stack<double> operandStack, string operatorToken)
    {
        if (operations.TryGetValue(operatorToken, out var operation))
        {
            if (operandStack.Count < 2)
            {
                throw new ArgumentException("Invalid expression.");
            }

            double operand2 = operandStack.Pop();
            double operand1 = operandStack.Pop();
            double result = operation.Apply(operand1, operand2);
            operandStack.Push(result);
        }
        else
        {
            throw new ArgumentException($"Unsupported operator: {operatorToken}");
        }
    }

    private int Precedence(string op)
    {
        switch (op)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            default:
                return 0;
        }
    }
}

public class CalculatorTests
{
    static void Main()
    {
        CalculatorTests.RunTests();
        Console.ReadLine();
    }

    static void RunTests()
    {
        // Create a calculator
        Calculator calculator = new Calculator();

        // Test cases
        Test("1 + 3", 4);
        Test("1 + (3 * 4)", 13);
        Test("(2 + 3) * 5", 25);
        Test("10 / 2", 5);
        Test("7 - 4", 3);
        Test("2.5 * 4", 10);
        Test("1.2 + 3.8", 5);
        Test("1 + 2 * 3", 7);
        Test("((3 - 2) * 5) / 2", 2.5);
        Test("2 * (1 + 3) - 4 / 2", 10);

        // Additional test cases with parentheses and complex expressions
        Test("((1 + 2) * 3) / (4 - 1)", 3);
        Test("(1 + 2) * (3 + 4)", 21);
        Test("(5 - 2) * (8 / 2)", 18);

        // Test case with division by zero
        Test("5 / 0", double.PositiveInfinity);

        // Test case with mismatched parentheses
        Test("(1 + 2 * 3", double.NaN);
    }

    static void Test(string input, double expected)
    {
        Calculator calculator = new Calculator();

        try
        {
            double result = calculator.PerformCalculation(input);

            Console.WriteLine($"Test: {input}");
            Console.WriteLine($"Expected: {expected}");
            Console.WriteLine($"Result: {result}");

            if (double.IsNaN(expected))
            {
                Console.WriteLine(result.Equals(expected) ? "Test Passed (Exception Expected)" : "Test Failed (Exception Expected)");
            }
            else
            {
                Console.WriteLine(result == expected ? "Test Passed" : "Test Failed");
            }

            Console.WriteLine(new string('-', 30));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test: {input}");
            Console.WriteLine($"Expected: {expected}");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(double.IsNaN(expected) ? "Test Passed (Exception Expected)" : "Test Failed (Unexpected Exception)");
            Console.WriteLine(new string('-', 30));
        }
    }
}

