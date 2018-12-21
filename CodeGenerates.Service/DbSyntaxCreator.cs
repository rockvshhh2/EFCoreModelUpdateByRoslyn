using CodeGenerates.Core.Dto;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerates.Service
{
    public class DbSyntaxCreator
    {
        private readonly SyntaxCommand _syntaxCommand;
        private readonly Dictionary<string, SyntaxKind> ColumDataType;
        private readonly Dictionary<string, string> ColumOthersDataType;

        public DbSyntaxCreator()
        {
            _syntaxCommand = new SyntaxCommand();
            ColumDataType = new Dictionary<string, SyntaxKind>
            {
                { "nvarchar", SyntaxKind.StringKeyword },
                { "varchar", SyntaxKind.StringKeyword },
                { "char", SyntaxKind.StringKeyword },
                { "int", SyntaxKind.IntKeyword },
                { "bigint", SyntaxKind.IntKeyword },
                { "numeric", SyntaxKind.DecimalKeyword },
                { "decimal", SyntaxKind.DecimalKeyword },
                { "float", SyntaxKind.FloatKeyword }
            };

            ColumOthersDataType = new Dictionary<string, string> {
                { "date", nameof(DateTime) },
                { "datetime", nameof(DateTime) },
                { "datetime2", nameof(DateTime) }
            };
        }

        /// <summary>
        /// 建立空的Db Context
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public ClassDeclarationSyntax EmptyDbContextSyntaxCreator(string dbName)
        {
            return _syntaxCommand.CreateClass(new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword }, $"{dbName}DbContext", new string[] { "DbContext" });
        }

        public ClassDeclarationSyntax DbContextSyntaxCreator(DbDto db)
        {
            ClassDeclarationSyntax dbContext = _syntaxCommand.CreateClass(new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword }, $"{db.Name}DbContext", new string[] { "DbContext" });

            foreach (var table in db.Tables)
            {
                dbContext = dbContext.AddMembers(DbSetSyntaxCreator(table.Name));
            }

            //dbContext = dbContext.AddMembers(
            //    DbContextOnConfiguringUseSqlServer(db.ConnectionString)
            //    );

            return dbContext;
        }

        public ClassDeclarationSyntax EntitySyntaxCreator(TableDto table)
        {
            ClassDeclarationSyntax entuty = _syntaxCommand.CreateClass(new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword }, table.Name, new string[] { });

            foreach (var col in table.Columns)
            {
                var keyWord = ColumDataType.FirstOrDefault(x => x.Key == col.DataType.ToLower());
                var otherType = ColumOthersDataType.FirstOrDefault(x => x.Key == col.DataType.ToLower());

                PropertyDeclarationSyntax property;

                if (keyWord.Key != null)
                {
                    property = _syntaxCommand.CreateProperty(SyntaxKind.PublicKeyword, keyWord.Value, col.Name, col.IsNull);
                }
                else if (otherType.Key != null)
                {
                    property = _syntaxCommand.CreateProperty(new SyntaxKind[] { SyntaxKind.PublicKeyword }, otherType.Value, col.Name);
                }
                else
                {
                    continue;
                }

                List<string> xmlSummary = new List<string>();

                if (!string.IsNullOrEmpty(col.Description))
                {
                    xmlSummary.Add(col.Description);
                }

                if (col.Length.HasValue && col.Length.Value != 0)
                {
                    xmlSummary.Add($"Length : {(col.Length.Value != -1 ? col.Length.Value.ToString() : "Max")}");
                }

                if (!string.IsNullOrEmpty(col.Collation))
                {
                    xmlSummary.Add($"collation : {col.Collation}");
                }

                property = _syntaxCommand.SetPropertyXmlSummary(property, xmlSummary.ToArray());

                List<AttributeSyntax> attributes = new List<AttributeSyntax>();

                if (!col.IsNull)
                {
                    attributes.Add(_syntaxCommand.Attribute("Required"));
                }

                if (col.Length.HasValue && col.Length.Value > 0)
                {
                    attributes.Add(_syntaxCommand.MaxLengthAttribute(col.Length.Value));
                }

                if (attributes.Count > 0)
                {
                    property = property.WithAttributeLists(SyntaxFactory.List(
                    new AttributeListSyntax[] {
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SeparatedList(attributes))
                    }));
                }

                entuty = entuty.AddMembers(property);
            }

            return entuty;
        }

        public MemberDeclarationSyntax DbSetSyntaxCreator(string tableName)
        {
            return _syntaxCommand.CreateProperty(new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.VirtualKeyword }, "DbSet", new string[] { tableName }, $"{tableName}s");
        }

        public MemberDeclarationSyntax DbQuerySyntaxCreator(string tableName)
        {
            return _syntaxCommand.CreateProperty(new SyntaxKind[] { SyntaxKind.PublicKeyword, SyntaxKind.VirtualKeyword }, "DbQuery", new string[] { tableName }, $"{tableName}s");
        }

        private MemberDeclarationSyntax DbContextOnConfiguringUseSqlServer(string connectionString)
        {
            //init Method
            MethodDeclarationSyntax methodDeclaration = SyntaxFactory.MethodDeclaration(
                    _syntaxCommand.CreateType(SyntaxKind.VoidKeyword)
                    , SyntaxFactory.Identifier("OnConfiguring")
                );

            //Modifiers
            methodDeclaration = methodDeclaration.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ProtectedKeyword)
                , SyntaxFactory.Token(SyntaxKind.OverrideKeyword)
                );

            //Parameter
            methodDeclaration = methodDeclaration.AddParameterListParameters(
                SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier("optionsBuilder")
                    ).WithType(
                    SyntaxFactory.IdentifierName("DbContextOptionsBuilder")
                    )
                );

            //Body
            methodDeclaration = methodDeclaration.AddBodyStatements(
                SyntaxFactory.IfStatement(
                    SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression
                        , SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression
                            , SyntaxFactory.IdentifierName("optionsBuilder")
                            , SyntaxFactory.IdentifierName("IsConfigured")
                            )
                        )
                    , SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression
                                    , SyntaxFactory.IdentifierName("optionsBuilder")
                                    , SyntaxFactory.IdentifierName("UseSqlServer")
                                    )
                                ).AddArgumentListArguments(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression
                                            , SyntaxFactory.Literal(
                                                $"@\"{connectionString}\""
                                                , connectionString)
                                            )
                                        )
                                )
                            )
                        )
                    )
                );

            return methodDeclaration;
        }        
    }
}
