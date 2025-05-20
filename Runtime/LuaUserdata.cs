namespace Ectoplasm.Runtime;

public class LuaUserdata(object userdata, LuaTable? metatable = null)
{
    public object Userdata = userdata;
    public LuaTable? Metatable = metatable;
}