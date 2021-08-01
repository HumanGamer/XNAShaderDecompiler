namespace XNAShaderDecompiler
{
    public enum SymbolClass
    {
        MOJOSHADER_SYMCLASS_SCALAR = 0,
        MOJOSHADER_SYMCLASS_VECTOR,
        MOJOSHADER_SYMCLASS_MATRIX_ROWS,
        MOJOSHADER_SYMCLASS_MATRIX_COLUMNS,
        MOJOSHADER_SYMCLASS_OBJECT,
        MOJOSHADER_SYMCLASS_STRUCT,
        MOJOSHADER_SYMCLASS_TOTAL /* housekeeping value; never returned. */
    }
}