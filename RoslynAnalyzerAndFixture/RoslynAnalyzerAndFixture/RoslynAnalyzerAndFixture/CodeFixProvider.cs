using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace RoslynAnalyzerAndFixture
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RoslynAnalyzerAndFixtureCodeFixProvider)), Shared]
    public class RoslynAnalyzerAndFixtureCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Lokalisiere string";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(RoslynAnalyzerAndFixtureAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<LiteralExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => LocalizeString(context.Document, declaration, c),
                    Title),
                diagnostic);

        }

        private async Task<Document> LocalizeString(Document document, LiteralExpressionSyntax literalExpressionSyntax, CancellationToken cancellationToken)
        {
            // get current syntax root
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);

            // find node from expected string literal.
            var currentNode = syntaxRoot.FindNode(literalExpressionSyntax.Span);

            // get the syntax generator of the current document !!!
            var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

            // create the identifier of the type which has to be created.
            var identifierName = SyntaxFactory.IdentifierName("ConsoleApp1.TranslatableText");

            // create argument syntax
            var argument = SyntaxFactory.Argument(literalExpressionSyntax);

            // create the object creation expression.
            var objectCreationExpressionSyntax = syntaxGenerator.ObjectCreationExpression(identifierName, argument);

            // create a new syntax root.
            var newRoot = syntaxGenerator.ReplaceNode(syntaxRoot, currentNode, objectCreationExpressionSyntax);

            // return the document with the new root.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}