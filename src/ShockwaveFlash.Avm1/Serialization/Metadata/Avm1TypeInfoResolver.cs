using System.Collections.Generic;

namespace ShockwaveFlash.Avm1.Serialization.Metadata;

public static class Avm1TypeInfoResolver
{
    public static IAvm1TypeInfoResolver Combine(params IAvm1TypeInfoResolver?[] resolvers)
    {
        return new CombiningResolver(resolvers);
    }

    private sealed class CombiningResolver : IAvm1TypeInfoResolver
    {
        private readonly IAvm1TypeInfoResolver?[] _resolvers;

        public CombiningResolver(IAvm1TypeInfoResolver?[] resolvers)
        {
            _resolvers = resolvers;
        }

        public Avm1TypeInfo? GetTypeInfo(Type type, Avm1SerializerOptions options)
        {
            foreach (var resolver in _resolvers)
            {
                if (resolver?.GetTypeInfo(type, options) is { } info)
                    return info;
            }

            return null;
        }
    }
}
