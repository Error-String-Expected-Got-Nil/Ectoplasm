using System.Runtime.InteropServices;
using System.Text;

namespace Ectoplasm.Runtime;

/// <summary>
/// Represents a dynamic value used in Lua.
/// </summary>
/// <remarks>
/// Struct is sized and laid out assuming a 64-bit process. It will still work with a 32-bit process, but some space
/// will be wasted. This is unfortunately unavoidable.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct LuaValue
{
    [FieldOffset(0)] private bool _boolean;
    [FieldOffset(0)] private long _integer;
    [FieldOffset(0)] private double _float;
    [FieldOffset(0)] private byte[] _string = null!;
    [FieldOffset(0)] private Func<LuaState, LuaState> _function = null!;
    [FieldOffset(0)] private object _userdata = null!;
    [FieldOffset(0)] private LuaThread _thread = null!;
    [FieldOffset(0)] private LuaTable _table = null!;

    [FieldOffset(8)] private LuaTable? _metatable;
    
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
            ? _string
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
            ? Encoding.UTF8.GetString(_string) 
            : throw new InvalidCastException("LuaValue does not represent a Lua string value.");
        
        
    /// <summary>
    /// Gets this LuaValue as a Lua function delegate.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Function"/>.
    /// </exception>
    public Func<LuaState, LuaState> Function
        => Kind == LuaValueKind.Function
            ? _function
            : throw new InvalidCastException("LuaValue does not represent a function value.");
    
    /// <summary>
    /// Gets this LuaValue as an object.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/>.
    /// </exception>
    public object Userdata
        => Kind == LuaValueKind.Userdata
            ? _userdata
            : throw new InvalidCastException("LuaValue does not represent a userdata value.");

    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaThread"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Thread"/>.
    /// </exception>
    public LuaThread Thread
        => Kind == LuaValueKind.Thread
            ? _thread
            : throw new InvalidCastException("LuaValue does not represent a Lua thread value.");
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaTable"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Table"/>.
    /// </exception>
    public LuaTable Table
        => Kind == LuaValueKind.Table
            ? _table
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
    public Func<LuaState, LuaState>? NullableFunction
        => Kind == LuaValueKind.Nil
            ? null
            : Function;
    
    /// <summary>
    /// Gets this LuaValue as an object, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public object? NullableUserdata
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
    /// Creates a new LuaValue with nil value and no metatable.
    /// </summary>
    public LuaValue()
    {
        _metatable = null;
        Kind = LuaValueKind.Nil;
    }

    /// <summary>
    /// Creates a new LuaValue with boolean value and no metatable.
    /// </summary>
    /// <param name="value">Boolean value of the new LuaValue.</param>
    public LuaValue(bool value)
    {
        _boolean = value;
        _metatable = null;
        Kind = LuaValueKind.Boolean;
    }

    /// <summary>
    /// Creates a new LuaValue with integer value and no metatable.
    /// </summary>
    /// <param name="value">Integer value of the new LuaValue.</param>
    public LuaValue(long value)
    {
        _integer = value;
        _metatable = null;
        Kind = LuaValueKind.Integer;
    }

    /// <summary>
    /// Creates a new LuaValue with float value and no metatable.
    /// </summary>
    /// <param name="value">Float value of the new LuaValue.</param>
    public LuaValue(double value)
    {
        _float = value;
        _metatable = null;
        Kind = LuaValueKind.Float;
    }

    /// <summary>
    /// Creates a new LuaValue with string value and no metatable.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    public LuaValue(byte[] value)
    {
        _string = value;
        _metatable = null;
        Kind = LuaValueKind.String;
    }

    /// <summary>
    /// Creates a new LuaValue with string value and no metatable. Automatically converts the given string to a UTF-8
    /// byte sequence to be stored in the value.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    public LuaValue(string value)
    {
        _string = Encoding.UTF8.GetBytes(value);
        _metatable = null;
        Kind = LuaValueKind.String;
    }

    /// <summary>
    /// Creates a new LuaValue with function value and no metatable.
    /// </summary>
    /// <param name="value">Function value of the new LuaValue.</param>
    public LuaValue(Func<LuaState, LuaState> value)
    {
        _function = value;
        _metatable = null;
        Kind = LuaValueKind.Function;
    }
    
    /// <summary>
    /// Creates a new LuaValue with userdata value and no metatable.
    /// </summary>
    /// <param name="value">Userdata value of the new LuaValue.</param>
    public LuaValue(object value)
    {
        _userdata = value;
        _metatable = null;
        Kind = LuaValueKind.Userdata;
    }

    /// <summary>
    /// Creates a new LuaValue with Lua thread value and no metatable.
    /// </summary>
    /// <param name="value">Lua thread value of the new LuaValue.</param>
    public LuaValue(LuaThread value)
    {
        _thread = value;
        _metatable = null;
        Kind = LuaValueKind.Thread;
    }

    /// <summary>
    /// Creates a new LuaValue with Lua table value and no metatable.
    /// </summary>
    /// <param name="value">Lua table value of the new LuaValue.</param>
    public LuaValue(LuaTable value)
    {
        _table = value;
        _metatable = null;
        Kind = LuaValueKind.Table;
    }
    
    #endregion
    
    #region Standard Operations
    
    // TODO: Arithmetic operations with metatables
    
    #endregion
    
    #region Implicit Conversions

    // Implicit conversions are assumed safe; they will never throw an exception.
    
    public static implicit operator LuaValue(bool value) => new(value);
    public static implicit operator LuaValue(long value) => new(value);
    public static implicit operator LuaValue(double value) => new(value);
    public static implicit operator LuaValue(byte[] value) => new(value);
    public static implicit operator LuaValue(string value) => new(value);
    public static implicit operator LuaValue(Func<LuaState, LuaState> value) => new(value);
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
    public static explicit operator Func<LuaState, LuaState>(LuaValue value) => value.Function;
    public static explicit operator LuaThread(LuaValue value) => value.Thread;
    public static explicit operator LuaTable(LuaValue value) => value.Table;
    
    #endregion
}