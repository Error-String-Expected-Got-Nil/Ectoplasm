using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Ectoplasm.Lexing;
using Ectoplasm.Runtime.Stdlib;
using Ectoplasm.Runtime.Tables;
using Ectoplasm.Runtime.Functions;

namespace Ectoplasm.Runtime.Values;

/// <summary>
/// Represents a dynamic value used in Lua.
/// </summary>
/// <remarks>
/// To create a <see cref="LuaValue"/> with a <see cref="Kind"/> of <see cref="LuaValueKind.Nil"/>, use the 'default'
/// keyword.
/// </remarks>
[StructLayout(LayoutKind.Explicit)]
public struct LuaValue : IEquatable<LuaValue>
{
    [FieldOffset(0)] internal bool _boolean;
    [FieldOffset(0)] internal long _integer;
    [FieldOffset(0)] internal double _float;

    // Unfortunately, you aren't allowed to have a reference type overlapping a non-reference type in an explicitly
    // laid out struct, so we can't have a union of every possible type. Besides having this extra reference field, the
    // only other practical solution seemed like having a *single* object-type field, but that would necessitate boxing
    // and unboxing whenever a bool/long/double was accessed, which seemed worse than increasing the size of a LuaValue
    // from 16 to 24 bytes.
    [FieldOffset(8)] internal object _ref = null!;
    
    /// <summary>
    /// Internal field with the actual runtime type of this dynamically-typed Lua value.
    /// </summary>
    [FieldOffset(16)] internal LuaValueKind _kind;

    /// <summary>
    /// The actual runtime type of this dynamically-typed Lua value.
    /// </summary>
    public LuaValueKind Kind => _kind;
    
    /// <summary>
    /// Determines the truthiness of this LuaValue. If <see cref="Kind"/> is <see cref="LuaValueKind.Boolean"/>, returns
    /// the underlying value of this LuaValue. Otherwise, returns true if <see cref="Kind"/> is not
    /// <see cref="LuaValueKind.Nil"/>, false if it is.
    /// </summary>
    public bool IsTruthy => _kind == LuaValueKind.Boolean ? _boolean : _kind != LuaValueKind.Nil;

    #region Checked Getter Properties
    
    /// <summary>
    /// Gets this LuaValue as a bool.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Boolean"/>.
    /// </exception>
    public bool Boolean 
        => _kind == LuaValueKind.Boolean 
            ? _boolean 
            : throw new InvalidCastException("LuaValue does not represent a boolean value.");

    /// <summary>
    /// Gets this LuaValue as a long.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Integer"/>.
    /// </exception>
    public long Integer
        => _kind == LuaValueKind.Integer
            ? _integer
            : throw new InvalidCastException("LuaValue does not represent an integer value.");
    
    /// <summary>
    /// Gets this LuaValue as a double.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Float"/>.
    /// </exception>
    public double Float
        => _kind == LuaValueKind.Float
            ? _float
            : throw new InvalidCastException("LuaValue does not represent a float value.");
    
    /// <summary>
    /// Gets this LuaValue as a string.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/>.
    /// </exception>
    public string String
        => _kind == LuaValueKind.String
            ? (string)_ref
            : throw new InvalidCastException("LuaValue does not represent a string value.");
        
    /// <summary>
    /// Gets this LuaValue as a Lua function delegate.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Function"/>.
    /// </exception>
    public LuaFunction Function
        => _kind == LuaValueKind.Function
            ? (LuaFunction)_ref
            : throw new InvalidCastException("LuaValue does not represent a function value.");
    
    /// <summary>
    /// Gets this LuaValue as an object.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/>.
    /// </exception>
    public LuaUserdata Userdata
        => _kind == LuaValueKind.Userdata
            ? (LuaUserdata)_ref
            : throw new InvalidCastException("LuaValue does not represent a userdata value.");

    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaThread"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Thread"/>.
    /// </exception>
    public LuaThread Thread
        => _kind == LuaValueKind.Thread
            ? (LuaThread)_ref
            : throw new InvalidCastException("LuaValue does not represent a Lua thread value.");
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaTable"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Table"/>.
    /// </exception>
    public LuaTable Table
        => _kind == LuaValueKind.Table
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
        => _kind == LuaValueKind.Nil
            ? null
            : Boolean;
    
