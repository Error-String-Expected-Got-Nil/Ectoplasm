using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Holds all data necessary for a Lua function closure, intended to be attached as an instance object to Lua function
/// delegates.
/// </summary>
public record Closure(Upvalue[] Upvalues, LuaValue[] Constants, Prototype[] Prototypes);