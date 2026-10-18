namespace FlyVip.Web.Areas.Admin.Crud;

public interface ICrudForm<in TEntity>
{
    void Load(TEntity entity);

    void Apply(TEntity entity);
}
