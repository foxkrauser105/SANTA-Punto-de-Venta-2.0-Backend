using System.Reflection;

namespace SANTA.PoS.Business.Helpers;

/// <summary>
/// Helper class for dynamically updating entity properties from DTO objects.
/// Provides flexible property update mechanisms with control over which properties are updated and how null values are handled.
/// </summary>
public static class PropertyUpdateHelper
{
    /// <summary>
    /// Updates an entity with non-null properties from a DTO.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to update</typeparam>
    /// <typeparam name="TDto">The DTO type containing the update values</typeparam>
    /// <param name="entity">The entity instance to update</param>
    /// <param name="dto">The DTO containing the new values</param>
    /// <param name="allowNullValues">If true, allows null values to override entity properties. Default is false.</param>
    /// <exception cref="ArgumentNullException">Thrown if entity or dto is null</exception>
    public static void UpdateEntityFromDto<TEntity, TDto>(TEntity entity, TDto dto)
        where TEntity : class
        where TDto : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);

        var dtoProperties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        var entityProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var dtoProperty in dtoProperties)
        {
            var dtoValue = dtoProperty.GetValue(dto);

            // Skip null values if allowNullValues is false
            if (dtoValue is null)
                continue;

            var entityProperty = entityProperties.FirstOrDefault(p =>
                p.Name.Equals(dtoProperty.Name, StringComparison.OrdinalIgnoreCase) &&
                p.CanWrite);

            if (entityProperty is null)
                continue;

            try
            {
                // Get the underlying types to handle nullable types properly
                var dtoPropertyType = Nullable.GetUnderlyingType(dtoProperty.PropertyType) ?? dtoProperty.PropertyType;
                var entityPropertyType = Nullable.GetUnderlyingType(entityProperty.PropertyType) ?? entityProperty.PropertyType;

                // Check if the value can be assigned (handles type compatibility and nullable types)
                if (entityPropertyType.IsAssignableFrom(dtoPropertyType))
                {
                    entityProperty.SetValue(entity, dtoValue);
                }
            }
            catch (TargetInvocationException)
            {
                // Property setter threw an exception, skip this property
                continue;
            }
        }
    }

    /// <summary>
    /// Updates an entity with only the specified properties from a DTO, including null values.
    /// This method only updates properties that are explicitly listed, allowing you to control exactly which fields are modified.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to update</typeparam>
    /// <typeparam name="TDto">The DTO type containing the update values</typeparam>
    /// <param name="entity">The entity instance to update</param>
    /// <param name="dto">The DTO containing the new values</param>
    /// <param name="propertyNames">The names of the properties to update (case-insensitive). Only these properties will be updated.</param>
    /// <exception cref="ArgumentNullException">Thrown if entity, dto, or propertyNames is null</exception>
    public static void UpdateEntityFromDtoSelective<TEntity, TDto>(TEntity entity, TDto dto, params string[] propertyNames)
        where TEntity : class
        where TDto : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(propertyNames);

        var dtoProperties = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
        var entityProperties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

        foreach (var propertyName in propertyNames)
        {
            var dtoProperty = dtoProperties.FirstOrDefault(p =>
                p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (dtoProperty is null)
                continue;

            var dtoValue = dtoProperty.GetValue(dto);

            var entityProperty = entityProperties.FirstOrDefault(p =>
                p.Name.Equals(dtoProperty.Name, StringComparison.OrdinalIgnoreCase) &&
                p.CanWrite);

            if (entityProperty is null)
                continue;

            try
            {
                // Get the underlying types to handle nullable types properly
                var dtoPropertyType = Nullable.GetUnderlyingType(dtoProperty.PropertyType) ?? dtoProperty.PropertyType;
                var entityPropertyType = Nullable.GetUnderlyingType(entityProperty.PropertyType) ?? entityProperty.PropertyType;

                // Check if the value can be assigned (handles type compatibility and nullable types)
                if (entityPropertyType.IsAssignableFrom(dtoPropertyType))
                {
                    entityProperty.SetValue(entity, dtoValue);
                }
            }
            catch (TargetInvocationException)
            {
                // Property setter threw an exception, skip this property
                continue;
            }
        }
    }
}
