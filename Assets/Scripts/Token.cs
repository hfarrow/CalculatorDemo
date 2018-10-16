using System.Collections.Generic;

namespace CalculatorDemo
{
    public enum TokenType
    {
        Number,
        Operator,
        Function,
        Variable,
        LParen,
        RParen,
        Comma,
    }

    /// <summary>
    /// A struct representing a single lexical token and it's position in the original source string.
    /// A token can have additional metadata properties added and retreived. For example, a function token
    /// might store the number of argument the function will need to pop and consume from a RPN stack.
    /// </summary>
    public struct Token
    {
        public readonly TokenType Type;
        public readonly int Index;
        public readonly int Length;
        public readonly string Source;
        private readonly Dictionary<string, object> metadata;

        public readonly string SourceSlice;
        public readonly int EndIndex;

        public bool TryGetMetadataProperty<T>(string key, out T value)
        {
            value = default(T);
            object v;
            if(metadata.TryGetValue(key, out v))
            {
                if (v is T)
                {
                    value = (T) v;
                    return true;
                }
            }
            return false;
        }
        
        public void SetMetadataProperty<T>(string key, T value)
        {
            metadata[key] = value;
        }

        public Token(TokenType type, int index, int length, string source, Dictionary<string, object> metadata = null)
        {
            Type = type;
            Index = index;
            Length = length;
            Source = source;
            this.metadata = metadata ?? new Dictionary<string, object>();

            SourceSlice = Source.Substring(Index, Length);
            EndIndex = Index + Length - 1;
        }

        public override string ToString()
        {
            return $"'{SourceSlice}' at index {Index}";
        }
    }
}