    /// <summary>
    /// Gets this LuaValue as a long, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Integer"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public long? NullableInteger
        => _kind == LuaValueKind.Nil
            ? null
            : Integer;
    
    /// <summary>
    /// Gets this LuaValue as a double, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Float"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public double? NullableFloat
        => _kind == LuaValueKind.Nil
            ? null
            : Float;
    
    /// <summary>
    /// Gets this LuaValue as a string, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.String"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public string? NullableString
        => _kind == LuaValueKind.Nil
            ? null
            : String;
    
    /// <summary>
    /// Gets this LuaValue as a Lua function delegate, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Function"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaFunction? NullableFunction
        => _kind == LuaValueKind.Nil
            ? null
            : Function;
    
    /// <summary>
    /// Gets this LuaValue as an object, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Userdata"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaUserdata? NullableUserdata
        => _kind == LuaValueKind.Nil
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
        => _kind == LuaValueKind.Nil
            ? null
            : Thread;
    
    /// <summary>
    /// Gets this LuaValue as a <see cref="LuaTable"/>, or null if <see cref="Kind"/> is <see cref="LuaValueKind.Nil"/>.
    /// </summary>
    /// <exception cref="InvalidCastException">
    /// Thrown if <see cref="Kind"/> is not <see cref="LuaValueKind.Table"/> or <see cref="LuaValueKind.Nil"/>.
    /// </exception>
    public LuaTable? NullableTable
        => _kind == LuaValueKind.Nil
            ? null
            : Table;
    
    #endregion

    #region Basic Constructors

    /// <summary>
    /// Creates a new LuaValue with boolean value.
    /// </summary>
    /// <param name="value">Boolean value of the new LuaValue.</param>
    public static LuaValue New(bool value)
        => new()
        {
            _boolean = value,
            _kind = LuaValueKind.Boolean
        };

    /// <summary>
    /// Creates a new LuaValue with integer value.
    /// </summary>
    /// <param name="value">Integer value of the new LuaValue.</param>
    public static LuaValue New(long value)
        => new()
        {
            _integer = value,
            _kind = LuaValueKind.Integer
        };

    /// <summary>
    /// Creates a new LuaValue with float value.
    /// </summary>
    /// <param name="value">Float value of the new LuaValue.</param>
    public static LuaValue New(double value)
        => new()
        {
            _float = value,
            _kind = LuaValueKind.Float
        };

    /// <summary>
    /// Creates a new LuaValue with string value. Automatically converts the given string to a UTF-8 byte sequence to
    /// be stored in the value.
    /// </summary>
    /// <param name="value">String value of the new LuaValue.</param>
    public static LuaValue New(string value)
        => new()
        {
            _ref = value,
            _kind = LuaValueKind.String
        };

    /// <summary>
    /// Creates a new LuaValue with function value.
    /// </summary>
    /// <param name="value">Function value of the new LuaValue.</param>
    public static LuaValue New(LuaFunction value)
        => new()
        {
            _ref = value,
            _kind = LuaValueKind.Function
        };
    
    /// <summary>
    /// Creates a new LuaValue with userdata value.
    /// </summary>
    /// <param name="value">Userdata value of the new LuaValue.</param>
    public static LuaValue New(LuaUserdata value)
        => new()
        {
            _ref = value,
            _kind = LuaValueKind.Userdata
        };

    /// <summary>
    /// Creates a new LuaValue with Lua thread value.
    /// </summary>
    /// <param name="value">Lua thread value of the new LuaValue.</param>
    public static LuaValue New(LuaThread value)
        => new()
        {
            _ref = value,
            _kind = LuaValueKind.Thread
        };

    /// <summary>
    /// Creates a new LuaValue with Lua table value.
    /// </summary>
    /// <param name="value">Lua table value of the new LuaValue.</param>
    public static LuaValue New(LuaTable value)
        => new()
        {
            _ref = value,
            _kind = LuaValueKind.Table
        };

