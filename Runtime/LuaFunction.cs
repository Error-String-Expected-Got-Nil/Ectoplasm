namespace Ectoplasm.Runtime;

/// <summary>
/// Delegate type representing a Lua function. The LuaState holds arguments and accepts return values. The nullable
/// return of its own type indicates whether this function tail-called: If another LuaFunction was returned, this
/// indicates the function has a tail call, and it should be executed.
/// </summary>
public delegate LuaFunction? LuaFunction(LuaState state);