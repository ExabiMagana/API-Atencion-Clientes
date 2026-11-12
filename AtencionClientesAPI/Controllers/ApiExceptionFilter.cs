using AtencionClientesAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AtencionClientesAPI.Controllers;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        int codigo;
        string mensaje;

        switch (context.Exception)
        {
            case ReglaNegocioException regla:
                codigo = regla.CodigoHttp;
                mensaje = regla.Message;
                break;

            case DbUpdateConcurrencyException:
                codigo = StatusCodes.Status409Conflict;
                mensaje =
                    "Otro proceso modificó el registro. Consulta su estado y vuelve a intentarlo.";
                break;

            case DbUpdateException actualizacion
                when actualizacion.InnerException is SqlException sql
                    && (sql.Number == 2601 || sql.Number == 2627):
                codigo = StatusCodes.Status409Conflict;
                mensaje =
                    "La operación entra en conflicto con otro registro. " +
                    "Si estás asignando un turno, consulta nuevamente la disponibilidad del empleado.";
                break;

            default:
                return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = codigo,
            Title = "No se pudo completar la operación.",
            Detail = mensaje
        })
        {
            StatusCode = codigo
        };

        context.ExceptionHandled = true;
    }
}