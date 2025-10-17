using Ectoplasm.Runtime.Values;
using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.SimpleExpressions;

/// <summary>
/// Delegate type for a simple expression that has been compiled to a dynamic method.
/// </summary>
public delegate LuaValue SimpleExpressionDelegate(LuaTable? env);