namespace Artifacts.Infrastructure;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}
