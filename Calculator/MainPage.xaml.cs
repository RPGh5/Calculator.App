using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Calculator;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Calculation> history { get; set; }
    private StringBuilder currentCalculation = new StringBuilder();
    private double result = 0;
    private bool clearDisplayOnNextInput = false;
    private bool dumpseaker = false;

    public MainPage()
    {
        history = new ObservableCollection<Calculation>();
        InitializeComponent();
        BindingContext = this;
    }

    void OnButtonClicked(object sender, EventArgs e)
    {
        Button button_clicked = (Button)sender;
        string button_value = button_clicked.Text;
        if(button_value=="+"|| button_value == "-" || button_value == "*" || button_value == "/")
        {
            currentCalculation.Append(" " + button_value + " ");
            this.displayLabel.Text = currentCalculation.ToString();
            clearDisplayOnNextInput = false;
        }
        else
        {
            if (clearDisplayOnNextInput == true)
            {
                currentCalculation.Clear();
                clearDisplayOnNextInput = false;
                currentCalculation.Append(button_value);
                this.displayLabel.Text = currentCalculation.ToString();
            }
            else
            {
                currentCalculation.Append(button_value);
                this.displayLabel.Text = currentCalculation.ToString();
            }
        }

    }
    private void historyListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var item = e.Item as Calculation;
        string res = item.Result.ToString();
        if (clearDisplayOnNextInput)
        {
            currentCalculation.Clear();
            clearDisplayOnNextInput = false;
        }
        currentCalculation.Append(res);
        this.displayLabel.Text = currentCalculation.ToString();
    }

    public void OnClearClicked(object sender, EventArgs e)
    {
        currentCalculation.Clear();
        this.displayLabel.Text = currentCalculation.ToString();
        result = 0;
    }

    public void OnEqualsClicked(object sender, EventArgs e)
    {
        result = CalculateResult(InfixToRPN(currentCalculation.ToString()));
        this.displayLabel.Text = result.ToString();
        if(!dumpseaker)
            history.Add(new Calculation(currentCalculation.ToString(), result));
        else
            dumpseaker= false;
        currentCalculation.Clear();
        currentCalculation.Append(result);
        clearDisplayOnNextInput = true;
    }

    private double CalculateResult(List<string> rpn)
    {
        var stack = new Stack<double>();
        foreach (var token in rpn)
        {
            if (double.TryParse(token, out var value))
            {
                stack.Push(value);
            }
            else
            {
                if (stack.Count > 1)
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    switch (token)
                    {
                        case "+":
                            stack.Push(left + right);
                            break;
                        case "-":
                            stack.Push(left - right);
                            break;
                        case "*":
                            stack.Push(left * right);
                            break;
                        case "/":
                            if (right == 0)
                            {
                                dumpseaker= true;
                                DisplayAlert("Error", "You cannot divide by 0!", "Sorry");
                                stack.Push(0);
                                return stack.Pop();
                            }
                            else
                            {
                                stack.Push(left / right);
                            }
                            break;
                    }
                }
            }
        }
        return stack.Pop();
    }

    private List<string> InfixToRPN(string calculation)
    {
        var output = new List<string>();
        var stack = new Stack<string>();
        var tokens = calculation.Split(' ');
        foreach (var token in tokens)
        {
            if (double.TryParse(token, out _))
            {
                output.Add(token);
            }
            else
            {
                while (stack.Count > 0 && GetPrecedence(token) <= GetPrecedence(stack.Peek()))
                {
                    output.Add(stack.Pop());
                }
                stack.Push(token);
            }
        }
        while (stack.Count > 0)
        {
            output.Add(stack.Pop());
        }
        return output;
    }

    private int GetPrecedence(string token)
    {
        if (token == "+" || token == "-")
        {
            return 1;
        }
        else if (token == "*" || token == "/")
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }

    public class Calculation
    {
        public string CalculationString { get; set; }
        public string Result { get; set; }

        public Calculation(string calculation, double result)
        {
            this.CalculationString = calculation;
            this.Result = result.ToString();

        }
    }

}

