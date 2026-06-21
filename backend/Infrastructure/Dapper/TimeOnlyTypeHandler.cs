using System.Data;
using Dapper;

namespace SnoopyAirlines.Infrastructure.Dapper
{
    public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value.ToTimeSpan();
        }

        public override TimeOnly Parse(object value)
        {
            return value switch
            {
                TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
                DateTime dateTime => TimeOnly.FromDateTime(dateTime),
                TimeOnly timeOnly => timeOnly,
                _ => throw new InvalidCastException(
                    $"No se pudo convertir el valor '{value}' ({value.GetType()}) a TimeOnly."),
            };
        }
    }
}