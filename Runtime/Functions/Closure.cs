using Ectoplasm.Runtime.Values;

namespace Ectoplasm.Runtime.Functions;

/// <summary>
/// Holds all data necessary for a Lua function closure, intended to be attached as an instance object to Lua function
/// delegates. Also holds chunk debug info, if captured during compilation.
/// </summary>
public record Closure(Upvalue[] Upvalues, LuaValue[] Constants, Prototype[] Prototypes, ChunkDebugInfo? DebugInfo);