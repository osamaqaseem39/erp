namespace ERP.Api.Mappers;

public abstract class BaseMapper<TSource, TDestination> : IMapper<TSource, TDestination>
{
    public abstract TDestination Map(TSource source);
    public abstract TSource Map(TDestination destination);

    public virtual IEnumerable<TDestination> Map(IEnumerable<TSource> source)
    {
        return source.Select(Map);
    }

    public virtual IEnumerable<TSource> Map(IEnumerable<TDestination> destination)
    {
        return destination.Select(Map);
    }
}