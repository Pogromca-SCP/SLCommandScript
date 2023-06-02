using CommandSystem;
using SLCommandScript.Core.Language.Expressions;
using System;

namespace SLCommandScript.Core.Language;

public class Interpreter : Expr.IVisitor<bool>, Direct.IVisitor<bool>
{
    public ICommandSender Sender { get; private set; }

    public string ErrorMessage { get; set; }

    public Interpreter(ICommandSender sender)
    {
        Reset(sender);
        ErrorMessage = null;
    }

    public void Reset(ICommandSender sender)
    {
        Sender = sender;
    }

    public bool VisitCommandExpr(Expr.Command expr)
    {
        if (expr is null)
        {
            ErrorMessage = "[Interpreter] Provided command expression is invalid";
            return false;
        }

        if (expr.Cmd is null)
        {
            ErrorMessage = "[Interpreter] Cannot execute an invalid command";
            return false;
        }

        if (expr.Arguments is null)
        {
            ErrorMessage = "[Interpreter] Provided command arguments are invalid";
            return false;
        }

        if (expr.Arguments.Length < 1)
        {
            ErrorMessage = "[Interpreter] Provided command arguments array is empty";
            return false;
        }
        
        var result = expr.Cmd.Execute(new ArraySegment<string>(expr.Arguments, 1, expr.Arguments.Length), Sender, out var message);

        if (!result)
        {
            ErrorMessage = message;
        }

        return result;
    }

    public bool VisitDirectiveExpr(Expr.Directive expr)
    {
        if (expr is null)
        {
            ErrorMessage = "[Interpreter] Provided directive expression is invalid";
            return false;
        }

        if (expr.Body is null)
        {
            ErrorMessage = "[Interpreter] Directive expression body is invalid";
            return false;
        }

        return expr.Body.Accept(this);
    }

    public bool VisitForeachDirect(Direct.Foreach direct)
    {
        if (direct is null)
        {
            ErrorMessage = "[Interpreter] Provided foreach directive is invalid";
            return false;
        }

        if (direct.Body is null)
        {
            ErrorMessage = "[Interpreter] Foreach directive body is invalid";
            return false;
        }

        if (direct.Iterable is null)
        {
            ErrorMessage = "[Interpreter] Foreach directive iterable object is invalid";
            return false;
        }

        while (!direct.Iterable.IsAtEnd)
        {
            direct.Iterable.LoadNext(null);
            var result = direct.Body.Accept(this);

            if (!result)
            {
                return false;
            }
        }
        
        return true;
    }

    public bool VisitIfDirect(Direct.If direct)
    {
        if (direct is null)
        {
            ErrorMessage = "[Interpreter] Provided if directive is invalid";
            return false;
        }

        if (direct.Then is null)
        {
            ErrorMessage = "[Interpreter] If directive then branch is invalid";
            return false;
        }

        if (direct.Condition is null)
        {
            ErrorMessage = "[Interpreter] If directive condition is invalid";
            return false;
        }

        if (direct.Condition.Accept(this))
        {
            return direct.Then.Accept(this);
        }
        else if (direct.Else is not null)
        {
            return direct.Else.Accept(this);
        }
        else
        {
            return true;
        }
    }
}
