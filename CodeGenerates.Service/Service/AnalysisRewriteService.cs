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

        public AnalysisRewriteService(SyntaxCommand syntaxCommand)
        {
            _syntaxCommand = syntaxCommand;
        }

        public List<string> UpdateModels(string modelPath, List<DbDto> dbDtos,bool IsPlural, bool IsNeedAttributes)
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

                foreach (var nameSpaceSyntax in cs.Members.OfType<NamespaceDeclarationSyntax>())
                {
                    List<ClassDeclarationSyntax> finalClass = new List<ClassDeclarationSyntax>();

                    foreach (ClassDeclarationSyntax @class in nameSpaceSyntax.Members.OfType<ClassDeclarationSyntax>())
                    {
                        TableDto matchTable = dbDtos
                            .SelectMany(x => x.Tables)
                            .Where(x => IsPlural ? x.Name == $"{@class.Identifier.Text}s" : x.Name == @class.Identifier.Text)
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
