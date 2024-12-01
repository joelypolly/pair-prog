using Microsoft.AspNetCore.Mvc;
using PairProgrammingApi.DataAccess;

namespace PairProgrammingApi.Modules.Common;

/// <summary>
/// Creating an abstract class that will have the shared features of all module controllers
/// </summary>
public abstract class ModuleBase(PairContext dbContext)
{
    protected readonly PairContext DbContext =  dbContext;
}