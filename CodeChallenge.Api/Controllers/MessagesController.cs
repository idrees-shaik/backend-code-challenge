using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageLogic _logic;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageLogic logic, ILogger<MessagesController> logger)
    {
        _logic = logic;
        _logger = logger;
    }

    // GET: api/v1/organizations/{organizationId}/messages
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid organizationId)
    {
        var result = await _logic.GetAllMessagesAsync(organizationId); // use var here

        return result switch
        {
            Created<IEnumerable<Message>> created => Ok(created.Value),
            NotFound nf => NotFound(nf.Message),
            _ => BadRequest("Unknown error")
        };
    }

    // GET: api/v1/organizations/{organizationId}/messages/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid organizationId, Guid id)
    {
        var message = await _logic.GetMessageAsync(organizationId, id);
        if (message == null)
            return NotFound("Message not found");
        return Ok(message);
    }

    // POST: api/v1/organizations/{organizationId}/messages
    [HttpPost]
    public async Task<IActionResult> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        Result result = await _logic.CreateMessageAsync(organizationId, request);

        return result switch
        {
            Created<Message> created => CreatedAtAction(
                nameof(GetById),
                new { organizationId, id = created.Value.Id },
                created.Value),

            ValidationError ve => BadRequest(ve.Errors),
            Conflict c => Conflict(c.Message),
            _ => BadRequest("Unknown error")
        };
    }

    // PUT: api/v1/organizations/{organizationId}/messages/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        Result result = await _logic.UpdateMessageAsync(organizationId, id, request);

        return result switch
        {
            Updated => Ok(),
            ValidationError ve => BadRequest(ve.Errors),
            NotFound nf => NotFound(nf.Message),
            Conflict c => Conflict(c.Message),
            _ => BadRequest("Unknown error")
        };
    }

    // DELETE: api/v1/organizations/{organizationId}/messages/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid organizationId, Guid id)
    {
        Result result = await _logic.DeleteMessageAsync(organizationId, id);

        return result switch
        {
            Deleted => NoContent(),
            NotFound nf => NotFound(nf.Message),
            _ => BadRequest("Unknown error")
        };
    }
}
