using ParsWoW.Api.Infrastructure.Dbc.Engine;

namespace ParsWoW.Api.Application.Abstractions.Dbc;

/// <summary>
/// Non-generic schema contract usable by the generic loader. The
/// provider base registers <c>FileName → projector</c> from this
/// surface alone, so the loader no longer relies on hard-coded file
/// names or repeated registration calls.
/// </summary>
public interface IDbcSchema
{
    /// <summary>DBC file basename including extension, e.g. <c>Item.dbc</c>.</summary>
    string FileName { get; }

    /// <summary>If false, the file is optional and the API starts even if it is missing.</summary>
    bool Required { get; }

    /// <summary>Project a raw <see cref="DbcRecord"/> to the typed result.</summary>
    object Project(DbcRecord raw);
}

/// <summary>Strongly-typed schema contract used by application code.</summary>
public interface IDbcSchema<out TRecord> : IDbcSchema
{
    TRecord ProjectTyped(DbcRecord raw);
    object IDbcSchema.Project(DbcRecord raw) => ProjectTyped(raw)!;
}

/// <summary>
/// Convenience abstract base for schemas. Concrete subclasses supply
/// their projection logic only — file name and required bit come from
/// the class metadata.
/// </summary>
public abstract class DbcSchemaBase<TRecord> : IDbcSchema<TRecord>
{
    public abstract string FileName { get; }
    public virtual bool Required => true;
    public abstract TRecord ProjectTyped(DbcRecord raw);
}
