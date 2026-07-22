using Ectoplasm.Runtime.Tables;

namespace Ectoplasm.Runtime;

public class LuaUserdata(object userdata, LuaTable? metatable = null)
{
    /// <summary>
    /// The .NET object contained in this userdata.
    /// </summary>
    public readonly object Userdata = userdata;

    /// <summary>
    /// The metatable attached to this userdata object, if any. See the Lua reference manual (version 5.4, section 2.4)
    /// if unfamiliar.
    /// </summary>
    public LuaTable? Metatable = metatable;
}