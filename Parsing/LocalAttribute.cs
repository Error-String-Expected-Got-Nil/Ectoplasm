namespace Ectoplasm.Parsing;

/// <summary>
/// 
/// </summary>
public enum LocalAttribute
{
    /// <summary>
    /// Local has no attribute.
    /// </summary>
    None,
    
    /// <summary>
    /// Local has 'const' attribute, indicating it can only be assigned in its declaration statement.
    /// </summary>
    Const,
    
    /// <summary>
    /// Local has 'close' attribute, indicating it should be closed on falling out of scope. For more details on what
    /// this means, see the Lua reference manual (version 5.4, section 3.3.8). To-be-closed variables are also
    /// considered constant in the same way as variables with the 'const' attribute.
    /// </summary>
    Close
}