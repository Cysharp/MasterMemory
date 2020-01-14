using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace MasterMemory.GeneratorCore
{

    public class GenerationContext
    {
        public string ClassName { get; set; }
        public string MemoryTableName { get; set; }
        public string[] UsingStrings { get; set; }
        public PrimaryKey PrimaryKey { get; set; }
        public SecondaryKey[] SecondaryKeys { get; set; }

        public string InputFilePath { get; set; }
        public ClassDeclarationSyntax OriginalClassDeclaration { get; set; }

        public Property[] Properties { get; set; }
        public KeyBase[] Keys => new KeyBase[] { PrimaryKey }.Concat(SecondaryKeys).ToArray();

    }

    public class Property
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public abstract class KeyBase
    {
        public bool IsNonUnique { get; set; }
        public string StringComparisonOption { get; set; }
        public KeyProperty[] Properties { get; set; }
        public abstract string SelectorName { get; }
        public abstract string TableName { get; }
        public abstract bool IsPrimary { get; }

        public string BuildKeyAccessor(string lambdaArgument)
        {
            if (Properties.Length == 1)
            {
                return lambdaArgument + "." + Properties[0].Name;
            }
            else
            {
                return "(" + string.Join(", ", Properties.Select(x => lambdaArgument + "." + x.Name)) + ")";
            }
        }

        public string BuildTypeName()
        {
            if (Properties.Length == 1)
            {
                return Properties[0].TypeName;
            }
            else
            {
                return "(" + string.Join(", ", Properties.Select(x => x.TypeName + " " + x.Name)) + ")";
            }
        }

        public string BuildMethodName()
        {
            if (Properties.Length == 1)
            {
                return Properties[0].Name;
            }
            else
            {
                return string.Join("And", Properties.Select(x => x.Name));
            }
        }

        public string BuildPropertyTupleName()
        {
            if (Properties.Length == 1)
            {
                return Properties[0].Name;
            }
            else
            {
                return "(" + string.Join(", ", Properties.Select(x => x.Name)) + ")";
            }
        }

        public string BuildFindPrefix()
        {
            return IsNonUnique ? "FindMany" : "FindUnique";
        }

        public string BuildReturnTypeName(string elementName)
        {
            return IsNonUnique ? "RangeView<" + elementName + ">" : elementName;
        }

        public string BuildComparer()
        {
            if (!IsStringType)
            {
                return $"System.Collections.Generic.Comparer<{BuildTypeName()}>.Default";
            }
            else
            {
                if (StringComparisonOption != null)
                {
                    return "System.StringComparer." + StringComparisonOption.Split('.').Last();
                }
                else
                {
                    return "System.StringComparer.Ordinal";
                }
            }
        }

        public bool IsIntType
        {
            get
            {
                if (Properties.Length == 1)
                {
                    var typeName = Properties[0].TypeName;
                    if (typeName == "int" || typeName == "Int32" || typeName == "System.Int32")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsStringType
        {
            get
            {
                if (Properties.Length == 1)
                {
                    var typeName = Properties[0].TypeName;
                    if (typeName == "string" || typeName == "String" || typeName == "System.String")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsComparableNumberType
        {
            get
            {
                if (Properties.Length == 1)
                {
                    var typeName = Properties[0].TypeName;
                    if (typeName == "int" || typeName == "Int32" || typeName == "System.Int32"
                     || typeName == "long" || typeName == "Int64" || typeName == "System.Int64"
                     || typeName == "uint" || typeName == "UInt32" || typeName == "System.UInt32"
                     || typeName == "ulong" || typeName == "UInt64" || typeName == "System.UInt64"
                     || typeName == "byte" || typeName == "Byte" || typeName == "System.Byte"
                     || typeName == "sbyte" || typeName == "SByte" || typeName == "System.SByte"
                     )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanInlineBinarySearch
        {
            get
            {
                return (this is PrimaryKey) && (IsComparableNumberType) && !IsNonUnique;
            }
        }
    }

    public class PrimaryKey : KeyBase
    {
        public override string SelectorName => "primaryIndexSelector";
        public override string TableName => "data";
        public override bool IsPrimary => true;
    }

    public class SecondaryKey : KeyBase
    {
        public int IndexNo { get; set; }
        public override string SelectorName => $"secondaryIndex{IndexNo}Selector";
        public override string TableName => $"secondaryIndex{IndexNo}";
        public override bool IsPrimary => false;
    }

    public class KeyProperty
    {
        public int KeyOrder { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

        public override string ToString()
        {
            return $"{TypeName} {Name} : {KeyOrder}";
        }
    }
}
