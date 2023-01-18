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

        public readonly K First
        {
            get => _first;
        }

        public readonly V Second
        {
            get => _second;
        }

        public readonly K Left
        {
            get => _first;
        }

        public readonly V Right
        {
            get => _second;
        }

        public readonly K Key
        {
            get => _first;
        }

        public readonly V Value
        {
            get => _second;
        }

        public readonly K GetFirst()
        {
            return _first;
        }

        public readonly V GetSecond()
        {
            return _second;
        }

        public readonly K GetLeft()
        {
            return _first;
        }

        public readonly V GetRight()
        {
            return _second;
        }

        public readonly K GetKey()
        {
            return _first;
        }

        public readonly V GetValue()
        {
            return _second;
        }

        public readonly override string ToString()
        {
            return "{" + _first + ", " + _second + "}";
        }

        public readonly bool Equals(Pair<K, V> other)
        {
            return Equals(_first, other._first) && object.Equals(_second, other._second);
        }

        public readonly override bool Equals(object o)
        {
            return o is Pair<K, V> other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(_first, _second);
        }

        public readonly void Deconstruct(out K first, out V second)
        {
            first = _first;
            second = _second;
        }
    }
}