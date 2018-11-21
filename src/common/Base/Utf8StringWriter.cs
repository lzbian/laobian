using System.IO;
using System.Text;

namespace Laobian.Common.Base
{
    /// <summary>
    /// <see cref="StringWriter"/> in UTF8 encoding
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
