using CodeGenerates.Core.Dto;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CodeGenerates.Service.Rewriter
{
    public class EntityColumRewriter : CSharpSyntaxRewriter
    {
        private readonly TableDto _table;
        private readonly SyntaxCommand _syntaxCommand;

        public EntityColumRewriter(TableDto table, SyntaxCommand syntaxCommand) : base()            
        {
            _table = table;
            _syntaxCommand = syntaxCommand;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var matchColum = _table.Columns.FirstOrDefault(x => x.Name == node.Identifier.Text);

            if (matchColum == null)
            {
                return base.VisitPropertyDeclaration(node);
            }

            #region Xml Summary

            List<string> xmlSummary = new List<string>();

            if (!string.IsNullOrEmpty(matchColum.Description))
            {
                xmlSummary.Add(matchColum.Description);
            }

            if (matchColum.Length.HasValue && matchColum.Length.Value != 0)
            {
                xmlSummary.Add($"Length : {(matchColum.Length.Value != -1 ? matchColum.Length.Value.ToString() : "Max")}");
            }

            if (!string.IsNullOrEmpty(matchColum.Collation))
            {
                xmlSummary.Add($"collation : {matchColum.Collation}");
            }

            node = _syntaxCommand.SetPropertyXmlSummary(node, xmlSummary.ToArray());

            #endregion

            #region Attribute

            if (_table.IsNeedAttributes)
            {
                List<AttributeSyntax> attributes = new List<AttributeSyntax>();

                if (!matchColum.IsNull)
                {
                    attributes.Add(_syntaxCommand.Attribute("Required"));
                }

                if (matchColum.Length.HasValue && matchColum.Length.Value > 0)
                {
                    attributes.Add(_syntaxCommand.MaxLengthAttribute(matchColum.Length.Value));
                }

                if (!string.IsNullOrEmpty(matchColum.Description))
                {
                    attributes.Add(_syntaxCommand.DescriptionAttribute(matchColum.Description));
                }

                if (attributes.Count > 0)
                {
                    node = node.WithAttributeLists(SyntaxFactory.List(
                    new AttributeListSyntax[] {
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SeparatedList(attributes))
                    }));
                }
            }

            #endregion

            return base.VisitPropertyDeclaration(node);
        }
    }
}
