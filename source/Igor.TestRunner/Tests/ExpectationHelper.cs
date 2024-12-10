namespace Igor.TestRunner.Tests
{
    public static class Expectations
    {
        public static MatchExpectation Match(this Expression left, Expression right) => new MatchExpectation(left, right);

        public static ErrorExpectation Error(int line, string spec) => new ErrorExpectation(line, spec);

        public static BoolExpression Bool(bool value) => new BoolExpression(value);

        public static FloatExpression Float(double value) => new FloatExpression(value);

        public static IntegerExpression Int(int value) => new IntegerExpression(value);

        public static StringExpression String(string value) => new StringExpression(value);

        public static NameExpression Name(string value, int line = 0) => new NameExpression(line, value);

        public static AccessExpression Access(this Expression host, string property) => new AccessExpression(host, new PropertyAccessor(property));

        public static AccessExpression Access(this Expression host, int index) => new AccessExpression(host, new IndexAccessor(index));
    }
}
