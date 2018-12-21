using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using CodeGenerates.Core.Dto;
using CodeGenerates.Service.Rewriter;
using Microsoft.CodeAnalysis;

namespace CodeGenerates.Service.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class AnalysisRewriteService
    {
        private readonly SyntaxCommand _syntaxCommand;
        private readonly DbSyntaxCreator _dbSyntaxCreator;

        public AnalysisRewriteService(SyntaxCommand syntaxCommand, DbSyntaxCreator dbSyntaxCreator)
        {
            _syntaxCommand = syntaxCommand;
            _dbSyntaxCreator = dbSyntaxCreator;
        }

        public List<string> UpdateModels(string modelPath, List<DbDto> dbDtos,bool IsPlural, bool IsNeedAttributes, bool IsCreateView)
        {
            List<string> modelStrings = new List<string>();

            string[] filePaths = Directory.GetFiles(modelPath);

            foreach (var file in filePaths)
            {
                string csText = "";
                using (StreamReader str = new StreamReader(file))
                {
                    csText = str.ReadToEnd();
                }

                if (string.IsNullOrEmpty(csText))
                {
                    continue;
                }

                if (!(CSharpSyntaxTree.ParseText(csText).GetRoot() is CompilationUnitSyntax cs))
                {
                    continue;
                }

                List<NamespaceDeclarationSyntax> finalMemebers = new List<NamespaceDeclarationSyntax>();

                foreach (NamespaceDeclarationSyntax nameSpaceSyntax in cs.Members.OfType<NamespaceDeclarationSyntax>())
                {
                    List<UsingDirectiveSyntax> usings = new List<UsingDirectiveSyntax> {
                        _syntaxCommand.CreateNamespace("System.ComponentModel.DataAnnotations")
                    };

                    usings.AddRange(
                        cs.Usings
                        );

                    if (usings.Count > 0)
                    {
                        cs = cs.WithUsings(SyntaxFactory.List(usings));
                    }

                    List<ClassDeclarationSyntax> finalClass = new List<ClassDeclarationSyntax>();

                    foreach (ClassDeclarationSyntax @class in nameSpaceSyntax.Members.OfType<ClassDeclarationSyntax>())
                    {
                        var isDbContext = @class.BaseList?.Types.Any(x => x.Kind() == SyntaxKind.SimpleBaseType && x.Type.ToString().Contains("DbContext"));

                        if (isDbContext != null && isDbContext.HasValue && isDbContext.Value)
                        {
                            IEnumerable<TableDto> matchViews = dbDtos
                                .SelectMany(x => x.Tables)
                                .Where(x => x.IsView);

                            List<MemberDeclarationSyntax> members = new List<MemberDeclarationSyntax>();

                            members.AddRange(@class.Members);

                            foreach (var view in matchViews)
                            {
                                string viewModelName = $"{view.Name}Model";

                                #region DB Context

                                members.Add(_dbSyntaxCreator.DbQuerySyntaxCreator(viewModelName));

                                members.Add(SyntaxFactory.PropertyDeclaration(
                                    SyntaxFactory.GenericName(SyntaxFactory.Identifier("IQueryable"))
                                    .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(viewModelName))))
                                    , SyntaxFactory.Identifier($"{view.Name}")
                                    )
                                    .WithModifiers(SyntaxFactory.TokenList(
                                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                                        ))
                                    .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ThisExpression(),
                                                    SyntaxFactory.IdentifierName(viewModelName)),
                                                SyntaxFactory.IdentifierName("FromSql")))
                                                .WithArgumentList(SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal($"SELECT * FROM {view.Name}")
                                                            ))
                                                        ))
                                                    )
                                        ))
                                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                );

                                #endregion

                                #region Entity

                                CompilationUnitSyntax viewCsFile = SyntaxFactory.CompilationUnit();

                                viewCsFile = viewCsFile.WithUsings(SyntaxFactory.List(usings));

                                ClassDeclarationSyntax viewClass = _dbSyntaxCreator.EntitySyntaxCreator(view);

                                var viewNamespace = nameSpaceSyntax.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(
                                    new MemberDeclarationSyntax[] {
                                        viewClass
                                    }
                                    ));

                                viewCsFile = viewCsFile.WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(
                                    new MemberDeclarationSyntax[] {
                                        viewNamespace
                                    }
                                    ));

                                File.WriteAllText($"{modelPath}\\{view.Name}.cs", viewCsFile.NormalizeWhitespace().ToString());

                                #endregion
                            }

                            finalClass.Add(@class.WithMembers(
                                SyntaxFactory.List(members)
                                ));
                        }
                        else
                        {
                            TableDto matchTable = dbDtos
                                .SelectMany(x => x.Tables)
                                .Where(x => !x.IsView && IsPlural ? x.Name == $"{@class.Identifier.Text}s" : x.Name == @class.Identifier.Text)
                                .FirstOrDefault();

                            if (matchTable == null)
                            {
                                finalClass.Add(@class);
                                continue;
                            }

                            if (!matchTable.IsNeed)
                            {
                                finalClass.Add(@class);
                                continue;
                            }

                            matchTable.IsNeedAttributes = IsNeedAttributes;
                            EntityColumRewriter rewriter = new EntityColumRewriter(matchTable, new SyntaxCommand());

                            finalClass.Add(rewriter.Visit(@class) as ClassDeclarationSyntax);
                        }
                    }                    

                    if (finalClass.Count > 0)
                    {
                        finalMemebers.Add(nameSpaceSyntax.WithMembers(SyntaxFactory.List(
                        finalClass.OfType<MemberDeclarationSyntax>()
                        )));
                    }
                }

                if (finalMemebers.Count > 0)
                {
                    cs = cs.WithMembers(SyntaxFactory.List(
                       finalMemebers.OfType<MemberDeclarationSyntax>()
                       ));

                    string fullText = cs.NormalizeWhitespace().ToString();

                    modelStrings.Add(fullText);

                    File.WriteAllText(file, fullText);
                }
            }

            return modelStrings;
        }
    }
}
