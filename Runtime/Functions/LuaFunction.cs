namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Delegate type representing a Lua function. The LuaState holds arguments and accepts return values.
/// </summary>
public delegate void LuaFunction(LuaState state);