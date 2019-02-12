using Nabla.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nabla.Linq
{
    internal static class SelectBuilder
    {

        public static IQueryable<TTarget> Build<TSource, TTarget>(IQueryable<TSource> source, ModelConvertOptions options)
        {
            if (options == null)
                options = ModelConvert.Target<TTarget>().Source<TSource>().Options.IgnoreReadOnly();

            options.IgnoreReadOnly();

            MemberBindingState state = new MemberBindingState(Expression.Parameter(typeof(TSource), "obj"));

            ModelConvert.WalkConvertiblProperties(null, ModelInfo.GetModelInfo(typeof(TTarget)), null, ModelInfo.GetModelInfo(typeof(TSource)), options, BuildProperty, state);

            var body = Expression.MemberInit(Expression.New(typeof(TTarget)), state.Bindings);

            var lambda = Expression.Lambda<Func<TSource, TTarget>>(body, state.Parameter);

            return source.Select(lambda);
        }

        private static void BuildProperty(ModelPropertyInfo mp1, PropertyConvertContext context, object foo)
        {
            if (!mp1.IsReadOnly)
            {
                MemberBindingState state = (MemberBindingState)foo;
                Expression member = state.Parameter;

                foreach (var mp0 in context.SourcePropertyChain)
                {
                    member = Expression.MakeMemberAccess(member, mp0.PropertyInfo);
                }

                state.Bindings.Add(Expression.Bind(mp1.PropertyInfo, member));
            }
        }

        class MemberBindingState
        {
            public MemberBindingState(ParameterExpression parameter)
            {
                Parameter = parameter;
                Bindings = new List<MemberBinding>();
            }

            public ParameterExpression Parameter { get; set; }

            public List<MemberBinding> Bindings { get; set; }
        }
    }
}
