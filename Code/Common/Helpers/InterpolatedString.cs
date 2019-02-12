using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla
{
    public class InterpolatedString
    {
        string _formatString;
        Segement[] _segments;

        public InterpolatedString(string formatString)
        {
            _formatString = formatString ?? throw new ArgumentNullException(nameof(formatString));

            _segments = new FormatStringParser().Parse(formatString);
        }

        public string FormatString => _formatString;

        public string Interpolate(object value)
        {
            StringBuilder text = new StringBuilder();

            foreach (var seg in _segments)
            {
                text.Append(seg.GetText(value));
            }

            return text.ToString();
        }

        private static ValueProvider CreateValueProvider(object value)
        {
            ValueProvider provider;
            if (value is IDictionary<string, object> dict)
                provider = new DictionaryValueProvider(dict);
            else if (value is NameValueCollection coll)
                provider = new DictionaryValueProvider(coll);
            else
                provider = new TypeDescriptorValueProvider(value);
            return provider;
        }

        private void EnsureInitialized()
        {
            if (_segments == null)
            {
                //List<Segement> segements = new List<Segement>(10);
                //char[] array = _format.ToCharArray();
                //int length = array.Length;
                //StringBuilder temp = new StringBuilder();
                //int type = 0;

                //for (int i = 0; i < length; i++)
                //{
                //    char ch = array[i];

                //    if (ch == '{')
                //    {
                //        if (i < length - 1 && array[i + 1] == ch)
                //        {
                //            i++;
                //        }
                //        else
                //        {
                //            if (temp.Length > 0)
                //                segements.Add()
                //        }
                //    }
                //}
            }
        }

        class FormatStringParser
        {
            const char NOMOREDATA = '\0';
            const int TYPE_LITERAL = 0, TYPE_EXPRESSION = 1;

            char[] _data;
            List<Segement> _segements;
            int _index, _length;
            StringBuilder _segement;
            int _type, _colon;

            public Segement[] Parse(string format)
            {
                _data = format.ToCharArray();
                _segements = new List<Segement>(10);
                _index = 0;
                _length = format.Length;
                _segement = new StringBuilder();
                _type = TYPE_LITERAL;
                _colon = -1;

                while (Parse()) ;

                FinishSegement();

                return _segements.ToArray();
            }


            private char Current
            {
                get
                {
                    if (_index < _length)
                        return _data[_index];
                    else
                        return NOMOREDATA;
                }
            }

            private char Next
            {
                get
                {
                    int i = _index + 1;

                    if (i < _length)
                        return _data[i];
                    else
                        return NOMOREDATA;
                }
            }

            private void FinishSegement()
            {
                if (_segement.Length > 0)
                {
                    if (_type == TYPE_LITERAL)
                    {
                        _segements.Add(new LiteralSegement(_segement.ToString()));
                    }
                    else
                    {
                        if (_colon > 0)
                        {
                            if (_colon < _segement.Length - 1)
                                _segements.Add(new ExpressionSegement(_segement.ToString(0, _colon), _segement.ToString(_colon + 1, _segement.Length - _colon - 1)));
                            else
                                _segements.Add(new ExpressionSegement(_segement.ToString(0, _colon)));

                        }
                        else
                            _segements.Add(new ExpressionSegement(_segement.ToString()));
                    }

                    _colon = -1;
                    _segement.Clear();
                }
            }

            private bool Parse()
            {
                var ch = Current;

                if (ch != NOMOREDATA)
                {
                    var skip = false;

                    if (ch == '{')
                    {
                        if (Next == ch)
                        {
                            _index++;
                        }
                        else if (_type == TYPE_LITERAL)
                        {
                            FinishSegement();
                            skip = true;
                            _type = TYPE_EXPRESSION;
                        }
                    }
                    else if (ch == '}')
                    {
                        if (Next == ch)
                        {
                            _index++;
                        }
                        else if (_type == TYPE_EXPRESSION)
                        {
                            FinishSegement();
                            skip = true;
                            _type = TYPE_LITERAL;
                        }
                    }

                    if (!skip)
                    {
                        if (ch == ':' && _type == TYPE_EXPRESSION && _colon == -1)
                        {
                            if (_segement.Length == 0)
                                ThrowUnexpectedToken(ch);

                            _colon = _segement.Length;
                        }

                        _segement.Append(ch);
                    }

                    return ++_index < _length;
                }
                else
                    return false;
            }

            private void ThrowUnexpectedToken(char token)
            {
                throw new FormatException("Unexpected token " + token);
            }
        }

        abstract class Segement
        {
            public abstract string GetText(object context);

        }

        class LiteralSegement : Segement
        {
            public LiteralSegement(string text)
            {
                Text = text;
            }

            public string Text { get; }

            public override string GetText(object context)
            {
                return Text;
            }
        }

        class ExpressionSegement : Segement
        {
            public ExpressionSegement(string expression)
                : this(expression, null)
            {

            }

            public ExpressionSegement(string expression, string format)
            {
                Expression = expression;
                Format = format;

            }

            public string Expression { get; }
            public string Format { get; }

            private object GetValue(object context, Queue<string> names)
            {
                if (context == null)
                    return null;

                string name = names.Dequeue();

                var provider = CreateValueProvider(context);

                object value = provider.GetValue(name);

                if (names.Count > 0)
                    return GetValue(value, names);
                else
                    return value;
            }

            public override string GetText(object context)
            {
                object value = GetValue(context, new Queue<string>(Expression.Split(Type.Delimiter)));

                if (value == null)
                    return string.Empty;

                string format = Format;

                if (!string.IsNullOrEmpty(format))
                {
                    if (value is IFormattable formattable)
                        return formattable.ToString(format, null);
                }

                return value.ToString();
            }
        }

        abstract class ValueProvider
        {
            public abstract object GetValue(string expression);
        }

        class DictionaryValueProvider : ValueProvider
        {
            private readonly IDictionary<string, object> _dictionary;

            public DictionaryValueProvider(NameValueCollection coll)
                : this(ToDictionary(coll))
            {

            }

            public DictionaryValueProvider(IDictionary<string, object> dictionary)
            {
                this._dictionary = dictionary;
            }

            public override object GetValue(string expression)
            {

                if (_dictionary != null && _dictionary.TryGetValue(expression, out object value))
                    return value;

                return null;
            }

            private static Dictionary<string, object> ToDictionary(NameValueCollection coll)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();

                foreach (string key in coll.AllKeys)
                {
                    dict.Add(key, coll[key]);
                }

                return dict;
            }
        }

        class TypeDescriptorValueProvider : ValueProvider
        {
            PropertyDescriptorCollection _properties;
            object _instance;

            public TypeDescriptorValueProvider(object value)
            {
                if (value != null)
                {
                    _properties = TypeDescriptor.GetProperties(value);
                    _instance = value;
                }
            }

            public override object GetValue(string expression)
            {
                if (_instance != null)
                {
                    var prop = _properties[expression];

                    if (prop == null)
                        throw new InvalidOperationException("Property " + expression + " not defined on " + _instance.GetType());

                    return prop.GetValue(_instance);
                }

                return null;
            }
        }
    }
}
