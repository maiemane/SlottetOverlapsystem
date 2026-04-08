using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public sealed class CitizensController : ControllerBase
{
    private readonly ICreateCitizenService _createCitizenService;

    public CitizensController(ICreateCitizenService createCitizenService)
    {
        _createCitizenService = createCitizenService;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateCitizenResponse>> Create(
        [FromBody] CreateCitizenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createCitizenService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn, lejlighedsnummer, afdeling og trafiklys er påkrævet."),
                "DepartmentNotFound" => NotFound("Afdelingen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Citizen!.CitizenId },
            result.Citizen);
    }
}
