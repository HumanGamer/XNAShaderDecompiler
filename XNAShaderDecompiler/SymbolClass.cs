namespace XNAShaderDecompiler
{
    public enum SymbolClass:uint
    {
        Scalar = 0,
        Vector,
        MatrixRows,
        MatrixColumns,
        Object,
        Struct,
        Total /* housekeeping value; never returned. */
    }
}