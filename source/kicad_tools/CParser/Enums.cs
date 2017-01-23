using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CParser
{
    public enum Operator
    {
        None,
        UnaryNegate,
        Add, Sub, Mul, Div, Modulo, LeftShift, RightShift, BitAnd, BitOr, BitXor,   // + - * / % << >> & | ^
        LogicalAnd, LogicalOr, // and or 
        Not,   // ! (boolean/bitwise)
        Equal, NotEqual, LessThan, LessOrEqual, GreaterThan, GreaterOrEqual, // = != < <= > >=
        Index   // [ array index
    };


    public enum ScriptResult
    {
        Ok,
        ErrorReadingFile,
        NoHeader, EmptyFile, HasErrors,
        InvalidStatement, InvalidArgumentCount, InvalidNumber, InvalidType,
        UnknownVariable, UnknownFunction, UnexpectedToken,
        ExpectedEquals, ExpectedNumber, ExpectedName, ExpectedEndWhile, ExpectedEndIf, ExpectedEndFunction, ExpectedExpression,
        EndBlock
    };

}
