using Igor.Text;
using System.Collections.Generic;
using System.Linq;

namespace Igor.Parser
{
    public abstract class Term
    {
        public abstract bool TryMatch(IgorScanner scanner, out Token token);

        public virtual bool Test(IgorScanner scanner)
        {
            var currentPosition = scanner.CurrentPosition;
            var result = TryMatch(scanner, out var _);
            scanner.CurrentPosition = currentPosition;
            return result;
        }

        public virtual string ExpectedName => ToString();

        public static Term operator /(Term a, Term b)
        {
            if (a is OneOfTerm oneOf && !oneOf.IsSealed)
            {
                oneOf.Add(b);
                return oneOf;
            }
            else
                return new OneOfTerm(false, a, b);
        }
    }

    public class OneOfTerm : Term
    {
        private readonly List<Term> terms;

        public bool IsSealed { get; }

        public OneOfTerm(params Term[] terms) : this(true, terms)
        {
        }

        public OneOfTerm(bool seal, params Term[] terms)
        {
            this.terms = terms.ToList();
            IsSealed = seal;
        }

        public override bool Test(IgorScanner scanner) => terms.Any(t => t.Test(scanner));

        public override bool TryMatch(IgorScanner scanner, out Token token)
        {
            foreach (var term in terms)
            {
                if (term.TryMatch(scanner, out token))
                    return true;
            }
            token = Token.None;
            return false;
        }

        public void Add(Term term) => terms.Add(term);

        public override string ExpectedName => terms.JoinStrings(" or ", t => t.ExpectedName);
    }
}
