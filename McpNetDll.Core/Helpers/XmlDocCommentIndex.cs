using System.Xml.Linq;

namespace McpNetDll.Helpers;

public class XmlDocCommentIndex
{
    private readonly Dictionary<(string type, string name), string> _fieldDocs = new();
    private readonly Dictionary<(string type, string name), string> _methodDocs = new();
    private readonly Dictionary<(string type, string name), string> _propDocs = new();
    private readonly Dictionary<string, string> _typeDocs = new(StringComparer.Ordinal);

    public void AddFromXml(string xmlPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath)) return;
        try
        {
            var doc = XDocument.Load(xmlPath);
            var members = doc.Root?.Element("members")?.Elements("member") ?? Enumerable.Empty<XElement>();
            foreach (var m in members)
            {
                var nameAttr = m.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(nameAttr)) continue;
                var summary = (m.Element("summary")?.Value ?? m.Value)?.Trim();
                if (string.IsNullOrWhiteSpace(summary)) continue;

                switch (nameAttr[0])
                {
                    case 'T':
                        // T:Namespace.Type
                        var typeFull = nameAttr.Length > 2 ? nameAttr.Substring(2) : null;
                        if (!string.IsNullOrWhiteSpace(typeFull)) _typeDocs[typeFull!] = Normalize(summary);
                        break;
                    case 'P':
                        // P:Namespace.Type.Property
                        ParseMember(nameAttr.Substring(2), out var pType, out var pName);
                        if (!string.IsNullOrEmpty(pType) && !string.IsNullOrEmpty(pName))
                            _propDocs[(pType!, pName!)] = Normalize(summary);
                        break;
                    case 'F':
                        // F:Namespace.Type.Field
                        ParseMember(nameAttr.Substring(2), out var fType, out var fName);
                        if (!string.IsNullOrEmpty(fType) && !string.IsNullOrEmpty(fName))
                            _fieldDocs[(fType!, fName!)] = Normalize(summary);
                        break;
                    case 'M':
                        // M:Namespace.Type.Method(Params)
                        var sig = nameAttr.Substring(2);
                        var paren = sig.IndexOf('(');
                        var withoutParams = paren >= 0 ? sig.Substring(0, paren) : sig;
                        ParseMember(withoutParams, out var mType, out var mName);
                        if (!string.IsNullOrEmpty(mType) && !string.IsNullOrEmpty(mName))
                            _methodDocs[(mType!, mName!)] = Normalize(summary);
                        break;
                }
            }
        }
        catch
        {
            // ignore malformed XML docs
        }
    }

    public string? GetTypeDoc(string fullTypeName)
    {
        return _typeDocs.TryGetValue(fullTypeName, out var d) ? d : null;
    }

    public string? GetPropertyDoc(string fullTypeName, string propertyName)
    {
        return _propDocs.TryGetValue((fullTypeName, propertyName), out var d) ? d : null;
    }

    public string? GetFieldDoc(string fullTypeName, string fieldName)
    {
        return _fieldDocs.TryGetValue((fullTypeName, fieldName), out var d) ? d : null;
    }

    public string? GetMethodDoc(string fullTypeName, string methodName)
    {
        return _methodDocs.TryGetValue((fullTypeName, methodName), out var d) ? d : null;
    }

    private static void ParseMember(string value, out string? typeFull, out string? memberName)
    {
        typeFull = null;
        memberName = null;
        if (string.IsNullOrWhiteSpace(value)) return;
        var lastDot = value.LastIndexOf('.');
        if (lastDot <= 0 || lastDot >= value.Length - 1) return;
        typeFull = value.Substring(0, lastDot);
        memberName = value.Substring(lastDot + 1);
    }

    private static string Normalize(string s)
    {
        return string.Join(" ",
                s.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
            .Trim();
    }
}