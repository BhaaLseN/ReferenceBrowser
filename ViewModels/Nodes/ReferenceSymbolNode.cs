using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ReferenceBrowser.ViewModels.Nodes
{
    public class ReferenceSymbolNode : NodeBase
    {
        public ISymbol Symbol { get; }
        public IEnumerable<ReferencedSymbol> ReferenceSymbols { get; }

        public ReferenceSymbolNode(ISymbol symbol, IEnumerable<ReferencedSymbol> referenceSymbols)
            : base($"{symbol?.Name} ({symbol?.Kind}, {referenceSymbols?.Sum(r => r.Locations.Count())} References)")
        {
            Symbol = symbol;
            ReferenceSymbols = referenceSymbols;
        }
    }
}