    /// <summary>
    /// Creates a new LuaValue from a <see cref="LuaToken"/>. 
    /// </summary>
    /// <param name="token">The <see cref="LuaToken"/> holding the value of the new LuaValue.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the <see cref="LuaToken"/> is not a value.
    /// </exception>
    // This constructor remains after the switch to the New functions since efficiency doesn't matter as much for it.
    public LuaValue(LuaToken token)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (token.Type)
        {
            case TokenType.Nil:
                _kind = LuaValueKind.Nil;
                return;
            case TokenType.True:
                _kind = LuaValueKind.Boolean;
                _boolean = true;
                return;
            case TokenType.False:
                _kind = LuaValueKind.Boolean;
                _boolean = false;
                return;
            case TokenType.String:
                _kind = LuaValueKind.String;
                _ref = (string)token.Data!;
                return;
            case TokenType.Numeral:
                break;
            default:
                throw new ArgumentException(
                    $"Attempt to create LuaValue from non-value LuaToken. Token: \"{token.OriginalString}\"", 
                    nameof(token));
        }

        if (token.Data is long value)
        {
            _kind = LuaValueKind.Integer;
            _integer = value;
            return;
        }

        _kind = LuaValueKind.Float;
        _float = (double)token.Data!;
    }
    
    #endregion
    
    #region Utility Methods

    /// <summary>
    /// Attempt to coerce this LuaValue to a long.
    /// </summary>
    /// <param name="value">The coerced value, or 0 if a value could not be coerced.</param>
    /// <returns>
    /// True if the LuaValue was an integer, or a float convertable to an integer without loss of fraction. False
    /// otherwise.
    /// </returns>
    // TODO: Add bool argument to optionally make this permit string coercion as well
    public bool TryCoerceInteger(out long value)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_kind)
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

    public override string ToString() => GlobalFunctions.LuaToString(this);

    public override bool Equals([NotNullWhen(true)] object? obj) 
        => obj is LuaValue other && GlobalFunctions.LuaValueEquality(this, other);
    
    public bool Equals(LuaValue other)
        => GlobalFunctions.LuaValueEquality(this, other);

    public override int GetHashCode()
        => _kind switch
        {
            LuaValueKind.Nil => -1,
            LuaValueKind.Boolean => _boolean.GetHashCode(),
            LuaValueKind.Integer => _integer.GetHashCode(),
            // Lua attempts to treat any float that is coercible to an integer without loss of precision the same as
            // that integer, so we must do the same here.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            LuaValueKind.Float => _float == (long)_float
                ? ((long)_float).GetHashCode() 
                : _float.GetHashCode(),
            _ => _ref.GetHashCode()
        };

    #endregion
    
    #region Implicit Conversions

    // Implicit conversions are assumed safe; they will never throw an exception.
    
    public static implicit operator LuaValue(bool value) => New(value);
    public static implicit operator LuaValue(long value) => New(value);
    public static implicit operator LuaValue(double value) => New(value);
    public static implicit operator LuaValue(string value) => New(value);
    public static implicit operator LuaValue(LuaFunction value) => New(value);
    public static implicit operator LuaValue(LuaUserdata value) => New(value);
    public static implicit operator LuaValue(LuaThread value) => New(value);
    public static implicit operator LuaValue(LuaTable value) => New(value);

    #endregion
    
    #region Explicit Conversions
    
    // Explicit conversions are not necessarily safe; they may throw an exception, hence why they are explicit.
    // In this case they are all syntactic sugar for the checked getter properties.
    
    public static explicit operator bool(LuaValue value) => value.Boolean;
    public static explicit operator long(LuaValue value) => value.Integer;
    public static explicit operator double(LuaValue value) => value.Float;
    public static explicit operator bool?(LuaValue value) => value.NullableBoolean;
    public static explicit operator long?(LuaValue value) => value.NullableInteger;
    public static explicit operator double?(LuaValue value) => value.NullableFloat;
    public static explicit operator string(LuaValue value) => value.String;
    public static explicit operator LuaFunction(LuaValue value) => value.Function;
    public static explicit operator LuaUserdata(LuaValue value) => value.Userdata;
    public static explicit operator LuaThread(LuaValue value) => value.Thread;
    public static explicit operator LuaTable(LuaValue value) => value.Table;
    
    #endregion
    
    #region Operator Overloads

    public static bool operator true(LuaValue value) => value.IsTruthy;
    public static bool operator false(LuaValue value) => !value.IsTruthy;
    public static bool operator ==(LuaValue left, LuaValue right) => left.Equals(right);
    public static bool operator !=(LuaValue left, LuaValue right) => !(left == right);

    #endregion
}