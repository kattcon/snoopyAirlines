using System.Data;
using Dapper;

namespace SnoopyAirlines.Infrastructure.Dapper
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                DateOnly dateOnly => dateOnly,
                _ => throw new InvalidCastException(
                    $"No se pudo convertir el valor '{value}' ({value.GetType()}) a DateOnly."),
            };
        }
    }
}