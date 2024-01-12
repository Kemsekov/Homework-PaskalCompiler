public static class ProgramFileReader
{
    public static ProgramText ReadProgramFile(string path)
    {
        if (!File.Exists(path))
            throw new ArgumentException("File not found");

        return new ProgramText(File.ReadAllLines(path));
    }
}