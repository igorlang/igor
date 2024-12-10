using System;
using System.Collections;
using System.Collections.Generic;

namespace Igor.Parser
{
    // Implement IEnumerable to allow collection initializer
    public class SetOfChars : IEnumerable<char>
    {
        private readonly BitArray chars = new BitArray(128, false);

        public void Add(char c)
        {
            int code = c;
            if (code >= 128)
                throw new ArgumentOutOfRangeException(nameof(c));
            chars[code] = true;
        }

        public SetOfChars()
        {
        }

        public SetOfChars(Predicate<char> predicate)
        {
            for (int i = 0; i < 128; i++)
            {
                var c = (char)i;
                chars[i] = predicate(c);
            }
        }

        public bool Test(char c)
        {
            int code = c;
            if (code >= 128)
                return false;
            return chars[code];
        }

        public IEnumerator<char> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
