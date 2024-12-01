using PairProgrammingApi.DataAccess;

namespace PairProgrammingApi.Modules.HealthCheck.Models;

public class HealthCheckDbModel: EntityBase
{
    public bool CheckSuccessful { get; set; }
}