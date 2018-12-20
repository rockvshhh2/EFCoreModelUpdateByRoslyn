using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerates.Service
{
    public class SyntaxCommand
    {
        /// <summary>
        /// 建立類別
        /// </summary>
        /// <param name="modifiers">存取修飾詞</param>
        /// <param name="className">類別名稱</param>
        /// <param name="inheritances">繼承類別或實作介面</param>
        /// <returns></returns>
        public ClassDeclarationSyntax CreateClass(SyntaxKind[] modifiers, string className, string[] inheritances)
        {
            //init class
            ClassDeclarationSyntax @class = SyntaxFactory.ClassDeclaration(className);

            //Modifiers
            @class = @class.AddModifiers(
                modifiers.Select(x => SyntaxFactory.Token(x)).ToArray()
                );

            List<SyntaxNodeOrToken> bases = new List<SyntaxNodeOrToken>();

            for (int i = 0; i < inheritances.Length; i++)
            {
                if (i > 0)
                {
                    bases.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }

                bases.Add(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(inheritances[i])));
            }

            //Base
            if (bases.Count > 0)
            {
                @class = @class.AddBaseListTypes(
                SyntaxFactory.SeparatedList<BaseTypeSyntax>(bases.ToArray()).ToArray()
                );
            }

            return @class;
        }

        /// <summary>
        /// 建立屬性
        /// </summary>
        /// <param name="modifiers">存取修飾詞</param>
        /// <param name="valueType">屬性型別(實質型別)</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns></returns>
        public PropertyDeclarationSyntax CreateProperty(SyntaxKind modifiers, SyntaxKind valueType, string propertyName, bool isNullable)
        {
            PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(
                CreateType(valueType, isNullable),
                SyntaxFactory.Identifier(propertyName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(modifiers)
                        )
                )
                .WithAccessorList(
                    CreatePropertyAccessor()
                );

            return property;
        }

        /// <summary>
        /// 建立屬性
        /// </summary>
        /// <param name="modifiers">存取修飾詞</param>
        /// <param name="type">參考型別</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns></returns>
        public PropertyDeclarationSyntax CreateProperty(SyntaxKind[] modifiers, string type, string propertyName)
        {
            PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(
                CreateType(type),
                SyntaxFactory.Identifier(propertyName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(modifiers.Select(x => SyntaxFactory.Token(x)).ToArray())
                )
                .WithAccessorList(
                    CreatePropertyAccessor()
                );

            return property;
        }

        /// <summary>
        /// 建立屬性
        /// </summary>
        /// <param name="modifiers">存取修飾詞</param>
        /// <param name="genericType">泛型 型別</param>
        /// <param name="typeArgs">T1,T2.....型別</param>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns></returns>
        public PropertyDeclarationSyntax CreateProperty(SyntaxKind[] modifiers, string genericType, string[] typeArgs, string propertyName)
        {
            PropertyDeclarationSyntax property = SyntaxFactory.PropertyDeclaration(
                CreateType(genericType, typeArgs),
                SyntaxFactory.Identifier(propertyName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(modifiers.Select(x => SyntaxFactory.Token(x)).ToArray())
                )
                .WithAccessorList(
                    CreatePropertyAccessor()
                );

            return property;
        }

        /// <summary>
        /// 取得自動實作屬性的存取詞
        /// get;set;
        /// </summary>
        /// <returns></returns>
        public AccessorListSyntax CreatePropertyAccessor()
        {
            return SyntaxFactory.AccessorList(
                        SyntaxFactory.List(
                            new AccessorDeclarationSyntax[]{
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))}
                            )
                    );
        }

        /// <summary>
        /// 取得型別
        /// </summary>
        /// <param name="valueType">實質型別</param>
        /// <returns></returns>
        public TypeSyntax CreateType(SyntaxKind valueType, bool isNullable = false)
        {
            if (isNullable && valueType != SyntaxKind.StringKeyword)
            {
                return SyntaxFactory.NullableType(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(valueType)
                        )
                    );
            }
            else
            {
                return SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(valueType)
                    );
            }

        }

        /// <summary>
        /// 取得型別
        /// </summary>
        /// <param name="type">參考型別</param>
        /// <returns></returns>
        public TypeSyntax CreateType(string type)
        {
            return SyntaxFactory.IdentifierName(type);
        }

        /// <summary>
        /// 取得型別
        /// </summary>
        /// <param name="genericType">泛型 型別</param>
        /// <param name="typeArgs">T1,T2.....型別</param>
        /// <returns></returns>
        public TypeSyntax CreateType(string genericType, string[] typeArgs)
        {
            GenericNameSyntax generic = SyntaxFactory.GenericName(genericType);

            List<SyntaxNodeOrToken> types = new List<SyntaxNodeOrToken>();

            for (int i = 0; i < typeArgs.Length; i++)
            {
                if (i > 0)
                {
                    types.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }

                types.Add(SyntaxFactory.IdentifierName(typeArgs[i]));
            }

            generic = generic.AddTypeArgumentListArguments(
                SyntaxFactory.SeparatedList<TypeSyntax>(
                    types.ToArray()
                    ).ToArray()
                );

            return generic;
        }

        /// <summary>
        /// XML註解
        /// </summary>
        /// <param name="messages">多筆註解，會自動換行</param>
        /// <returns></returns>
        public SyntaxTriviaList CreateXmlSummary(string[] messages)
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();

            foreach (var msg in messages)
            {
                tokens.Add(SyntaxFactory.XmlTextNewLine(
                    SyntaxFactory.TriviaList(),
                    Environment.NewLine,
                    Environment.NewLine,
                    SyntaxFactory.TriviaList()));

                tokens.Add(SyntaxFactory.XmlTextLiteral(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.DocumentationCommentExterior("///")),
                        $" {msg}",
                        $" {msg}",
                        SyntaxFactory.TriviaList()));

                tokens.Add(SyntaxFactory.XmlTextNewLine(
                    SyntaxFactory.TriviaList(),
                    Environment.NewLine,
                    Environment.NewLine,
                    SyntaxFactory.TriviaList()));

                tokens.Add(SyntaxFactory.XmlTextLiteral(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.DocumentationCommentExterior("///")),
                    " ",
                    " ",
                    SyntaxFactory.TriviaList()));
            }

            return SyntaxFactory.TriviaList(
                SyntaxFactory.Trivia(
                        SyntaxFactory.DocumentationCommentTrivia(
                            SyntaxKind.SingleLineDocumentationCommentTrivia,
                            SyntaxFactory.List<XmlNodeSyntax>(
                                new XmlNodeSyntax[]{
                                        SyntaxFactory.XmlText()
                                        .WithTextTokens(
                                            SyntaxFactory.TokenList(
                                                SyntaxFactory.XmlTextLiteral(
                                                    SyntaxFactory.TriviaList(
                                                        SyntaxFactory.DocumentationCommentExterior("///")),
                                                    " ",
                                                    " ",
                                                    SyntaxFactory.TriviaList()))),
                                        SyntaxFactory.XmlExampleElement(
                                            SyntaxFactory.SingletonList<XmlNodeSyntax>(
                                                SyntaxFactory.XmlText()
                                                .WithTextTokens(
                                                    SyntaxFactory.TokenList(
                                                        tokens.ToArray()))))
                                        .WithStartTag(
                                            SyntaxFactory.XmlElementStartTag(
                                                SyntaxFactory.XmlName(
                                                    SyntaxFactory.Identifier("summary"))))
                                        .WithEndTag(
                                            SyntaxFactory.XmlElementEndTag(
                                                SyntaxFactory.XmlName(
                                                    SyntaxFactory.Identifier("summary")))),
                                        SyntaxFactory.XmlText()
                                        .WithTextTokens(
                                            SyntaxFactory.TokenList(
                                                SyntaxFactory.XmlTextNewLine(
                                                    SyntaxFactory.TriviaList(),
                                                    Environment.NewLine,
                                                    Environment.NewLine,
                                                    SyntaxFactory.TriviaList())))})))

                );
        }

        /// <summary>
        /// 設定XML註解
        /// </summary>
        /// <param name="property">屬性</param>
        /// <param name="messages">註解</param>
        /// <returns></returns>
        public PropertyDeclarationSyntax SetPropertyXmlSummary(PropertyDeclarationSyntax property, string[] messages)
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();
            var modifer = property.Modifiers.FirstOrDefault();

            if (modifer != null)
            {
                tokens.Add(modifer.WithLeadingTrivia(CreateXmlSummary(messages)));
            }

            tokens.AddRange(property.Modifiers.Skip(1));

            property = property.WithModifiers(SyntaxFactory.TokenList(tokens));

            return property;
        }

        /// <summary>
        /// 建立特性
        /// </summary>
        /// <param name="attrName">特性名稱</param>
        /// <returns></returns>
        public AttributeSyntax Attribute(string attrName)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Required"));
        }

        /// <summary>
        /// 最大長度的特性
        /// </summary>
        /// <param name="maxValue">最大值</param>
        /// <returns></returns>
        public AttributeSyntax MaxLengthAttribute(int maxValue)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("MaxLength"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                        new[] {
                            SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(maxValue)
                                )
                            )
                        }
                        )
                    ));
        }

        /// <summary>
        /// 最小長度的特性
        /// </summary>
        /// <param name="maxValue">最小值</param>
        /// <returns></returns>
        public AttributeSyntax MinLengthAttribute(int maxValue)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("MinLength"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                        new[] {
                            SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(maxValue)
                                )
                            )
                        }
                        )
                    ));
        }

        /// <summary>
        /// 描述的特性
        /// </summary>
        /// <param name="maxValue">描述</param>
        /// <returns></returns>
        public AttributeSyntax DescriptionAttribute(string description)
        {
            return SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Description"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList<AttributeArgumentSyntax>(
                        new[] {
                            SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(description)
                                )
                            )
                        }
                        )
                    ));
        }

        /// <summary>
        /// using命名空間
        /// </summary>
        /// <param name="fullNamespace">完整命名空間，例如 : System.Web</param>
        /// <returns></returns>
        public UsingDirectiveSyntax CreateNamespace(string fullNamespace)
        {
            string[] qualifiedNames = fullNamespace.Split(".");

            if (qualifiedNames.Length <= 1)
            {
                return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(fullNamespace));
            }

            QualifiedNameSyntax qualifiedNameSyntax = SyntaxFactory.QualifiedName(
                SyntaxFactory.IdentifierName(qualifiedNames[0]),
                SyntaxFactory.IdentifierName(qualifiedNames[1])
                );

            if (qualifiedNames.Count() > 2)
            {
                for (int i = 2; i < qualifiedNames.Count(); i++)
                {
                    qualifiedNameSyntax = SyntaxFactory.QualifiedName(
                        qualifiedNameSyntax,
                        SyntaxFactory.IdentifierName(qualifiedNames[i])
                        );
                }
            }

            return SyntaxFactory.UsingDirective(qualifiedNameSyntax);
        }
    }
}
