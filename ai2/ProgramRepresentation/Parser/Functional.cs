using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ProgramRepresentation.Parser
{
    sealed class Functional
    {
        public abstract class ParseTreeElement
        {
            public enum EnumType
            {
                SCOPE, // round braces
                       // nameOfOperation is the name of the operation
                ARRAY, // [ ] braces
                NUMBER,
                IDENTIFIER, // starts with #
                STRING,
                FLOAT,
                BOOLEAN
            }

            public EnumType type;
            

            public List<ParseTreeElement> childrens = new List<ParseTreeElement>();

            public ParseTreeElement(ParseTreeElement.EnumType type)
            {
                this.type = type;
            }
        }

        public sealed class ScopeParseTreeElement : ParseTreeElement
        {
            public enum EnumType
            {
                ADD,
                SUB,
                MUL,
                DIV,
                
                /*
                PARALLEL,
                SERIAL,
                 */
                FOLD,
                INVOKE,
                MATCH,

                FOREACH,
                PASS,
                FSET,
            }

            public EnumType scopeType;

            public ScopeParseTreeElement() : base(ParseTreeElement.EnumType.SCOPE)
            {
            }
        }

        public sealed class ArrayParseTreeElement : ParseTreeElement
        {
            public ArrayParseTreeElement() : base(ParseTreeElement.EnumType.ARRAY)
            {
            }
        }

        public sealed class NumberParseTreeElement : ParseTreeElement
        {
            public enum EnumNumberType
            {
                INTEGER,
                FLOAT
            }

            public EnumNumberType numberType;
            public int valueInt;
            public float valueFloat;

            public NumberParseTreeElement(EnumNumberType numberType) : base(ParseTreeElement.EnumType.NUMBER)
            {
                this.numberType = numberType;
            }

            public static NumberParseTreeElement createInteger(int integer)
            {
                NumberParseTreeElement resultTreeElement = new NumberParseTreeElement(EnumNumberType.INTEGER);
                resultTreeElement.valueInt = integer;

                return resultTreeElement;
            }

            public static NumberParseTreeElement createFloat(float value)
            {
                NumberParseTreeElement resultTreeElement = new NumberParseTreeElement(EnumNumberType.FLOAT);
                resultTreeElement.valueFloat = value;

                return resultTreeElement;
            }
        }

        public sealed class IdentifierParseTreeElement : ParseTreeElement
        {
            public string identifier;

            public IdentifierParseTreeElement(string identifier) : base(ParseTreeElement.EnumType.IDENTIFIER)
            {
                this.identifier = identifier;
            }
        }

        public sealed class StringParseTreeElement : ParseTreeElement
        {
            public string content;

            public StringParseTreeElement(string content) : base(ParseTreeElement.EnumType.STRING)
            {
                this.content = content;
            }
        }


        public sealed class BooleanParseTreeElement : ParseTreeElement
        {
            public enum EnumBooleanType
            {
                FALSE,
                TRUE
            }

            public EnumBooleanType booleanType;

            public BooleanParseTreeElement(EnumBooleanType booleanType)
                : base(ParseTreeElement.EnumType.BOOLEAN)
            {
                this.booleanType = booleanType;
            }
        }

        static public ParseTreeElement parseRecursive(string text)
        {
            text = text.Trim();

            if( text.Length == 0 )
            {
                throw new Exception("Void string encountered!");
            }

            if( text[0] == '(' && text[text.Length-1] == ')' )
            {
                return parseScope(text.Substring(1, text.Length - 2));
            }
            else if( text[0] == '[' && text[text.Length - 1] == ']' )
            {
                return parseArray(text.Substring(1, text.Length - 2));
            }
            else if( isFloat(text) )
            {
                return parseFloat(text);
            }
            else if( isInteger(text) )
            {
                return parseInteger(text);
            }
            else if( text[0] == '#' )
            {
                return parseIdentifier(text.Substring(1));
            }
            else if( text[0] == '"' && text[text.Length - 1] == '"' )
            {
                return parseString(text);
            }
            else if( text == "true" || text == "false" )
            {
                return parseBoolean(text);
            }

            // if we are here nothing matched
            throw new Exception("nothing matched!");
        }

        static private ParseTreeElement parseFloat(string text)
        {
            Regex regexFloat = new Regex("^(?<value>-?\\d+\\.\\d+)");
            Match match;
            float value;

            match = regexFloat.Match(text);
            System.Diagnostics.Debug.Assert(match.Success);
            float.TryParse(match.Groups["value"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

            return NumberParseTreeElement.createFloat(value);
        }

        static private ParseTreeElement parseString(string text)
        {
            string innerContent;

            if( text[0] != '"' || text[text.Length - 1] != '"' )
            {
                throw new Exception("invalid string");
            }

            innerContent = text.Substring(1, text.Length - 2);

            if( innerContent.IndexOf('"') != -1 )
            {
                throw new Exception("invalid string");
            }

            return new StringParseTreeElement(innerContent);
        }

        /**
         * identifer text is the identifier without # and spaces before or after
         * 
         */
        static private ParseTreeElement parseIdentifier(string text)
        {
            if( !isValidIdentifier(text) )
            {
                throw new Exception("invalid identifer!");
            }

            return new IdentifierParseTreeElement(text);
        }

        static private ParseTreeElement parseInteger(string text)
        {
            return NumberParseTreeElement.createInteger(Convert.ToInt32(text));
        }

        static private ParseTreeElement parseArray(string innerText)
        {
            ArrayParseTreeElement resultTreeElement;
            List<string> splitedTokenStrings;
            List<ParseTreeElement> splitedTokens;

            innerText = innerText.Trim();

            resultTreeElement = new ArrayParseTreeElement();
            splitedTokens = new List<ParseTreeElement>();

            splitedTokenStrings = splitAfterTokens(innerText);
            foreach (string iterationTokenString in splitedTokenStrings)
            {
                splitedTokens.Add(parseRecursive(iterationTokenString));
            }

            resultTreeElement.childrens = splitedTokens;

            return resultTreeElement;
        }

        static private ParseTreeElement parseBoolean(string text)
        {
            if( text == "true" )
            {
                return new BooleanParseTreeElement(BooleanParseTreeElement.EnumBooleanType.TRUE);
            }
            else if( text == "false" )
            {
                return new BooleanParseTreeElement(BooleanParseTreeElement.EnumBooleanType.FALSE);
            }
            else
            {
                throw new Exception("Invalid boolean string!");
            }
        }

        static private ParseTreeElement parseScope(string innerText)
        {
            int indexOfFirstPairOrSpace;

            string stringBeforeFirstPair;
            string operationAsString;
            string stringAfterOperation;

            ScopeParseTreeElement resultTreeElement;
            List<string> splitedTokenStringsAfterOperation;
            List<ParseTreeElement> splitedTokensAfterOperation;

            innerText = innerText.Trim();

            indexOfFirstPairOrSpace = findPositionOfFirstPairOrSpace(innerText);

            if( indexOfFirstPairOrSpace == -1 )
            {
                indexOfFirstPairOrSpace = 0;
            }
            

            resultTreeElement = new ScopeParseTreeElement();
            splitedTokensAfterOperation = new List<ParseTreeElement>();

            stringBeforeFirstPair = innerText.Substring(0, indexOfFirstPairOrSpace);
            stringBeforeFirstPair = stringBeforeFirstPair.Trim();
            operationAsString = stringBeforeFirstPair;

            resultTreeElement.scopeType = translateOperationToOperationType(operationAsString);

            // length correct?
            stringAfterOperation = innerText.Substring(indexOfFirstPairOrSpace, innerText.Length - indexOfFirstPairOrSpace);

            splitedTokenStringsAfterOperation = splitAfterTokens(stringAfterOperation);
            foreach( string iterationTokenString in splitedTokenStringsAfterOperation )
            {
                splitedTokensAfterOperation.Add(parseRecursive(iterationTokenString));
            }

            resultTreeElement.childrens = splitedTokensAfterOperation;

            return resultTreeElement;
        }

        /**
         * splits string into strings with single elements, which are
         * * lists
         * * parentesis
         * * identifiers/numbers
         * * named variables
         * 
         */
        static private List<string> splitAfterTokens(string text)
        {
            int indexOfClosing;
            int currentIndex;
            List<string> result;
            Regex regexFloat = new Regex("^(?<value>-?\\d+\\.\\d+)");

            text = text.Trim();

            result = new List<string>();
            currentIndex = 0;

            for(;;)
            {
                Match floatMatch;

                // search for non space
                for(;;)
                {
                    if( currentIndex >= text.Length )
                    {
                        return result;
                    }

                    if( text[currentIndex] != ' ' )
                    {
                        break;
                    }

                    currentIndex++;
                }

                System.Diagnostics.Debug.Assert(currentIndex < text.Length);

                floatMatch = regexFloat.Match(text.Substring(currentIndex, text.Length-currentIndex));

                if (text[currentIndex] == '[')
                {
                    indexOfClosing = parsingSearchIndexOfClosing('[', text, currentIndex + 1);

                    if (indexOfClosing == -1)
                    {
                        throw new Exception("unbalanced braces");
                    }

                    indexOfClosing++;

                    result.Add(text.Substring(currentIndex, indexOfClosing-currentIndex));

                    currentIndex = indexOfClosing;
                }
                else if (text[currentIndex] == '(')
                {
                    string substring;

                    indexOfClosing = parsingSearchIndexOfClosing('(', text, currentIndex + 1);

                    if (indexOfClosing == -1)
                    {
                        throw new Exception("unbalanced braces");
                    }

                    indexOfClosing++;

                    substring = text.Substring(currentIndex, indexOfClosing-currentIndex);
                    result.Add(substring);

                    currentIndex = indexOfClosing;
                }
                else if( text[currentIndex] == '"' )
                {
                    // TODO< parse also escapes >

                    indexOfClosing = text.IndexOf('"', currentIndex+1);

                    if (indexOfClosing == -1)
                    {
                        throw new Exception("unclosed string");
                    }

                    indexOfClosing++;

                    result.Add(text.Substring(currentIndex, indexOfClosing - currentIndex));

                    currentIndex = indexOfClosing;
                }
                // case for integer and/or identifier
                else if( isInteger(text[currentIndex].ToString()) || text[currentIndex] == '#' )
                {
                    int indexOfEnd;
                    string substring;

                    indexOfEnd = findIndexOfEndOfIdentifierOrNumber(text, currentIndex);

                    System.Diagnostics.Debug.Assert(indexOfEnd > currentIndex);
                    substring = text.Substring(currentIndex, indexOfEnd-currentIndex);
                    result.Add(substring);

                    currentIndex = indexOfEnd+1;
                }
                else if( floatMatch.Success )
                {
                    string valueAsString;
                    
                    valueAsString = floatMatch.Groups["value"].Value;

                    result.Add(valueAsString);
                    currentIndex = currentIndex + valueAsString.Length;
                }
                else if( currentIndex < text.Length - 5 && text.Substring(currentIndex, 5) == "true " )
                {
                    result.Add("true");
                    currentIndex += 5;
                }    
                else if( currentIndex < text.Length - 6 && text.Substring(currentIndex, 6) == "false " )
                {
                    result.Add("false");
                    currentIndex += 5;
                }
                else
                {
                    char c = text[currentIndex];

                    throw new Exception("internal error");
                }
            }
        }

        static private int parsingSearchIndexOfClosing(char openingInput, string text, int startIndexForSearch)
        {
            int i;
            int currentLevel;
            char opening, closing;

            System.Diagnostics.Debug.Assert(openingInput == '(' || openingInput == '[');

            currentLevel = 1;

            opening = openingInput;

            if (openingInput == '(' )
            {
                closing = ')';
            }
            else if (openingInput == '[')
            {
                closing = ']';
            }
            else
            {
                // UNREACHABLE should be unreachable
                return -1;
            }

            for( i = startIndexForSearch; i < text.Length; i++ )
            {
                if( text[i] == opening )
                {
                    currentLevel++;
                }
                else if( text[i] == closing )
                {
                    currentLevel--;
                }

                if( currentLevel == 0 )
                {
                    break;
                }
            }

            if (currentLevel == 0)
            {
                return i;
            }
            else
            {
                return -1;
            }
        }

        static private ScopeParseTreeElement.EnumType translateOperationToOperationType(string operationAsString)
        {
            if( operationAsString == "invoke" )
            {
                return ScopeParseTreeElement.EnumType.INVOKE;
            }
            else if( operationAsString == "fold" )
            {
                return ScopeParseTreeElement.EnumType.FOLD;
            }
            else if (operationAsString == "match")
            {
                return ScopeParseTreeElement.EnumType.MATCH;
            }
            else if (operationAsString == "foreach")
            {
                return ScopeParseTreeElement.EnumType.FOREACH;
            }
            else if (operationAsString == "pass")
            {
                return ScopeParseTreeElement.EnumType.PASS;
            }
            else if( operationAsString == "set" )
            {
                return ScopeParseTreeElement.EnumType.FSET;
            }
            else if (operationAsString == "+")
            {
                return ScopeParseTreeElement.EnumType.ADD;
            }
            else if (operationAsString == "-")
            {
                return ScopeParseTreeElement.EnumType.SUB;
            }
            else if (operationAsString == "*")
            {
                return ScopeParseTreeElement.EnumType.MUL;
            }
            else if (operationAsString == "/")
            {
                return ScopeParseTreeElement.EnumType.DIV;
            }
            else
            {
                throw new Exception("invalid operation!");
            }
        }

        static private int findIndexOfEndOfIdentifierOrNumber(string text, int startIndex)
        {
            int i;

            for( i = startIndex; i < text.Length; i++ )
            {
                if( isInteger(text[i].ToString()) )
                {
                    continue;
                }
                else if( Misc.Parsing.isLetter(text[i]) )
                {
                    continue;
                }
                else if( text[i] == '#' || text[i] == '.' )
                {
                    continue;
                }

                // if we are here it didn't find any "continuation" symbol
                // so we return the index
                return i;
            }

            // not found, return index so that substring is the complete remaining text
            return text.Length;
        }

        static private int findPositionOfFirstPairOrSpace(string text)
        {
            return text.IndexOfAny(new char[]{'[', '(', ' '});
        }

        
        static private bool isValidIdentifier(string text)
        {
            int i;

            if( text.Length == 0 )
            {
                return false;
            }

            // identifier can't start with numbers
            if( isInteger(text[0].ToString()) )
            {
                return false;
            }

            // the following signs must be letters or numbers or _
            for (i = 1; i < text.Length; i++ )
            {
                char currentChar;

                currentChar = text[i];

                if(
                    isInteger(currentChar.ToString()) ||
                    Misc.Parsing.isLetter(currentChar.ToString().ToLower()[0]) ||
                    currentChar == '_'
                )
                {
                    continue;
                }

                // if we are here there is a invalid sign
                return false;
            }

            return true;
        }

        static private bool isInteger(string text)
        {
            foreach( char iterationChar in text )
            {
                if( iterationChar.ToString().IndexOfAny(new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'}) == -1 )
                {
                    return false;
                }
            }

            return true;
        }

        static private bool isFloat(string text)
        {
            Regex regexFloat = new Regex("^(?<value>-?\\d+\\.\\d+)");
            Match match;

            match = regexFloat.Match(text);
            return match.Success;
        }

        
    }
}
