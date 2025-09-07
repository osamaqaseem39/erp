namespace ERP.Api.Mappers;

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
    TSource Map(TDestination destination);
    IEnumerable<TDestination> Map(IEnumerable<TSource> source);
    IEnumerable<TSource> Map(IEnumerable<TDestination> destination);
}