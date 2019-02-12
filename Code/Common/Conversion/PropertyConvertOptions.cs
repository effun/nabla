using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Conversion
{
    public class PropertyConvertOptions
    {
        /// <summary>
        /// 指示是否忽略此属性，定义在目标属性上。
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 为目标属性定义一个转换器，用于将源模型的值转换为目标模型的值。
        /// </summary>
        public Action<PropertyConvertContext> Convert { get; set; }

        /// <summary>
        /// 属性的默认值，定义在目标属性上。
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// 为集合定义一个过滤器，该方法应定义在源模型的属性上，第一传入参数为源集合的项。
        /// </summary>
        public Func<object, PropertyConvertContext, bool> Filter { get; set; }

        /// <summary>
        /// 指定将目标属性影射到源模型的哪个属性上，定义在目标属性上。
        /// </summary>
        public string MapTo { get; set; }
    }
}
