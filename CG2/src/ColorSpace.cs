namespace ColorReplacer
{
    public delegate double ComparisonAlgorithm(IColorSpace a, IColorSpace b);

    public interface IColorSpace
    {
        void Initialize(IRgb color);

        IRgb ToRgb();

        T To<T>() where T : IColorSpace, new();

        double Compare(IColorSpace compareToValue, IColorSpaceComparison comparer);
    }

    public abstract class ColorSpace : IColorSpace
    {
        public abstract void Initialize(IRgb color);
        public abstract IRgb ToRgb();
        public double Compare(IColorSpace compareToValue, IColorSpaceComparison comparer)
        {
            return comparer.Compare(this, compareToValue);
        }

        public T To<T>() where T : IColorSpace, new()
        {
            if (typeof (T) == GetType())
            {
                return (T) MemberwiseClone();
            }

            var newColorSpace = new T();
            newColorSpace.Initialize(ToRgb());

            return newColorSpace;
        }
    }
}