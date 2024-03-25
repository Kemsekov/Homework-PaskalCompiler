namespace Modules;
/// <summary>
/// Binary-disjoint enum of identifier type
/// </summary>
public enum IdentifierType{
    PROGS   =0x000001,  // program
    TYPES   =0x000010,  // type
    CONSTS  =0x00100,   // constant
    VARS    =0x001000,  // variable
    PROCS   =0x010000,  // procedure
    FUNCS   =0x100000   // function
}
