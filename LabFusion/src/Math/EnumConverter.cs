using System.Linq.Expressions;

namespace LabFusion.Math;

public static class EnumConverter
{
    public static int ConvertToInt32<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.EnumToInt32Func(value);
    }

    public static short ConvertToInt16<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.EnumToInt16Func(value);
    }

    public static byte ConvertToByte<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.EnumToByteFunc(value);
    }

    public static TEnum ConvertToEnum<TEnum>(int value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.Int32ToEnumFunc(value);
    }

    public static TEnum ConvertToEnum<TEnum>(short value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.Int16ToEnumFunc(value);
    }

    public static TEnum ConvertToEnum<TEnum>(byte value) where TEnum : struct, Enum
    {
        return FuncCache<TEnum>.ByteToEnumFunc(value);
    }

    private static class FuncCache<T> where T : struct, Enum
    {
        public static Func<T, int> EnumToInt32Func = CompileEnumToInt32Func<T>();

        public static Func<T, short> EnumToInt16Func = CompileEnumToInt16Func<T>();

        public static Func<T, byte> EnumToByteFunc = CompileEnumToByteFunc<T>();

        public static Func<int, T> Int32ToEnumFunc = CompileInt32ToEnumFunc<T>();

        public static Func<short, T> Int16ToEnumFunc = CompileInt16ToEnumFunc<T>();

        public static Func<byte, T> ByteToEnumFunc = CompileByteToEnumFunc<T>();
    }

    private static Func<TEnum, int> CompileEnumToInt32Func<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(TEnum));

        var body = Expression.Convert(inputParameter, typeof(int));

        var lambda = Expression.Lambda<Func<TEnum, int>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }

    private static Func<TEnum, short> CompileEnumToInt16Func<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(TEnum));

        var body = Expression.Convert(inputParameter, typeof(short));

        var lambda = Expression.Lambda<Func<TEnum, short>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }

    private static Func<TEnum, byte> CompileEnumToByteFunc<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(TEnum));

        var body = Expression.Convert(inputParameter, typeof(byte));

        var lambda = Expression.Lambda<Func<TEnum, byte>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }

    private static Func<int, TEnum> CompileInt32ToEnumFunc<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(int));

        var body = Expression.Convert(inputParameter, typeof(TEnum));

        var lambda = Expression.Lambda<Func<int, TEnum>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }

    private static Func<short, TEnum> CompileInt16ToEnumFunc<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(short));

        var body = Expression.Convert(inputParameter, typeof(TEnum));

        var lambda = Expression.Lambda<Func<short, TEnum>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }

    private static Func<byte, TEnum> CompileByteToEnumFunc<TEnum>() where TEnum : struct, Enum
    {
        var inputParameter = Expression.Parameter(typeof(byte));

        var body = Expression.Convert(inputParameter, typeof(TEnum));

        var lambda = Expression.Lambda<Func<byte, TEnum>>(body, inputParameter);

        var func = lambda.Compile();

        return func;
    }
}
