//using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterMemory.CodeGenerator.CodeAnalysis
{
    // public MasterMemoryHintAttribute(Type elementType, params Type[] keyTypes)
    public static class HintAttributeDefinition
    {
        public const string FullName = "MasterMemory.MasterMemoryHintAttribute";

        public static INamedTypeSymbol GetElementType(AttributeData attr)
        {
            var args = attr.ConstructorArguments;
            var v = args[0].Value as INamedTypeSymbol;
            return v;
        }

        public static INamedTypeSymbol[] GetKeyTypes(AttributeData attr)
        {
            try
            {
                var args = attr.ConstructorArguments;
                var v = args[1].Values;
                return v.Select(x => x.Value).OfType<INamedTypeSymbol>().ToArray();
            }
            catch
            {
                return new[] { (INamedTypeSymbol)attr.ConstructorArguments[1].Value };
            }
        }
    }

    public class AssemblyAttributeCollector
    {
        readonly string csProjPath;
        readonly AttributeData[] hintAttributes;
        readonly INamedTypeSymbol keyTupleMarker;

        static readonly SymbolDisplayFormat enumFormat = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);

        public EnumDefinitions[] EnumDefinitions { get; private set; }
        public KeyTupleDefinitions[] KeyTupleDefinitions { get; private set; }
        public ElementDefinitions[] ElementDefinitions { get; private set; }

        public AssemblyAttributeCollector(string csProjPath, IEnumerable<string> conditinalSymbols)
        {
            this.csProjPath = csProjPath;
            var compilation = RoslynExtensions.GetCompilationFromProject(csProjPath, conditinalSymbols.ToArray()).GetAwaiter().GetResult();

            this.keyTupleMarker = compilation.GetTypeByMetadataName("MasterMemory.IMemoryKey");
            var marker = compilation.GetTypeByMetadataName(HintAttributeDefinition.FullName);

            var attributes = compilation.Assembly.GetAttributes();

            this.hintAttributes = attributes.Where(x => x.AttributeClass == marker).ToArray();
        }

        public void Visit()
        {
            // element types...
            var elementTypes = new HashSet<INamedTypeSymbol>();
            var keyTypes = new HashSet<INamedTypeSymbol>();
            var keyTupleTypes = new HashSet<KeyTupleDefinitions>();
            var deconstructedEnumTypes = new HashSet<INamedTypeSymbol>();
            foreach (var item in hintAttributes)
            {
                var et = HintAttributeDefinition.GetElementType(item);
                elementTypes.Add(et);
                if (et.TypeKind == TypeKind.Enum)
                {
                    deconstructedEnumTypes.Add(et);
                }

                foreach (var item2 in HintAttributeDefinition.GetKeyTypes(item))
                {
                    keyTypes.Add(item2);
                    if (item2.TypeKind == TypeKind.Enum)
                    {
                        deconstructedEnumTypes.Add(item2);
                    }

                    foreach (var item3 in item2.AllInterfaces)
                    {
                        if (item3 == keyTupleMarker)
                        {
                            // deconstruct keytuple
                            var keyTuple = new KeyTupleDefinitions();
                            var l = new List<string>();

                            foreach (var item4 in item2.TypeArguments)
                            {
                                l.Add(item4.ToDisplayString());
                                if (item4.TypeKind == TypeKind.Enum)
                                {
                                    deconstructedEnumTypes.Add(item4 as INamedTypeSymbol);
                                }
                            }

                            keyTuple.FullNames = l.ToArray();
                            keyTupleTypes.Add(keyTuple);
                        }
                    }
                }
            }

            this.EnumDefinitions =  deconstructedEnumTypes
                .Select(x => new EnumDefinitions
                {
                    Name = x.Name,
                    Namespace = x.ContainingNamespace.IsGlobalNamespace ? "" : x.ContainingNamespace.ToDisplayString(),
                    FullName = x.ToDisplayString(),
                    UnderlyingType = x.EnumUnderlyingType.ToDisplayString(enumFormat)
                })
                .ToArray();

            this.KeyTupleDefinitions = keyTupleTypes.ToArray();

            this.ElementDefinitions = elementTypes
                .Select(x => new ElementDefinitions
                {
                    FullName = x.ToDisplayString()
                })
                .ToArray();
        }
    }
}
