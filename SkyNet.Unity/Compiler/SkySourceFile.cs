using System;
using System.Text;

internal class SkySourceFile : IDisposable
{
    private int indent = 0;
    private string file = "";
    private StringBuilder buffer = new StringBuilder();

    public int IndentLevel
    {
        get
        {
            return indent;
        }
        set
        {
            indent = value;
        }
    }

    public SkySourceFile(string path)
    {
        file = path;
    }

    public void Emit(string text)
    {
        buffer.Append(text);
    }

    public void Emit(string text, params object[] args)
    {
        buffer.Append(string.Format(text, args));
    }

    public void EmitBOL()
    {
        EmitBOL("");
    }

    public void EmitBOL(string text)
    {
        Emit(new string(' ', indent * 2));
        Emit(text);
    }

    public void EmitEOL(string text, params object[] args)
    {
        EmitEOL(string.Format(text, args));
    }

    public void EmitEOL(string text)
    {
        Emit(text);
        Emit("\r\n");
    }

    public void EmitEOL()
    {
        EmitEOL("");
    }

    public void EmitLine(string text)
    {
        EmitBOL();
        Emit(text);
        EmitEOL();
    }

    public void EmitLine(string text, params object[] args)
    {
        EmitBOL();
        Emit(string.Format(text, args));
        EmitEOL();
    }

    public void EmitScope(string text, Action callback)
    {
        EmitLine(text.Trim() + " {");
        Indented((() => callback()));
        EmitLine("}");
    }
   
    public void Indented(Action action)
    {
        ++IndentLevel;
        action();
        --IndentLevel;
    }

    public void Save()
    {
        if (System.IO.File.Exists(file))
            System.IO.File.Delete(file);
        System.IO.File.WriteAllText(file, buffer.ToString());
    }

    public void Dispose()
    {
        Save();
    }
}
