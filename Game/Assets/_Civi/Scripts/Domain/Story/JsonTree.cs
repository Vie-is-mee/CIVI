using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Civi.Runtime
{
    // Typed JSON tree for TV1 0.1. No runtime package dependency; rejects malformed JSON.
    public static class JsonTree
    {
        public static object Parse(string json) { return new Reader(json).Document(); }
        public static string Write(object value) { var b = new StringBuilder(); Emit(b, value); return b.ToString(); }
        public static Dictionary<string, object> Obj(object v, string where)
        {
            var x = v as Dictionary<string, object>;
            if (x == null) throw new FormatException(where + " must be an object");
            return x;
        }
        public static List<object> Arr(object v, string where)
        {
            var x = v as List<object>;
            if (x == null) throw new FormatException(where + " must be an array");
            return x;
        }
        public static object Need(Dictionary<string, object> d, string key)
        {
            object v;
            if (!d.TryGetValue(key, out v)) throw new FormatException("Missing property " + key);
            return v;
        }
        public static string Str(object v, string where)
        {
            var x = v as string;
            if (x == null) throw new FormatException(where + " must be a string");
            return x;
        }
        public static int Int(object v, string where)
        {
            if (!(v is long) || (long)v > int.MaxValue || (long)v < int.MinValue)
                throw new FormatException(where + " must be an integer");
            return (int)(long)v;
        }
        public static object Clone(object v)
        {
            var dict = v as Dictionary<string, object>;
            if (dict != null)
            {
                var copy = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var item in dict) copy.Add(item.Key, Clone(item.Value));
                return copy;
            }
            var list = v as List<object>;
            if (list != null)
            {
                var copy = new List<object>();
                foreach (var item in list) copy.Add(Clone(item));
                return copy;
            }
            return v;
        }
        public static bool Same(object a, object b)
        {
            if (a == null || b == null) return a == null && b == null;
            if ((a is long || a is double) && (b is long || b is double))
                return Convert.ToDouble(a, CultureInfo.InvariantCulture) ==
                       Convert.ToDouble(b, CultureInfo.InvariantCulture);
            return a.Equals(b);
        }

        static void Emit(StringBuilder b, object v)
        {
            if (v == null) { b.Append("null"); return; }
            if (v is string)
            {
                b.Append('"');
                foreach (char c in (string)v)
                {
                    switch (c)
                    {
                        case '"': b.Append("\\\""); break;
                        case '\\': b.Append("\\\\"); break;
                        case '\n': b.Append("\\n"); break;
                        case '\r': b.Append("\\r"); break;
                        case '\t': b.Append("\\t"); break;
                        case '\b': b.Append("\\b"); break;
                        case '\f': b.Append("\\f"); break;
                        default:
                            if (c < 32) b.Append("\\u").Append(((int)c).ToString("x4"));
                            else b.Append(c);
                            break;
                    }
                }
                b.Append('"'); return;
            }
            if (v is bool) { b.Append((bool)v ? "true" : "false"); return; }
            if (v is long || v is int) { b.Append(Convert.ToString(v, CultureInfo.InvariantCulture)); return; }
            if (v is double)
            {
                double n = (double)v;
                if (double.IsNaN(n) || double.IsInfinity(n)) throw new FormatException("Non-finite number");
                b.Append(n.ToString("R", CultureInfo.InvariantCulture)); return;
            }
            var obj = v as Dictionary<string, object>;
            if (obj != null)
            {
                b.Append('{'); bool first = true;
                foreach (var item in obj)
                {
                    if (!first) b.Append(',');
                    first = false; Emit(b, item.Key); b.Append(':'); Emit(b, item.Value);
                }
                b.Append('}'); return;
            }
            var arr = v as List<object>;
            if (arr != null)
            {
                b.Append('[');
                for (int i = 0; i < arr.Count; i++) { if (i > 0) b.Append(','); Emit(b, arr[i]); }
                b.Append(']'); return;
            }
            throw new FormatException("Unsupported JSON value type " + v.GetType().Name);
        }

        sealed class Reader
        {
            readonly string s; int i;
            public Reader(string json) { if (json == null) throw new ArgumentNullException(nameof(json)); s = json; }
            public object Document()
            {
                object v = Value(0); Ws();
                if (i != s.Length) Fail("Trailing characters");
                return v;
            }
            void Fail(string m) { throw new FormatException(m + " at " + i); }
            void Ws()
            {
                while (i < s.Length && (s[i] == ' ' || s[i] == '\n' || s[i] == '\r' || s[i] == '\t')) i++;
            }
            char Next()
            {
                if (i == s.Length) { Fail("Unexpected end"); return '\0'; }
                return s[i++];
            }
            bool Take(char c) { Ws(); if (i < s.Length && s[i] == c) { i++; return true; } return false; }
            void Expect(char c) { if (Next() != c) Fail("Expected " + c); }
            object Value(int depth)
            {
                if (depth > 80) Fail("Nesting too deep");
                Ws(); if (i == s.Length) Fail("Missing value");
                switch (s[i])
                {
                    case '{': return Object(depth + 1);
                    case '[': return Array(depth + 1);
                    case '"': return String();
                    case 't': Literal("true"); return true;
                    case 'f': Literal("false"); return false;
                    case 'n': Literal("null"); return null;
                    default:
                        if (s[i] == '-' || (s[i] >= '0' && s[i] <= '9')) return Number();
                        Fail("Invalid value"); return null;
                }
            }
            void Literal(string lit) { foreach (char c in lit) if (Next() != c) Fail("Invalid literal"); }
            object Object(int depth)
            {
                Expect('{'); var d = new Dictionary<string, object>(StringComparer.Ordinal);
                if (Take('}')) return d;
                while (true)
                {
                    Ws(); if (i >= s.Length || s[i] != '"') Fail("Expected property name");
                    string key = String(); Ws(); Expect(':');
                    object val = Value(depth);
                    if (d.ContainsKey(key)) Fail("Duplicate property " + key);
                    d.Add(key, val);
                    if (Take('}')) return d;
                    Expect(',');
                }
            }
            object Array(int depth)
            {
                Expect('['); var result = new List<object>();
                if (Take(']')) return result;
                while (true)
                {
                    result.Add(Value(depth));
                    if (Take(']')) return result;
                    Expect(',');
                }
            }
            string String()
            {
                Expect('"'); var b = new StringBuilder();
                while (true)
                {
                    char c = Next();
                    if (c == '"') return b.ToString();
                    if (c < 32) Fail("Unescaped control character");
                    if (c != '\\') { b.Append(c); continue; }
                    switch (Next())
                    {
                        case '"': b.Append('"'); break;
                        case '\\': b.Append('\\'); break;
                        case '/': b.Append('/'); break;
                        case 'n': b.Append('\n'); break;
                        case 'r': b.Append('\r'); break;
                        case 't': b.Append('\t'); break;
                        case 'b': b.Append('\b'); break;
                        case 'f': b.Append('\f'); break;
                        case 'u':
                            if (i + 4 > s.Length) Fail("Short Unicode escape");
                            ushort u;
                            if (!ushort.TryParse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out u)) Fail("Invalid Unicode escape");
                            b.Append((char)u); i += 4; break;
                        default: Fail("Invalid escape"); break;
                    }
                }
            }
            object Number()
            {
                int from = i;
                if (s[i] == '-') i++;
                if (i == s.Length) Fail("Invalid number");
                if (s[i] == '0') i++;
                else
                {
                    if (s[i] < '1' || s[i] > '9') Fail("Invalid integer");
                    while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
                }
                bool floating = false;
                if (i < s.Length && s[i] == '.')
                {
                    floating = true; i++; int fromDecimal = i;
                    while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
                    if (i == fromDecimal) Fail("Invalid decimal");
                }
                if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
                {
                    floating = true; i++;
                    if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
                    int fromExp = i;
                    while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
                    if (i == fromExp) Fail("Invalid exponent");
                }
                string token = s.Substring(from, i - from);
                long integral;
                if (!floating && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out integral)) return integral;
                double number;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out number) &&
                    !double.IsNaN(number) && !double.IsInfinity(number)) return number;
                Fail("Invalid number"); return null;
            }
        }
    }
}
