using System.Runtime.InteropServices;
using System.Text;

namespace Ectoplasm.Runtime;

/// <summary>
/// Represents a dynamic value used in Lua.
/// </summary>
/// <remarks>
/// To create a <see cref="LuaValue"/> with a <see cref="Kind"/> of <see cref="LuaValueKind.Nil"/>, use the 'default'
/// keyword.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct LuaValue
{
    [FieldOffset(0)] internal readonly bool _boolean;
    [FieldOffset(0)] internal readonly long _integer;
    [FieldOffset(0)] internal readonly double _float;

    // Unfortunately, you aren't allowed to have a reference type overlapping a non-reference type in an explicitly
    // laid out struct, so we can't have a union of every possible type. Besides having this extra reference field, the
    // only other practical solution seemed like having a *single* object-type field, but that would necessitate boxing
    // and unboxing whenever a bool/long/double was accessed, which seemed worse than increasing the size of a LuaValue
    // from 16 to 24 bytes.
    [FieldOffset(8)] internal readonly object _ref = null!;
    
    /// <summary>
    /// The actual runtime type of this dynamically-typed Lua value.
    /// </summary>
    [FieldOffset(16)] public readonly LuaValueKind Kind;
    
    /// <summary>
    /// Determines the truthiness of this LuaValue. Returns true if <see cref="Kind"/> is not
    /// <see cref="LuaValueKind.Nil"/>, or <see cref="LuaValueKind.Boolean"/> with an underlying value of false. False
    /// otherwise.
    /// </summary>
    public bool IsTruthy => Kind == LuaValueKind.Boolean ? _boolean : Kind != LuaValueKind.Nil;

    #region Checked Getter Properties
    
    /// <summary>
    /// Gets this LuaValue as a bool.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Boolean"/>.
    /// </exception>
    public bool Boolean 
        => Kind == LuaValueKind.Boolean 
            ? _boolean 
            : throw new InvalidCastException("LuaValue does not represent a boolean value.");

    /// <summary>
    /// Gets this LuaValue as a long.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Integer"/>.
    /// </exception>
    public long Integer
        => Kind == LuaValueKind.Integer
            ? _integer
            : throw new InvalidCastException("LuaValue does not represent an integer value.");
    
    /// <summary>
    /// Gets this LuaValue as a double.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Float"/>.
    /// </exception>
    public double Float
        => Kind == LuaValueKind.Float
            ? _float
            : throw new InvalidCastException("LuaValue does not represent a float value.");
    
    /// <summary>
    /// Gets this LuaValue as a read-only span. Does not return the underlying byte array, as Lua strings are immutable.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/>.
    /// </exception>
    public ReadOnlySpan<byte> String
        => Kind == LuaValueKind.String
            ? ((LuaString)_ref).Data
            : throw new InvalidCastException("LuaValue does not represent a Lua string value.");
    
    /// <summary>
    /// Gets this LuaValue as a string.
    /// </summary>
    /// <remarks>
    /// Be aware that a Lua string is, strictly speaking, only an immutable sequence of bytes, and not necessarily a
    /// valid UTF-8 character sequence.
    /// </remarks>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown by internal call if underlying Lua string did not actually represent a valid UTF-8 character sequence.
    /// </exception>
    public string StringUtf16 
        => Kind == LuaValueKind.String 
            ? ((LuaString)_ref).DataUtf16
            : throw new InvalidCastException("LuaValue does not represent a Lua string value.");
        
        
    /// <summary>
    /// Gets this LuaValue as a Lua function delegate.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Function"/>.
    /// </exception>
    public LuaFunction Function
        => Kind == LuaValueKind.Function
            ? (LuaFunction)_ref
            : throw new InvalidCastException("LuaValue does not represent a function value.");
    
    /// <summary>
    /// Gets this LuaValue as an object.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/>.
    /// </exception>
    public LuaUserdata Userdata
        => Kind == LuaValueKind.Userdata
            ? (LuaUserdata)_ref
            : throw new InvalidCastException("LuaValue does not represent a userdata value.");

    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaThread"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Thread"/>.
    /// </exception>
    public LuaThread Thread
        => Kind == LuaValueKind.Thread
            ? (LuaThread)_ref
            : throw new InvalidCastException("LuaValue does not represent a Lua thread value.");
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaTable"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Table"/>.
    /// </exception>
    public LuaTable Table
        => Kind == LuaValueKind.Table
            ? (LuaTable)_ref
            : throw new InvalidCastException("LuaValue does not represent a table value.");
    
    #endregion
    
    #region Nullable Checked Getter Properties

    /// <summary>
    /// Gets this LuaValue as a bool, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Boolean"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public bool? NullableBoolean
        => Kind == LuaValueKind.Nil
            ? null
            : Boolean;
    
    /// <summary>
    /// Gets this LuaValue as a long, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Integer"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public long? NullableInteger
        => Kind == LuaValueKind.Nil
            ? null
            : Integer;
    
    /// <summary>
    /// Gets this LuaValue as a double, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Float"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public double? NullableFloat
        => Kind == LuaValueKind.Nil
            ? null
            : Float;
    
    /// <summary>
    /// Gets this LuaValue as a read-only span, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// Does not return the underlying byte array, as Lua strings are immutable.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public ReadOnlySpan<byte> NullableString
        => Kind == LuaValueKind.Nil
            ? null
            : String;

    /// <summary>
    /// Gets this LuaValue as a string, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <remarks>
    /// Be aware that a Lua string is, strictly speaking, only an immutable sequence of bytes, and not necessarily a
    /// valid UTF-8 character sequence.
    /// </remarks>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown by internal call if underlying Lua string did not actually represent a valid UTF-8 character sequence.
    /// </exception>
    public string? NullableStringUtf16
        => Kind == LuaValueKind.Nil
            ? null
            : StringUtf16;
    
    /// <summary>
    /// Gets this LuaValue as a Lua function delegate, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Function"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaFunction? NullableFunction
        => Kind == LuaValueKind.Nil
            ? null
            : Function;
    
    /// <summary>
    /// Gets this LuaValue as an object, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaUserdata? NullableUserdata
        => Kind == LuaValueKind.Nil
            ? null
            : Userdata;
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaThread"/>, or null if <see cref="Kind"/> is
    /// <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Thread"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaThread? NullableThread
        => Kind == LuaValueKind.Nil
            ? null
            : Thread;
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaTable"/>, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Table"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaTable? NullableTable
        => Kind == LuaValueKind.Nil
            ? null
            : Table;
    
    #endregion

    #region Basic Constructors

    /// <summary>
    /// Creates a new LuaValue with boolean value.
    /// </summary>
    /// <param name="value">Boolean value of the new LuaValue.</param>
    public LuaValue(bool value)
    {
        _boolean = value;
        Kind = LuaValueKind.Boolean;
    }

    /// <summary>
    /// Creates a new LuaValue with integer value.
    /// </summary>
    /// <param name="value">Integer value of the new LuaValue.</param>
    public LuaValue(long value)
    {
        _integer = value;
        Kind = LuaValueKind.Integer;
    }

    /// <summary>
    /// Creates a new LuaValue with float value.
    /// </summary>
    /// <param name="value">Float value of the new LuaValue.</param>
    public LuaValue(double value)
    {
        _float = value;
        Kind = LuaValueKind.Float;
    }

    /// <summary>
    /// Creates a new LuaValue with string value.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    public LuaValue(ReadOnlySpan<byte> value)
    {
        _ref = new LuaString(value);
        Kind = LuaValueKind.String;
    }

    /// <summary>
    /// Creates a new LuaValue with string value. Automatically converts the given string to a UTF-8 byte sequence to
    /// be stored in the value.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    public LuaValue(string value)
    {
        _ref = new LuaString(value);
        Kind = LuaValueKind.String;
    }

    /// <summary>
    /// For internal use, create a LuaValue directly from a <see cref="LuaString"/>.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    internal LuaValue(LuaString value)
    {
        _ref = value;
        Kind = LuaValueKind.String;
    }

    /// <summary>
    /// Creates a new LuaValue with function value.
    /// </summary>
    /// <param name="value">Function value of the new LuaValue.</param>
    public LuaValue(LuaFunction? value)
    {
        if (value == null)
        {
            Kind = LuaValueKind.Nil;
            return;
        }
        
        _ref = value;
        Kind = LuaValueKind.Function;
    }
    
    /// <summary>
    /// Creates a new LuaValue with userdata value.
    /// </summary>
    /// <param name="value">Userdata value of the new LuaValue.</param>
    public LuaValue(LuaUserdata? value)
    {
        if (value == null)
        {
            Kind = LuaValueKind.Nil;
            return;
        }
        
        _ref = value;
        Kind = LuaValueKind.Userdata;
    }

    /// <summary>
    /// Creates a new LuaValue with Lua thread value.
    /// </summary>
    /// <param name="value">Lua thread value of the new LuaValue.</param>
    public LuaValue(LuaThread? value)
    {
        if (value == null)
        {
            Kind = LuaValueKind.Nil;
            return;
        }
        
        _ref = value;
        Kind = LuaValueKind.Thread;
    }

    /// <summary>
    /// Creates a new LuaValue with Lua table value.
    /// </summary>
    /// <param name="value">Lua table value of the new LuaValue.</param>
    public LuaValue(LuaTable? value)
    {
        if (value == null)
        {
            Kind = LuaValueKind.Nil;
            return;
        }
        
        _ref = value;
        Kind = LuaValueKind.Table;
    }
    
    #endregion
    
    #region Standard Operations

    /// <summary>
    /// Attempt to coerce this LuaValue to a long.
    /// </summary>
    /// <param name="value">The coerced value, or 0 if a value could not be coerced.</param>
    /// <returns>
    /// True if the LuaValue was an integer, or a float convertable to an integer without loss of fraction. False
    /// otherwise.
    /// </returns>
    public bool TryCoerceInteger(out long value)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (Kind)
        {
            case LuaValueKind.Integer:
                value = _integer;
                return true;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            case LuaValueKind.Float when _float == (long)_float:
                value = (long)_float;
                return true;
            default:
                value = 0;
                return false;
        }
    }
    
    // TODO: Arithmetic operations with metatables
    
    #endregion
    
    #region Implicit Conversions

    // Implicit conversions are assumed safe; they will never throw an exception.
    
    public static implicit operator LuaValue(bool value) => new(value);
    public static implicit operator LuaValue(long value) => new(value);
    public static implicit operator LuaValue(double value) => new(value);
    public static implicit operator LuaValue(ReadOnlySpan<byte> value) => new(value);
    public static implicit operator LuaValue(string value) => new(value);
    public static implicit operator LuaValue(LuaFunction value) => new(value);
    public static implicit operator LuaValue(LuaUserdata value) => new(value);
    public static implicit operator LuaValue(LuaThread value) => new(value);
    public static implicit operator LuaValue(LuaTable value) => new(value);

    public static implicit operator bool?(LuaValue value) => value.NullableBoolean;
    public static implicit operator long?(LuaValue value) => value.NullableInteger;
    public static implicit operator double?(LuaValue value) => value.NullableFloat;

    #endregion
    
    #region Explicit Conversions
    
    // Explicit conversions are not necessarily safe; they may throw an exception, hence why they are explicit.
    // In this case they are all syntactic sugar for the checked getter properties.
    
    public static explicit operator bool(LuaValue value) => value.Boolean;
    public static explicit operator long(LuaValue value) => value.Integer;
    public static explicit operator double(LuaValue value) => value.Float;
    public static explicit operator ReadOnlySpan<byte>(LuaValue value) => value.String;
    public static explicit operator string(LuaValue value) => value.StringUtf16;
    public static explicit operator LuaFunction(LuaValue value) => value.Function;
    public static explicit operator LuaUserdata(LuaValue value) => value.Userdata;
    public static explicit operator LuaThread(LuaValue value) => value.Thread;
    public static explicit operator LuaTable(LuaValue value) => value.Table;
    
    #endregion
    
    #region Operator Overloads

    public static bool operator true(LuaValue value) => value.IsTruthy;
    public static bool operator false(LuaValue value) => !value.IsTruthy;

    #endregion
}