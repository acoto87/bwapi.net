namespace BWAPI.NET
{
    public class Cache<T>
    {
        private int _frame = -1;
        private T _value;

        public void Set(T value, int frame)
        {
            _frame = frame;
            _value = value;
        }

        public bool Valid(int frame)
        {
            return _frame == frame;
        }

        public T Get()
        {
            return _value;
        }
    }

    public class IntegerCache : Cache<int>
    {
        public void SetOrAdd(int value, int frame)
        {
            if (Valid(frame))
            {
                Set(Get() + value, frame);
            }
            else
            {
                Set(value, frame);
            }
        }
    }

    public class BooleanCache : Cache<bool>
    {
    }

    public class OrderCache : Cache<Order>
    {
    }

    public class UnitTypeCache : Cache<UnitType>
    {
    }

    public class UpgradeTypeCache : Cache<UpgradeType>
    {
    }

    public class TechTypeCache : Cache<TechType>
    {
    }
}