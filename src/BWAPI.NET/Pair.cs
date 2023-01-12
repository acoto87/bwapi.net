using System;

namespace BWAPI.NET
{
    public readonly struct Pair<K, V> : IEquatable<Pair<K, V>>
    {
        private readonly K _first;
        private readonly V _second;

        public Pair(K first, V second)
        {
            _first = first;
            _second = second;
        }

        public K GetFirst()
        {
            return _first;
        }

        public V GetSecond()
        {
            return _second;
        }

        public K GetLeft()
        {
            return _first;
        }

        public V GetRight()
        {
            return _second;
        }

        public K GetKey()
        {
            return _first;
        }

        public V GetValue()
        {
            return _second;
        }

        public override string ToString()
        {
            return "{" + _first + ", " + _second + "}";
        }

        public bool Equals(Pair<K, V> other)
        {
            return object.Equals(_first, other._first) && object.Equals(_second, other._second);
        }

        public override bool Equals(object o)
        {
            return o is Pair<K, V> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_first, _second);
        }
    }
}