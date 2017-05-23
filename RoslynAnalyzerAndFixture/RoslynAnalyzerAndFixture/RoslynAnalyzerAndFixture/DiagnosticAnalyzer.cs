using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RoslynAnalyzerAndFixture
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RoslynAnalyzerAndFixtureAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "FindStringToLocalize";
        private static readonly LocalizableString Title = "Strings müssen lokalisiert werden";
        private static readonly LocalizableString MessageFormat = "Bitte lokalisieren sie '{0}'";
        private static readonly LocalizableString Description = "Alle strings müssen lokalisiert werden !!!";
        private const string Category = "Lokalisierung";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.StringLiteralExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = (LiteralExpressionSyntax)context.Node;
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), node.ToString()));
        }
    }
}
