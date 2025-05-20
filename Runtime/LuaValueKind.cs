namespace Ectoplasm.Runtime;

public enum LuaValueKind
{
    /// <summary>
    /// Nil, no value whatsoever. In boolean operations, is considered the same as false. No underlying C# type.
    /// </summary>
    Nil,
    
    /// <summary>
    /// Boolean value, true or false. Underlying C# type <see cref="bool"/>.
    /// </summary>
    Boolean,
    
    /// <summary>
    /// Signed integer value, 64-bit. Underlying C# type <see cref="long"/>.
    /// </summary>
    Integer,
    
    /// <summary>
    /// IEEE floating point value, 64-bit. Underlying C# type <see cref="double"/>.
    /// </summary>
    Float,
    
    /// <summary>
    /// Immutable byte sequence. Underlying C# type is <see cref="LuaString"/>, though this type is marked as internal,
    /// and it is only externally accessible through <see cref="LuaValue"/>.
    /// </summary>
    String,
    
    /// <summary>
    /// Lua function type. Underlying C# type <see cref="LuaFunction"/>.
    /// </summary>
    Function,
    
    /// <summary>
    /// Userdata type, arbitrary data passed in by Lua's native runtime environment. Underlying C# type
    /// <see cref="LuaUserdata"/>.
    /// </summary>
    Userdata,
    
    /// <summary>
    /// Lua thread type, used by Lua coroutines to track state when starting and stopping. Underlying C# type
    /// <see cref="LuaThread"/>.
    /// </summary>
    Thread,
    
    /// <summary>
    /// Lua table type. Underlying C# type <see cref="LuaTable"/>.
    /// </summary>
    Table
}