namespace Corsair.Abstractions;

/// <summary>
/// Marker interface representing any export specification for <typeparamref name="T"/>.
/// Implemented by both <see cref="IFixedSpec{T}"/> and <see cref="IDelimitedSpec{T}"/>.
/// </summary>
/// <typeparam name="T">The DTO type described by this spec.</typeparam>
public interface ISpec<T>
{
}
